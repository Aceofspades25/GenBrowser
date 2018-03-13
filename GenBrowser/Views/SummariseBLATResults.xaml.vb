Public Class SummariseBLATResults

    Public Class ResultLine
        Public Property SequenceName As String
        Public Property InputRange As String
        Public Property InputStrand As Char

        Public Property Score As Integer = 0
        Public Property InputStart As Integer
        Public Property InputEnd As Integer
        Public Property InputSize As Integer
        Public Property Identity As Double
        Public Property TargetChromosome As String
        Public Property TargetStrand As Char
        Public Property TargetStart As Integer
        Public Property TargetEnd As Integer
        Public Property TargetSize As Integer
    End Class

    Private Results As List(Of ResultLine)

    Private Sub btnRun_Click(sender As Object, e As RoutedEventArgs) Handles btnRun.Click
        Results = New List(Of ResultLine)
        ' Step 1: Open the query file and read all of the query names and positions into a table
        Dim srI As New IO.StreamReader(Me.tbxQueries.Text)
        While Not srI.EndOfStream
            Dim strLine As String = srI.ReadLine()
            If strLine.StartsWith(">") Then
                ' A new sequence header
                Dim Properties As String() = strLine.Split(" "c)
                Dim rl As New ResultLine()
                For Each strProperty As String In Properties
                    If strProperty.StartsWith(">"c) Then
                        rl.SequenceName = strProperty.TrimStart(">"c)
                    End If
                    If strProperty.StartsWith("range=") Then
                        rl.InputRange = strProperty.Split("="c)(1)
                    End If
                    If strProperty.StartsWith("strand=") Then
                        rl.InputStrand = strProperty.Split("="c)(1)
                    End If
                Next
                Me.Results.Add(rl)
            End If ' If this is a sequence header
        End While ' Not EOF
        srI.Close()

        ' Step 2: Read the results file and map these onto the appropriate inputs
        Dim srO As New IO.StreamReader(Me.tbxResults.Text)
        While Not srO.EndOfStream
            Dim strLine As String = srO.ReadLine()
            If strLine.IndexOf("details</A> ") <> -1 Then
                ' Remove all the links at the beginning of the result line
                strLine = strLine.Split({"details</A> "}, StringSplitOptions.None)(1)
                Dim Properties As String() = strLine.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
                ' Now find the line with this name
                Dim rl As ResultLine = Results.FirstOrDefault(Function(f) f.SequenceName = Properties(0))
                If Not rl Is Nothing Then
                    If rl.Score < Properties(1) Then
                        rl.Score = Properties(1)
                        rl.InputStart = Properties(2)
                        rl.InputEnd = Properties(3)
                        rl.InputSize = Properties(4)
                        rl.Identity = Properties(5).Trim("%"c)
                        rl.TargetChromosome = Properties(6)
                        rl.TargetStrand = Properties(7)
                        rl.TargetStart = Properties(8)
                        rl.TargetEnd = Properties(9)
                        rl.TargetSize = Properties(10)
                    End If
                End If
            End If  ' This is a result line
        End While ' Not EOF
        srO.Close()

        Dim sw As New IO.StreamWriter(Me.tbxOutput.Text)
        sw.WriteLine("sep=,")
        sw.WriteLine("SequenceName,InputRange,InputStrand,Score,InputStart,InputEnd,InputSize,Identity," & _
                     "TargetChromosome,TargetStrand,TargetStart,TargetEnd,TargetSize")
        For Each result As ResultLine In Results
            sw.WriteLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", _
            result.SequenceName, result.InputRange, result.InputStrand, result.Score, result.InputStart, _
            result.InputEnd, result.InputSize, result.Identity, result.TargetChromosome, result.TargetStrand, _
            result.TargetStart, result.TargetEnd, result.TargetSize))
        Next
        sw.Close()
    End Sub

    Private Sub SummariseBLATResults_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Me.tbxQueries.Text = My.Application.Info.DirectoryPath & "\Set1.txt"
        Me.tbxResults.Text = My.Application.Info.DirectoryPath & "\BLAT Results - Set1.html"
        Me.tbxOutput.Text = My.Application.Info.DirectoryPath & "\Set1Summary.csv"
    End Sub
End Class
