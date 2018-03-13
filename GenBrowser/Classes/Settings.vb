Public Enum TrimmingOptions
    None
    AtLeastTwo
    AllSpecies
End Enum

Public Class Settings

    Public Shared Property LastOpenDirectory As String
        Get
            Return My.Settings.LastOpenDirectory
        End Get
        Set(value As String)
            My.Settings.LastOpenDirectory = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property LastSaveDirectory As String
        Get
            Return My.Settings.LastSaveDirectory
        End Get
        Set(value As String)
            My.Settings.LastSaveDirectory = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property TrimmingOptions As TrimmingOptions
        Get
            Return My.Settings.TrimmingOptions
        End Get
        Set(value As TrimmingOptions)
            My.Settings.TrimmingOptions = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property BasePairsPerRow As Integer
        Get
            Return My.Settings.BasePairsPerRow
        End Get
        Set(value As Integer)
            My.Settings.BasePairsPerRow = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property LoadAsynchronously As Boolean
        Get
            Return My.Settings.LoadAsynchronously
        End Get
        Set(value As Boolean)
            My.Settings.LoadAsynchronously = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property HighlightBasePairs As Boolean
        Get
            Return My.Settings.HighlightBasePairs
        End Get
        Set(value As Boolean)
            My.Settings.HighlightBasePairs = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property DimmingOpacity As Double
        Get
            Return My.Settings.DimmingOpacity
        End Get
        Set(value As Double)
            My.Settings.DimmingOpacity = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property LastSearchString As String
        Get
            Return My.Settings.LastSearchString
        End Get
        Set(value As String)
            My.Settings.LastSearchString = value
            My.Settings.Save()
        End Set
    End Property

    Public Shared Property InitialLoadLength As Integer
        Get
            Return My.Settings.InitialLoadLength
        End Get
        Set(value As Integer)
            My.Settings.InitialLoadLength = value
            My.Settings.Save()
        End Set
    End Property

End Class
