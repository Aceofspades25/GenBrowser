Public Class FixLineLength

    Private Sub btnFix_Click(sender As Object, e As RoutedEventArgs) Handles btnFix.Click
        Dim res As String = Me.tbxString.Text.Replace(vbCr, "").Replace(vbLf, "")
        Dim length = res.Length

        Dim inserts As Integer = 0
        For i As Integer = 1 To length - 2
            If i Mod Me.tbxLength.Text = 0 Then
                res = res.Insert(i + (inserts * 2), vbCrLf)
                inserts += 1
            End If
        Next i

        Me.tbxString.Text = res
        Clipboard.SetText(res)
    End Sub

    Private Sub btnScan_Click(sender As Object, e As RoutedEventArgs) Handles btnScan.Click
        Dim res As String = Me.tbxString.Text
        Dim IllegalChars As Boolean = False
        For Each chr As Char In res
            If chr <> "A" And chr <> "G" And chr <> "C" And chr <> "T" And chr <> "N" And chr <> "-" And chr <> "a" And chr <> "g" And chr <> "c" And chr <> "t" And chr <> "n" Then
                IllegalChars = True
            End If
        Next
        If IllegalChars Then
            MessageBox.Show("Illegal characters found")
        Else
            MessageBox.Show("Clean")
        End If
    End Sub
End Class
