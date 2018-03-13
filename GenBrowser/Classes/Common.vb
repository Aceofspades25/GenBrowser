Imports System.IO

Public Class Common

    Public Shared Function GetDirectory(ByVal f As String) As String
        Dim filePortion As String = f.Split("\"c)(f.Split("\"c).Length - 1)
        Return f.Substring(0, f.Length - filePortion.Length)
    End Function

    Private Shared Function GetParent(reference As DependencyObject) As DependencyObject
        Dim parent As DependencyObject = Nothing

        If TypeOf reference Is FrameworkElement Then
            parent = DirectCast(reference, FrameworkElement).Parent
        ElseIf TypeOf reference Is FrameworkContentElement Then
            parent = DirectCast(reference, FrameworkContentElement).Parent
        End If

        Return If(parent, VisualTreeHelper.GetParent(reference))
    End Function

    Public Shared Function GetAncestor(Of T)(ByVal obj As DependencyObject, ByVal depth As Integer)
        ' Retrieves an ancestor of particular type for a xaml object to a given depth (e.g. return the second StackPanel in this objects ancestry)
        Dim parent = GetParent(obj)

        If parent IsNot Nothing Then
            If parent.GetType.Equals(GetType(T)) Then
                ' If parent is of the correct type
                Return If(depth = 1, parent, GetAncestor(Of T)(parent, depth - 1))
            Else
                Return GetAncestor(Of T)(parent, depth)
            End If
        Else
            Return Nothing
        End If
    End Function


    Private Shared Function GetNthChild(reference As DependencyObject, ByVal N As Integer) As DependencyObject
        Return VisualTreeHelper.GetChild(reference, N)
    End Function

    Public Shared Function GetFirstDescendant(Of T)(ByVal obj As DependencyObject) As DependencyObject
        'Dim firstDescendant = GetNthChild(obj, 0)
        'If firstDescendant IsNot Nothing Then
        '    If firstDescendant.GetType.Equals(GetType(T)) Then
        '        ' If parent is of the correct type
        '        Return firstDescendant
        '    Else
        '        Return GetFirstDescendant(Of T)(firstDescendant)
        '    End If
        'Else
        '    Return Nothing
        'End #

        'Dim firstDescendant = GetNthChild(obj, 0)
        If obj IsNot Nothing Then
            If obj.GetType.Equals(GetType(T)) Then
                ' If self is of the correct type
                Return obj
            Else
                Return GetFirstDescendant(Of T)(GetNthChild(obj, 0))
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Shared Function GetDescendantBoundToObject(Of T)(ByVal obj As DependencyObject, ByVal boundTo As Object) As DependencyObject
        Dim childCount = VisualTreeHelper.GetChildrenCount(obj)
        If TypeOf (obj) Is FrameworkElement Then
            If DirectCast(obj, FrameworkElement).DataContext Is boundTo Then Return GetFirstDescendant(Of T)(obj) ' Jackpot
        End If
        If childCount = 0 Then Return Nothing ' Reached a leaf node with no luck, return nothing        
        For i As Integer = 0 To childCount - 1
            Dim child = GetNthChild(obj, i)
            Dim result = GetDescendantBoundToObject(Of T)(child, boundTo)
            If Not result Is Nothing Then
                Return result ' Jackpot
            End If
        Next i
        Return Nothing ' No object was found bound to this element
    End Function

    Public Shared Sub WriteToFile(ByVal FileName As String, ByVal strOut As String)
        Dim sw As New System.IO.StreamWriter(FileName, False)
        sw.Write(strOut)
        sw.Close()
    End Sub

    Public Shared Function IsANucleotide(ByVal letter As Char)
        Return "AGCT".Contains(letter)
    End Function

    Public Shared Function IsSequenceString(ByVal str As String) As Boolean
        For i As Integer = 0 To str.Length - 1
            If Not IsANucleotide(str(i)) Then
                Return False
            End If
        Next i
        Return True
    End Function

End Class
