Imports System.ComponentModel

Public Class MasterRecord
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

#Region " Properties "

    Public Property FileName As String
    Public Property Sequences As List(Of Sequence)
    Public Property Positions As List(Of Position) ' Currently only used for WrapViewer
    Public Property PositionRows As List(Of PositionRow)

    Private _ExonSequence As Sequence = Nothing
    Private _HasExonSequence As Nullable(Of Boolean) = Nothing
    Public ReadOnly Property ExonSequence As Sequence
        Get
            If _HasExonSequence Is Nothing Then
                For Each s In Me.Sequences
                    If s.SpeciesName = "Exons" Then
                        _HasExonSequence = True
                        _ExonSequence = s
                        Return s
                    End If
                Next s
                _HasExonSequence = False
                Return Nothing
            Else
                Return _ExonSequence
            End If
        End Get
    End Property

    Public ReadOnly Property PairingsFileName As String
        Get
            Dim strExt As String = Me.FileName.Split(".")(Me.FileName.Split(".").Length - 1)
            Return Me.FileName.Replace("." & strExt, " - pairings.txt")
        End Get
    End Property

    ' Specified positions are those entered which will be used to crop the data the next time it is updated
    Private _SpecifiedStartPosition As Integer?
    Public Property SpecifiedStartPosition As Integer?
        Get
            Return _SpecifiedStartPosition
        End Get
        Set(value As Integer?)
            'If value Is Nothing Then value = 1
            _SpecifiedStartPosition = value
            NotifyPropertyChanged("SpecifiedStartPosition")
            NotifyPropertyChanged("SpecifiedPositionCount")
        End Set
    End Property
    Private _SpecifiedEndPosition As Integer?
    Public Property SpecifiedEndPosition As Integer?
        Get
            Return _SpecifiedEndPosition
        End Get
        Set(value As Integer?)
            _SpecifiedEndPosition = value
            NotifyPropertyChanged("SpecifiedEndPosition")
            NotifyPropertyChanged("SpecifiedPositionCount")
        End Set
    End Property
    Public Property SpecifiedPositionCount As Integer?
        Get
            If Me.SpecifiedEndPosition Is Nothing Then Return Nothing
            Return Me.SpecifiedEndPosition - Me.SpecifiedStartPosition + 1
        End Get
        Set(value As Integer?)
            ' setting this, tweaks the end position
            If value Is Nothing Then
                Me.SpecifiedEndPosition = Nothing
            Else
                Me.SpecifiedEndPosition = Me.SpecifiedStartPosition + value - 1
            End If
            NotifyPropertyChanged("SpecifiedEndPosition")
            NotifyPropertyChanged("SpecifiedPositionCount")
        End Set
    End Property

    ' Display positions are those currently rendered
    Public Property DisplayStartPosition As Integer?
    Public Property DisplayEndPosition As Integer?
    Public ReadOnly Property DisplayPositionCount As Integer?
        Get
            If Me.DisplayEndPosition Is Nothing Then Return Nothing
            Return Me.DisplayEndPosition - Me.DisplayStartPosition + 1
        End Get
    End Property

    ' FilePositionCount is the total number of positions within the file
    Private _FilePositionCount As Integer
    Public ReadOnly Property FilePositionCount As Integer
        Get
            If _FilePositionCount = 0 Then
                _FilePositionCount = Me.Sequences.Max(Function(f) f.Sequence.Count)
            End If
            Return _FilePositionCount
        End Get
    End Property

    Public Property HighlightCount As Integer
    Public Property DimCount As Integer

#End Region

    Private Sub SetHighlighting(ByVal pos As Position)
        Dim firstBasePair As Nucleobase = pos.Species(0)
        If Settings.HighlightBasePairs Then
            For i As Integer = 0 To pos.Species.Count - 1
                If pos.Species(i).Base <> firstBasePair.Base Then
                    ' Highlight any difference
                    pos.Highlight = True
                    Return
                End If
            Next i
        Else
            ' No highlighting
            pos.Highlight = True
        End If
    End Sub

    Public Sub PivotData()
        Dim TrimOptions = Settings.TrimmingOptions
        If TrimOptions = TrimmingOptions.AtLeastTwo And Me.Sequences.Count = 1 Then
            TrimOptions = TrimmingOptions.AllSpecies
        End If

        Select Case TrimOptions
            Case TrimmingOptions.None
                If SpecifiedStartPosition Is Nothing Then SpecifiedStartPosition = 1
                If SpecifiedEndPosition Is Nothing Then SpecifiedEndPosition = Me.FilePositionCount
            Case TrimmingOptions.AtLeastTwo
                If SpecifiedStartPosition Is Nothing Then SpecifiedStartPosition = Me.Sequences.OrderBy(Function(f) f.SequenceStartIndex)(1).SequenceStartIndex + 1
                If SpecifiedEndPosition Is Nothing Then SpecifiedEndPosition = Me.Sequences.OrderBy(Function(f) f.SequenceEndIndex)(Me.Sequences.Count - 2).SequenceEndIndex + 1
            Case TrimmingOptions.AllSpecies
                If SpecifiedStartPosition Is Nothing Then SpecifiedStartPosition = Me.Sequences.Max(Function(f) f.SequenceStartIndex) + 1
                If SpecifiedEndPosition Is Nothing Then SpecifiedEndPosition = Me.Sequences.Min(Function(f) f.SequenceEndIndex) + 1
        End Select

        If Me.SpecifiedEndPosition > Me.FilePositionCount Then
            Me.SpecifiedEndPosition = Me.FilePositionCount
        End If

        Me.DisplayStartPosition = Me.SpecifiedStartPosition
        Me.DisplayEndPosition = Me.SpecifiedEndPosition

        ' This sub pivots the data to generate a list of positions
        Dim count As Integer = 0
        Me.PositionRows = New List(Of PositionRow)
        For i As Integer = DisplayStartPosition To DisplayEndPosition
            ' Cycle through positions
            Dim pos As New Position(i)
            Dim row As PositionRow
            If count Mod Settings.BasePairsPerRow = 0 Then
                row = New PositionRow()
                Me.PositionRows.Add(row)
            End If
            count += 1
            pos.DisplayIndex = count
            For j = 0 To Me.Sequences.Count - 1
                ' Cycle through species at each position
                Try
                    pos.Species.Add(New Nucleobase(Me.Sequences(j).Sequence(i - 1), j))
                Catch ex As Exception
                    ' This species doesn't extend that far (fill it out with "-")
                    pos.Species.Add(New Nucleobase("-", j))
                End Try
            Next j
            SetHighlighting(pos)
            row.Positions.Add(pos)
        Next
    End Sub

    'Private Sub PivotDataForWrap()
    '    Dim startPosition As Integer = 0
    '    Dim endPosition As Integer = 0

    '    Dim TrimOptions = Settings.TrimmingOptions
    '    If TrimOptions = TrimmingOptions.AtLeastTwo And Me.Sequences.Count = 1 Then
    '        TrimOptions = TrimmingOptions.AllSpecies
    '    End If

    '    Select Case TrimOptions
    '        Case TrimmingOptions.None
    '            startPosition = 0
    '            endPosition = Me.TotalPositionCount - 1
    '        Case TrimmingOptions.AtLeastTwo
    '            startPosition = Me.Sequences.OrderBy(Function(f) f.SequenceStartIndex)(1).SequenceStartIndex
    '            endPosition = Me.Sequences.OrderBy(Function(f) f.SequenceEndIndex)(Me.Sequences.Count - 2).SequenceEndIndex
    '        Case TrimmingOptions.AllSpecies
    '            startPosition = Me.Sequences.Max(Function(f) f.SequenceStartIndex)
    '            endPosition = Me.Sequences.Min(Function(f) f.SequenceEndIndex)
    '    End Select

    '    'endPosition = startPosition + 500

    '    ' This sub pivots the data to generate a list of positions
    '    Dim count As Integer = 0
    '    Positions = New List(Of Position)(Me.TotalPositionCount)
    '    For i As Integer = startPosition To endPosition
    '        Dim pos As New Position(i + 1)
    '        count += 1
    '        pos.DisplayIndex = count
    '        For j = 0 To Me.Sequences.Count - 1
    '            Try
    '                pos.Species.Add(Me.Sequences(j).Sequence(i))
    '            Catch ex As Exception
    '            End Try
    '        Next

    '        Me.Positions.Add(pos)
    '    Next

    '    Me.DisplayPositionCount = endPosition - startPosition
    'End Sub

    Public Sub ReadSequences()
        Me.Sequences.Clear()
        Dim index As Integer = 0
        Dim tr As New System.IO.StreamReader(Me.FileName)
        While Not tr.EndOfStream
            Dim chr As Char = ChrW(tr.Peek())
            While chr <> ">"c
                tr.ReadLine()
                chr = ChrW(tr.Peek())
            End While
            Dim s As New Sequence(index)
            s.ReadSequence(tr)
            Me.Sequences.Add(s)
            index += 1
        End While
        tr.Close()
        'PivotData()
    End Sub

    Public Sub New()
        _FilePositionCount = 0
        Me.Sequences = New List(Of Sequence)
    End Sub

    Public Function Search(ByVal sequence As String, ByVal SpeciesIndex As Integer) As Integer?
        Return Me.Sequences(SpeciesIndex).Search(sequence)
    End Function

    Public Function Compare(ByVal SpeciesIndex1 As Integer, ByVal SpeciesIndex2 As Integer) As String
        Return Me.Sequences(SpeciesIndex1).CompareTo(Me.Sequences(SpeciesIndex2))
    End Function

    Public Sub InsertGaps(ByVal SpeciesIndex As Integer, ByVal PositionIndex As Integer, ByVal GapCount As Integer)
        Me.Sequences(SpeciesIndex).InsertGaps(PositionIndex, GapCount)
        ' Recalculate the _FilePositionCount
        _FilePositionCount = Me.Sequences.Max(Function(f) f.Sequence.Count)
    End Sub

    Public Sub SaveVisibleToFile()
        Dim tw As New System.IO.StreamWriter(Me.FileName)
        ' Write each species to the specified file between DisplayStartPosition and DisplayEndPosition
        For Each s In Me.Sequences
            tw.WriteLine(">" & s.SpeciesName & " " & s.OtherData)
            For i As Integer = DisplayStartPosition - 1 To DisplayEndPosition - 1 Step 80
                ' Read 80 positions then write them as a line
                Dim numToTake = Math.Min(CType(DisplayEndPosition, Integer) - i, 80)
                Dim str As String = String.Join("", s.Sequence.Skip(i).Take(numToTake).ToArray())
                tw.WriteLine(str)
            Next i
        Next
        tw.Close()
    End Sub

    Public Sub SaveDatasetToFile()
        Dim tw As New System.IO.StreamWriter(Me.FileName)
        ' Write each species to the specified file between DisplayStartPosition and DisplayEndPosition
        For Each s In Me.Sequences
            tw.WriteLine(">" & s.SpeciesName & " " & s.OtherData)
            For i As Integer = 0 To Me.FilePositionCount - 1 Step 80
                ' Read 80 positions then write them as a line
                Dim numToTake = Math.Min(CType(Me.FilePositionCount, Integer) - i, 80)
                Dim str As String = String.Join("", s.Sequence.Skip(i).Take(numToTake).ToArray())
                tw.WriteLine(str)
            Next i
        Next
        tw.Close()
    End Sub

    Public Sub SetRangeHighlight(ByVal Highlight As Boolean)
        For Each pr In Me.PositionRows
            For Each pos In pr.Positions
                If pos.FileIndex >= Me.SpecifiedStartPosition And pos.FileIndex <= Me.SpecifiedEndPosition Then _
                pos.Highlight = Highlight
            Next
        Next
    End Sub

    Public Sub CopySequenceRangeToClipboard(ByVal SpeciesIndex As Integer)
        Dim s As List(Of Char) = Me.Sequences(SpeciesIndex).Sequence
        Dim str As String = ""
        Dim count As Integer = 1
        For i As Integer = Me.SpecifiedStartPosition To Me.SpecifiedEndPosition            
            str &= s.Item(i)
            If count Mod 60 = 0 Then str &= vbCrLf
            count += 1
        Next i
        Clipboard.SetText(str)
    End Sub

    Public Sub SetAllHighlight(ByVal Highlight As Boolean)
        For Each pr In Me.PositionRows
            For Each pos In pr.Positions
                pos.Highlight = Highlight
            Next
        Next
    End Sub

    Public Sub CountHighlighted()
        Me.HighlightCount = 0
        Me.DimCount = 0
        For Each pr In Me.PositionRows
            For Each pos In pr.Positions
                If pos.Highlight Then Me.HighlightCount += 1 Else Me.DimCount += 1
            Next
        Next
    End Sub

    Private Sub AddPairing(ByRef pairings As List(Of Pairing), ByVal bucket As List(Of Integer), ByVal pos As Integer)
        For Each p In pairings
            If p.IsThisAPairingFor(bucket) Then
                p.AddPosition(pos)
                Return ' we have found the pairing and added a position index to it
            End If
        Next p
        ' we haven't found the pairing, therefore create it
        Dim newPairing As New Pairing(bucket)
        newPairing.AddPosition(pos)
        pairings.Add(newPairing)
    End Sub

    Private Function GetSpeciesNames(ByVal SpeciesList As List(Of Integer), Optional ByVal Separator As String = ", ") As String
        Dim list = New List(Of String)
        For Each s In SpeciesList
            list.Add(Me.Sequences(s).SpeciesName)
        Next s
        Return String.Join(Separator, list)
    End Function

    Public Sub FindAllPairings()
        Dim maxPairingOrdinal As Integer = Me.Sequences.Count / 2
        Dim pairings As New List(Of Pairing)
        For Each pr In Me.PositionRows
            For Each p In pr.Positions
                Dim ABucket As New List(Of Integer) ' A list of Species indeces that have an A in this position
                Dim GBucket As New List(Of Integer) ' A list of Species indeces that have an G in this position
                Dim CBucket As New List(Of Integer) ' A list of Species indeces that have an C in this position
                Dim TBucket As New List(Of Integer) ' A list of Species indeces that have an T in this position
                Dim BBucket As New List(Of Integer) ' A list of Species indeces that have an - in this position
                For Each s In p.Species
                    ' Place each species into 1 of 5 buckets
                    If s.Base = "A"c Then ABucket.Add(s.SpeciesIndex)
                    If s.Base = "G"c Then GBucket.Add(s.SpeciesIndex)
                    If s.Base = "C"c Then CBucket.Add(s.SpeciesIndex)
                    If s.Base = "T"c Then TBucket.Add(s.SpeciesIndex)
                    If s.Base = "-"c Then BBucket.Add(s.SpeciesIndex)
                Next s
                ' If any buckets have 2 or more species and lteq half the number of species then log these
                If ABucket.Count >= 2 And ABucket.Count <= maxPairingOrdinal Then AddPairing(pairings, ABucket, p.FileIndex)
                If GBucket.Count >= 2 And GBucket.Count <= maxPairingOrdinal Then AddPairing(pairings, GBucket, p.FileIndex)
                If CBucket.Count >= 2 And CBucket.Count <= maxPairingOrdinal Then AddPairing(pairings, CBucket, p.FileIndex)
                If TBucket.Count >= 2 And TBucket.Count <= maxPairingOrdinal Then AddPairing(pairings, TBucket, p.FileIndex)
                If BBucket.Count >= 2 And BBucket.Count <= maxPairingOrdinal Then AddPairing(pairings, BBucket, p.FileIndex)
            Next p
        Next pr
        Dim strResult As String = String.Format("Out of {0} positions counted" & vbCrLf & "-----------------" & vbCrLf & vbCrLf, Me.FilePositionCount)        
        ' List all pairings found (ordered by count)
        strResult &= "Listing all pairings found (ordered by count)..." & vbCrLf & vbCrLf
        Dim ald = pairings.OrderByDescending(Function(f) f.PositionList.Count)
        For Each a As Pairing In ald
            strResult &= String.Format("Species: {0} : {1} positions - {2}" & vbCrLf, GetSpeciesNames(a.SpeciesList), _
                                       a.PositionList.Count, String.Join(", ", a.PositionList))
        Next a

        Common.WriteToFile(Me.PairingsFileName, strResult)
        Process.Start(Me.PairingsFileName)
    End Sub

    Public Function IsExon(ByVal posIndex As Integer) As Boolean
        If Me.ExonSequence Is Nothing Then Return False
        If posIndex >= Me.ExonSequence.Sequence.Count Then Return False
        Return (Me.ExonSequence.Sequence(posIndex) <> "-")
    End Function

    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
