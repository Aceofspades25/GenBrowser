Public Class CleanString

    Private Sub btnClean_Click(sender As Object, e As RoutedEventArgs) Handles btnClean.Click
        Dim input As String = Me.tbxString.Text '.Replace(vbCr, "").Replace(vbLf, "")
        Dim result As String = ""
        For Each chr As Char In input
            If chr = "A" Or chr = "C" Or chr = "G" Or chr = "T" Or chr = "N" Or chr = "-" Or chr = "a" Or chr = "c" Or chr = "g" Or chr = "t" Or chr = "n" Then
                result &= chr
            End If
        Next chr
        Me.tbxString.Text = result
        Clipboard.SetText(result)
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

        'Dim c As Char = "	"
        'c.
    End Sub
End Class
