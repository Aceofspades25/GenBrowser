Public Class SequenceComparison
    Private ReadOnly Property SCContext As SequenceComparisonViewModel
        Get
            Return Me.Resources.Item("SequenceComparisonContext")
        End Get
    End Property

    Public Sub New(ByVal MainViewModel As MainViewModel)
        InitializeComponent()
        Me.SCContext.MainViewModel = MainViewModel
    End Sub
End Class
