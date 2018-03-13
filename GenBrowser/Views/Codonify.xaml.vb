Public Class Codonify

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim strIn As String = tbxInput.Text.Replace(vbCr, "").Replace(vbLf, "")
        Dim strOut As String = ""
        For i As Integer = 1 To strIn.Length
            strOut &= strIn(i - 1)
            If i Mod 30 = 0 Then
                strOut &= vbCrLf
            ElseIf i Mod 3 = 0 Then
                strOut &= " "
            End If
        Next i
        Me.tbxOutput.Text = strOut
        Clipboard.SetText(strOut)
    End Sub
End Class
