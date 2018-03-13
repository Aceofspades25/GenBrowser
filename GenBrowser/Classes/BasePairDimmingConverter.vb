Public Class BasePairDimmingConverter
    Implements IValueConverter

    'Public Property Factor As Double = 0.5

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim val As Boolean = CType(value, Boolean)

        Return If(val, 1, Settings.DimmingOpacity)
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Dim val As Double = CType(value, Double)

        If val < 1 Then Return True
        Return False
    End Function

End Class
