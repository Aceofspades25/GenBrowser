Public Class Find

    Private ReadOnly Property FindContext As FindViewModel
        Get
            Return Me.Resources.Item("FindContext")
        End Get
    End Property

    Public Sub New(ByVal MainViewModel As MainViewModel)
        InitializeComponent()
        Me.FindContext.MainViewModel = MainViewModel
    End Sub
End Class
