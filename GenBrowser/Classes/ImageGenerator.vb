Imports System.Drawing
Imports System.IO

Public Class ImageGenerator
    Private Data As MasterRecord
    Private Bitmap As Bitmap
    Const SequenceHeight As Integer = 10
    Const SetLength As Integer = 800

    'Dim AlignedColour As Pen = Pens.Green
    'Dim IndelColour As Pen = Pens.Black
    'Dim ExonColour As Pen = Pens.LightSeaGreen
    'Dim UnsequencedColour As Pen = Pens.LightGray
    Dim AlignedColour As Pen = Pens.LightGray
    Dim IndelColour As Pen = Pens.Black
    Dim ExonColour As Pen = Pens.LightGray
    Dim UnsequencedColour As Pen = Pens.Gray
    Dim SNPColour As Pen = Pens.White

    Public Sub New(ByVal Data As MasterRecord)
        Me.Data = Data
    End Sub

    Public Function GenerateOverview() As Bitmap
        Dim setHeight As Integer = (Me.Data.Sequences.Count + 1) * SequenceHeight
        If Not Me.Data.ExonSequence Is Nothing Then
            ' If it has an exon sequence, then don't display it and reduce the height
            setHeight = (Me.Data.Sequences.Count) * SequenceHeight
        End If
        Dim setCount As Integer = (Me.Data.FilePositionCount / SetLength) + 1
        Dim bmp As New Bitmap(SetLength + 80, setCount * setHeight + 10)
        Dim gr As Graphics = Graphics.FromImage(bmp)
        gr.FillRectangle(Brushes.Black, 0, 0, SetLength + 80, setCount * setHeight + 10)
        Dim fnt As New Font("Verdana", 10, FontStyle.Regular, GraphicsUnit.Pixel)
        Dim fnt2 As New Font("Verdana", 8, FontStyle.Regular, GraphicsUnit.Pixel)
        'Dim cnt As Integer = 0
        'For Each s In Me.Data.Sequences
        '    gr.DrawString(s.SpeciesName, fnt, Brushes.White, 10, cnt * 10 + 10)
        '    cnt += 1
        'Next s

        'For i As Integer = 0 To 799
        '    Dim scnt As Integer = 0
        '    For Each s In Me.Data.Sequences
        '        'Dim rect As New Rectangle(80 + i, scnt * 30 + 5, 2, 30)
        '        Dim pt1 As New Point(80 + i, scnt * 10 + 11)
        '        Dim pt2 As New Point(80 + i, (scnt + 1) * 10 + 9)
        '        If s.Sequence(i) = "N" Then
        '            'gr.FillRectangle(Brushes.Gray, rect)
        '            gr.DrawLine(Pens.Gray, pt1, pt2)
        '        ElseIf s.Sequence(i) = "-" Then
        '        Else
        '            'gr.FillRectangle(Brushes.Green, rect)
        '            gr.DrawLine(Pens.Green, pt1, pt2)
        '        End If
        '        scnt += 1
        '    Next s
        'Next i

        Dim currentSet As Integer = -1
        Dim currentCol As Integer = 0
        For posIndex As Integer = 0 To Data.FilePositionCount - 1
            If posIndex Mod SetLength = 0 Then
                ' Start a new set
                currentSet += 1
                'gr.DrawString(posIndex + 1, fnt2, Brushes.White, 80, currentSet * setHeight)
                gr.DrawString(posIndex + SetLength, fnt2, Brushes.White, SetLength + 50, currentSet * setHeight)
                Dim scnt1 As Integer = 0
                For Each s In Me.Data.Sequences
                    If s.SpeciesName <> "Exons" Then
                        Dim yPos As Integer = scnt1 * SequenceHeight + SequenceHeight + currentSet * setHeight
                        gr.DrawString(s.SpeciesName, fnt, Brushes.White, 10, yPos)
                        scnt1 += 1
                    End If
                Next s
                currentCol = 0

            End If

            ' Draw the sequences...
            Dim scnt2 As Integer = 0
            For Each s In Me.Data.Sequences
                If s.SpeciesName <> "Exons" And posIndex < s.Sequence.Count Then
                    Dim pt1 As New Point(80 + currentCol, scnt2 * SequenceHeight + SequenceHeight + 1 + currentSet * setHeight)
                    Dim pt2 As New Point(80 + currentCol, scnt2 * SequenceHeight + SequenceHeight + 9 + currentSet * setHeight)
                    If s.Sequence(posIndex) = "N" Then
                        gr.DrawLine(Me.UnsequencedColour, pt1, pt2)
                    ElseIf s.Sequence(posIndex) = "-" Then
                        gr.DrawLine(Me.IndelColour, pt1, pt2)
                    ElseIf Me.Data.Sequences(0).Sequence(posIndex) <> Me.Data.Sequences(1).Sequence(posIndex) _
                        And Common.IsANucleotide(Me.Data.Sequences(0).Sequence(posIndex)) _
                        And Common.IsANucleotide(Me.Data.Sequences(1).Sequence(posIndex)) Then
                        gr.DrawLine(Me.SNPColour, pt1, pt2)
                    ElseIf Me.Data.IsExon(posIndex) Then
                        gr.DrawLine(Me.ExonColour, pt1, pt2)
                    Else
                        gr.DrawLine(Me.AlignedColour, pt1, pt2)
                    End If
                    scnt2 += 1
                End If
            Next s

            currentCol += 1
        Next posIndex

        Return bmp
    End Function

    Public Function LoadImage() As BitmapSource
        Dim ms As New MemoryStream()
        Me.Bitmap = GenerateOverview()
        Me.Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
        ms.Position = 0
        Dim bi As New BitmapImage()
        bi.BeginInit()
        bi.StreamSource = ms
        bi.EndInit()
        Return bi
    End Function

    Public Sub SaveImage(ByVal FileName As String)
        'Dim format As New ImageFormat
        Me.Bitmap.Save(FileName, System.Drawing.Imaging.ImageFormat.Png)
    End Sub


End Class
