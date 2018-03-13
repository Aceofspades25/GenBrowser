Public Class Nucleobase
    Public Property Base As Char
    Public Property SpeciesIndex As Integer

    Public Function ReturnCompliment() As Char
        Select Case Me.Base
            Case "T" : Return "A"
            Case "A" : Return "T"
            Case "G" : Return "C"
            Case "C" : Return "G"
            Case "N" : Return "N"
            Case "-" : Return "-"
            Case Else : Return "N"
        End Select
    End Function

    Public Sub New(ByVal base As Char, ByVal speciesIndex As Integer)
        Me.Base = base
        Me.SpeciesIndex = speciesIndex
    End Sub
End Class
