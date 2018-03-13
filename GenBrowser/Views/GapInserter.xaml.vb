Public Class GapInserter
    Private ReadOnly Property GEContext As GapInserterViewModel
        Get
            Return Me.Resources.Item("GapInserterContext")
        End Get
    End Property

    Public Sub New(ByVal MainViewModel As MainViewModel)
        InitializeComponent()
        Me.GEContext.MainViewModel = MainViewModel
    End Sub
End Class
