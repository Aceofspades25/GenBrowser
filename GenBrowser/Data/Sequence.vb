Imports GenBrowser.Common
Imports System.IO

Public Class Sequence
    Public Property SpeciesID As Integer
    Public Property SpeciesName As String
    Public Property OtherData As String
    Public Property Sequence As List(Of Char)

    Private _StartIndex As Integer? = Nothing
    Public ReadOnly Property SequenceStartIndex As Integer
        Get
            If _StartIndex Is Nothing Then
                For i As Integer = 0 To Sequence.Count - 1
                    If Sequence(i) <> "N"c And Sequence(i) <> "-"c Then
                        Return i
                    End If
                Next i
            End If
            Return _StartIndex
        End Get
    End Property

    Private _EndIndex As Integer? = Nothing
    Public ReadOnly Property SequenceEndIndex As Integer
        Get
            If _EndIndex Is Nothing Then
                RecauculateSequenceEndIndex()
            End If
            Return _EndIndex
        End Get
    End Property

    Private Sub RecauculateSequenceEndIndex()
        For i As Integer = Sequence.Count - 1 To 0 Step -1
            If Sequence(i) <> "N"c And Sequence(i) <> "-"c Then
                _EndIndex = i
                Return
            End If
        Next i
    End Sub

    Public Sub ReadSequence(ByVal tr As TextReader)
        Dim HeaderLine As String = tr.ReadLine()
        Me.SpeciesName = HeaderLine.Split(" "c)(0).Trim(">"c)
        Me.OtherData = HeaderLine.Substring(Me.SpeciesName.Length + 1).Trim
        If Me.SpeciesName.IndexOf("|"c) <> -1 Then
            Me.SpeciesName = Me.SpeciesName.Split("|"c)(1)
        End If
        Dim chr As Char = ChrW(tr.Peek())
        While chr <> ">"c
            Dim NewLine As String = tr.ReadLine()
            If NewLine Is Nothing Then Return
            Sequence.AddRange(NewLine)
            chr = ChrW(tr.Peek())
        End While
    End Sub

    Public Sub New(ByVal SpeciesID As Integer)
        Me.SpeciesID = SpeciesID
        Sequence = New List(Of Char)
    End Sub

    Public Function Search(ByVal MatchingSequence As String) As Integer?
        For i As Integer = 0 To Me.Sequence.Count - 1
            For j = 0 To MatchingSequence.Length - 1
                If i + j >= Me.Sequence.Count - 1 Then Return Nothing
                If Me.Sequence(i + j) <> MatchingSequence(j) Then
                    Exit For ' Match has failed: Skip to the next i
                End If
                If j = MatchingSequence.Length - 1 Then
                    ' A complete match has been made when j has reached the end of the matching sequence 
                    Return i + 1 ' We return i+1 because our positions are counted from 1
                End If
            Next j
        Next i
        Return Nothing
    End Function

    Public Function IsTheEnd(ByVal s1 As List(Of Char), ByVal s2 As List(Of Char), ByVal index As Integer) As Boolean
        Dim end1 As Boolean = True
        Dim end2 As Boolean = True
        For i As Integer = index To s1.Count - 1
            If IsANucleotide(s1(i)) Then
                end1 = False
            End If
        Next i
        For i As Integer = index To s2.Count - 1
            If IsANucleotide(s2(i)) Then
                end2 = False
            End If
        Next i
        Return end1 Or end2
    End Function

    Public Function CompareTo(ByVal s As Sequence) As String
        Dim MinPosition As Integer = Math.Min(Me.Sequence.Count, s.Sequence.Count)
        Dim absDifferences As Integer = 0
        Dim positions As Integer = 0
        Dim SNPs As Integer = 0
        Dim completePositions As Integer = 0
        Dim prevLetter1 As Char = ""
        Dim prevLetter2 As Char = ""
        Dim indels1 As Integer = 0
        Dim indels2 As Integer = 0
        For i As Integer = 0 To MinPosition - 1
            If Me.Sequence(i) <> "N" And s.Sequence(i) <> "N" Then
                positions += 1
                If Me.Sequence(i) <> "-" And s.Sequence(i) <> "-" Then
                    completePositions += 1
                End If

                If Me.Sequence(i) <> s.Sequence(i) Then
                    absDifferences += 1
                    If Me.Sequence(i) <> "-" And s.Sequence(i) <> "-" Then
                        SNPs += 1
                    End If
                End If
                ' Try and count indels...
                If (Me.Sequence(i) = "-" And IsANucleotide(prevLetter1)) Or (s.Sequence(i) = "-" And IsANucleotide(prevLetter2)) Then
                    If IsANucleotide(prevLetter1) And IsANucleotide(prevLetter2) And Me.Sequence(i) = "-" And s.Sequence(i) = "-" Then
                        ' If they were both nucleotides before and they are both "-" now, don't count this
                        ' Do nothing
                    ElseIf IsTheEnd(Me.Sequence, s.Sequence, i) Then
                        ' This is the end of either sequence: Don't count this
                    Else
                        If Me.Sequence(i) = "-" Then indels1 += 1 Else indels2 += 1
                    End If
                ElseIf (Me.Sequence(i) = "-" And IsANucleotide(s.Sequence(i)) And prevLetter2 = "-") Or _
                    (s.Sequence(i) = "-" And IsANucleotide(Me.Sequence(i)) And prevLetter1 = "-") Then
                    ' Special case: An uneven ending (count this)
                    If Me.Sequence(i) = "-" Then indels1 += 1 Else indels2 += 1
                End If

            End If
            prevLetter1 = Me.Sequence(i)
            prevLetter2 = s.Sequence(i)
        Next i
        Return String.Format("Pair length:                                  {0}" & vbCrLf & _
                             "Positions (excluding Ns):         {1}" & vbCrLf & _
                             "Complete positions (no gaps): {2}" & vbCrLf & _
                             "Absolute difference:                  {3} ({4}%)" & vbCrLf & _
                             "SNPs:                                             {5} ({6}% of complete)" & vbCrLf & _
                             "Indels:                                           {7} ({8} + {9}) ({10}% of complete)" & vbCrLf & _
                             "Total mutations:                          {11} ({12}% of complete)" & vbCrLf & _
                             "Identity:                                         {13}%", _
                             MinPosition, positions, completePositions, _
                             absDifferences, (absDifferences / positions * 100).ToString("0.00"), _
                             SNPs, (SNPs / completePositions * 100).ToString("0.00"),
                             indels1 + indels2, indels1, indels2, ((indels1 + indels2) / completePositions * 100).ToString("0.00"),
                             indels1 + indels2 + SNPs, ((indels1 + indels2 + SNPs) / completePositions * 100).ToString("0.00"),
                             ((1.0 - ((indels1 + indels2 + SNPs) / completePositions)) * 100).ToString("0.00"))
    End Function

    Public Sub InsertGaps(ByVal PositionIndex As Integer, ByVal GapCount As Integer)
        Me.Sequence.InsertRange(PositionIndex - 1, Enumerable.Repeat("-"c, GapCount))
        RecauculateSequenceEndIndex()
    End Sub

End Class
