Public Class Settings
    Public Shared Ports As New List(Of Integer)
    Public Shared KEY As String = "<AsyncRAT123>"
    Public Shared ReadOnly VER As String = "AsyncRAT v1.9"
    Public Shared Online As New List(Of Client)
    Public Shared Blocked As New List(Of String)
    Public Shared Sent As Integer = 0
    Public Shared Received As Integer = 0
End Class
