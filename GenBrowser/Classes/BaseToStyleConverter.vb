Public Class BaseToStyleConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Select Case value
            Case "G"
                Return Application.Current.FindResource("GBlock")
            Case "A"
                Return Application.Current.FindResource("ABlock")
            Case "C"
                Return Application.Current.FindResource("CBlock")
            Case "T"
                Return Application.Current.FindResource("TBlock")
            Case "N"
                Return Application.Current.FindResource("NBlock")
            Case Else
                Return Application.Current.FindResource("GapBlock")
        End Select
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class
