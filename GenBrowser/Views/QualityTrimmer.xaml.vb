Public Class QualityTrimmer
    Public Property Header As String
    Public Property FASTA As String
    Public Property QualityList As List(Of Integer)
    Public Property StartPos As Nullable(Of Integer)
    Public Property EndPos As Nullable(Of Integer)

    Public ReadOnly Property MinScore As Integer
        Get
            If Not IsNumeric(Me.tbxMinScore.Text) Then
                Me.tbxMinScore.Text = 20
            End If
            Return Me.tbxMinScore.Text
        End Get
    End Property

    Public ReadOnly Property ConsecutiveFor As Integer
        Get
            If Not IsNumeric(Me.tbxConsecutiveFor.Text) Then
                Me.tbxConsecutiveFor.Text = 10
            End If
            Return Me.tbxConsecutiveFor.Text
        End Get
    End Property

    Private Sub ProcessFASTA()
        Dim sr As New System.IO.StringReader(Me.tbxFASTA.Text)
        Me.Header = sr.ReadLine()
        If Not Me.Header.StartsWith(">") Then
            Throw New Exception("Please enter a valid FASTA file format (must start with >)")
        End If
        Me.FASTA = sr.ReadToEnd().Replace(vbCr, "").Replace(vbLf, "")
    End Sub

    Private Function NextAverage(ByVal index As Integer) As Double
        ' Start at index and Read up to Me.ConsecutiveFor Quality scores and then Average these scores together and return the result
        Dim sum As Double = 0
        Dim count As Integer = 0
        For i As Integer = 0 To Me.ConsecutiveFor - 1
            sum += Me.QualityList(index + i)
            count += 1
            If index + i > Me.QualityList.Count - 1 Then
                Exit For
            End If
        Next i
        Return (sum / count)
    End Function

    Private Sub ProcessQualityData()
        Dim sr As New System.IO.StringReader(Me.tbxQuality.Text)
        Dim header As String = sr.ReadLine()
        If Not header.StartsWith(">") Then
            Throw New Exception("Please enter a valid Quality file format (must start with >)")
        End If
        Dim strQuality As String = sr.ReadToEnd().Replace(vbCr, " ").Replace(vbLf, " ").Replace("  ", " ")
        Dim stringSeparators() As String = {" "}
        Me.QualityList = strQuality.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(Function(str) Int32.Parse(str))

        For i As Integer = 0 To Me.QualityList.Count - 1
            If Me.StartPos Is Nothing And Me.QualityList(i) > Me.MinScore AndAlso NextAverage(i) > Me.MinScore Then
                Me.StartPos = i
            End If

            If Not Me.StartPos Is Nothing And Me.EndPos Is Nothing And Me.QualityList(i) < Me.MinScore AndAlso NextAverage(i) < Me.MinScore Then
                Me.EndPos = i - 1
                Exit For
            End If
        Next i

        If Me.EndPos Is Nothing Then Me.EndPos = Me.QualityList.Count - 1
    End Sub

    Private Sub TruncateFASTA()
        Dim truncFASTA As String = ""
        For i As Integer = Me.StartPos To Me.EndPos
            truncFASTA &= Me.FASTA(i)

            If (i - Me.StartPos + 1) Mod 60 = 0 Then
                truncFASTA &= vbCrLf
            End If
        Next i

        Me.FASTA = truncFASTA
    End Sub

    Private Sub btnExecute_Click(sender As Object, e As RoutedEventArgs) Handles btnExecute.Click
        Try
            ' Step 1: Process the FASTA into a string without line breaks and capture the header
            ProcessFASTA()
            ' Step 2: Read the quality data
            ProcessQualityData()
            ' Step 3: Truncate FASTA to quality region
            TruncateFASTA()

            Dim strResult = Me.Header & vbCrLf & Me.FASTA
            Me.tbxResult.Text = strResult
            Clipboard.SetText(strResult)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
End Class
