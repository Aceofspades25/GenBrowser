Public Class SequenceInverter

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim strIn As String = tbxInput.Text.Replace(vbCr, "").Replace(vbLf, "")
        Dim strOut As String = ""
        Dim count As Integer = 1
        For i As Integer = strIn.Length - 1 To 0 Step -1
            strOut &= New Nucleobase(strIn(i).ToString.ToUpper(), 0).ReturnCompliment()
            If count Mod 60 = 0 Then strOut &= vbCrLf
            count += 1
        Next i
        Me.tbxOutput.Text = strOut
        Clipboard.SetText(strOut)
    End Sub
End Class
