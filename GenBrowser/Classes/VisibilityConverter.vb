Public Class VisibilityConverter
    Implements IValueConverter

    Public Property Invert As Boolean = False

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim vis As Boolean = CType(value, Boolean)
        If Invert Then
            Return If(vis, Visibility.Collapsed, Visibility.Visible)
        Else
            Return If(vis, Visibility.Visible, Visibility.Collapsed)
        End If
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Dim vis As Visibility = CType(value, Visibility)
        If Invert Then
            Return (vis = Visibility.Collapsed)
        Else
            Return (vis = Visibility.Visible)
        End If
    End Function
End Class