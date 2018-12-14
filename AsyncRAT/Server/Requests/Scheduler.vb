
'       │ Author     : NYAN CAT
'       │ Name       : Task scheduler

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program is distributed for educational purposes only.


Public Class Incoming_Requests
    Public C As Client
    Public B As Byte()

    Sub New(ByVal C_ As Client, B_ As Byte())
        C = C_
        B = B_
    End Sub

End Class

Public Class Outcoming_Requests
    Public C As Client
    Public B As Byte()

    Sub New(ByVal C_ As Client, B_ As Byte())
        C = C_
        B = B_
    End Sub

End Class



Public Class Pending
    Public Shared Req_In As List(Of Incoming_Requests)
    Public Shared Sub Incoming()
        While True
            Threading.Thread.Sleep(1)
            Try
                Dim ClientReq As Incoming_Requests = Nothing
                If Req_In.Count > 0 Then
                    ClientReq = Req_In.Item(0)
                    Req_In.Remove(ClientReq)
                    Messages.Read(ClientReq.C, ClientReq.B)
                End If
            Catch ex As Exception
                Debug.WriteLine("Incoming " + ex.Message)
            End Try
        End While
    End Sub

    Public Shared Req_Out As List(Of Outcoming_Requests)
    Public Shared Sub OutComing()
        While True
            Threading.Thread.Sleep(1)
            Try
                Dim ClientReq As Outcoming_Requests = Nothing
                If Req_Out.Count > 0 Then
                    ClientReq = Req_Out.Item(0)
                    Req_Out.Remove(ClientReq)
                    ClientReq.C.BeginSend(ClientReq.B)
                End If
            Catch ex As Exception
                Debug.WriteLine("OutComing " + ex.Message)
            End Try
        End While
    End Sub
End Class