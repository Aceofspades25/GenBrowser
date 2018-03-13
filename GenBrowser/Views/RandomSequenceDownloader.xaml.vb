Imports System.Net

Class RandomSequenceDownloader

    Public Enum Species
        panTro4
        hg38
    End Enum

    Public Enum Sources
        UCSC
    End Enum

    Public Enum Chromosomes
        chr1
        chr2
        chr2A
        chr2B
        chr3
        chr4
        chr5
    End Enum

    Private Sub SetupPage()
        Me.ddlSpecies.ItemsSource = [Enum].GetValues(GetType(RandomSequenceDownloader.Species))
        Me.ddlSpecies.SelectedIndex = 0

        Me.ddlSource.ItemsSource = [Enum].GetValues(GetType(RandomSequenceDownloader.Sources))
        Me.ddlSource.SelectedIndex = 0

        Me.ddlChromosome.ItemsSource = [Enum].GetValues(GetType(RandomSequenceDownloader.Chromosomes))
        Me.ddlChromosome.SelectedIndex = 0

        Me.tbxFileName.Text = My.Application.Info.DirectoryPath & "\Set1.txt"
    End Sub

    Private Sub RandomSequenceDownloader_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SetupPage()
    End Sub

    Private Sub TestReadResult()
        Dim sr As New IO.StreamReader("C:\Steve\Genes\HC similarity\SampleResult.txt")
        Dim str As String = sr.ReadToEnd()
        sr.Close()

        str = str.Split({"<PRE>"}, StringSplitOptions.None)(1)
        str = str.Split({"</PRE>"}, StringSplitOptions.None)(0)
        str = str.Trim()

        str = str.Insert(1, "hg38_chr1_seq1 ")

        ' Reject anything with 2 or more consecutive Ns
        If str.IndexOf("NN") <> -1 Then
            MessageBox.Show("Rejected")
        End If


        Dim i As Integer = 0
    End Sub

    Private Sub btnRun_Click(sender As Object, e As RoutedEventArgs) Handles btnRun.Click    
        Dim sw As New IO.StreamWriter(Me.tbxFileName.Text, False)

        Dim webClient As New System.Net.WebClient
        webClient.Headers.Add(HttpRequestHeader.Cookie, "hguid=506244969_qw4AtrfwMayOzfbGQaxxoHaSgSNY")

        Dim r As New Random()

        Dim i As Integer = 1
        While i <= Me.tbxCount.Text
            ' Step 1: Pick a random location
            Dim startPos As Integer = r.Next(Me.tbxFrom.Text, Me.tbxTo.Text - Me.tbxLength.Text - 1)
            Dim endPos As Integer = startPos + Me.tbxLength.Text - 1
            Dim posString As String = Me.ddlChromosome.Text & ":" & startPos.ToString("0,0") & "-" & endPos.ToString("0,0")



            Dim strRequest As String = String.Format("http://genome.ucsc.edu/cgi-bin/hgc?g=htcGetDna2&getDnaPos={0}&db={1}&hgSeq.casing=upper&hgSeq.repMasking=lower&submit=get+DNA", WebUtility.UrlEncode(posString), Me.ddlSpecies.Text)
            Dim result As String = webClient.DownloadString(strRequest)
            ' Isolate the sequence from the returned HTML
            result = result.Split({"<PRE>"}, StringSplitOptions.None)(1)
            result = result.Split({"</PRE>"}, StringSplitOptions.None)(0)
            result = result.Trim()
            ' Name the sequence
            result = result.Insert(1, String.Format("{0}_{1}_seq{2} ", Me.ddlSpecies.Text, Me.ddlChromosome.Text, Me.tbxStartIndex.Text + i))

            ' Discard NNNNs
            If result.IndexOf("NN") = -1 Then
                ' Only record and increment if it doesn't contain NN (uppercase)
                sw.Write(result & vbCrLf & vbCrLf)
                i += 1
            End If
        End While

        sw.Close()
    End Sub
End Class
