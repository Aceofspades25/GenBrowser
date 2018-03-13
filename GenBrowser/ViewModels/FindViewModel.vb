Imports System.ComponentModel

Public Class FindViewModel
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

#Region " Bindable properties "

    Private _MainViewModel As MainViewModel
    Public Property MainViewModel As MainViewModel
        Get
            Return _MainViewModel
        End Get
        Set(value As MainViewModel)
            _MainViewModel = value
            NotifyPropertyChanged("MainViewModel")
        End Set
    End Property
    Public Property SearchString As String
    Public Property SpeciesIndex As Integer

#End Region

#Region " Command properties "

    Public Property SearchCommand As ICommand

#End Region

    Public Sub New()
        Me.SearchCommand = New CommandHandler(AddressOf OnSearch, True)
        Dim clipBoardText As String = Clipboard.GetText().Replace("-", "").Replace(vbCr, "").Replace(vbLf, "")
        If Common.IsSequenceString(clipBoardText) Then
            Me.SearchString = clipBoardText
        Else
            Me.SearchString = Settings.LastSearchString
        End If

    End Sub

#Region " Command handlers "

    Private Sub OnSearch()
        Dim positionIndex As Integer? = Me.MainViewModel.Data.Search(Me.SearchString.ToUpper(), SpeciesIndex)
        If positionIndex Is Nothing Then
            MessageBox.Show("Match not found", "Match not found", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Settings.LastSearchString = Me.SearchString
        ' Now we need to set SelectedPosition and SelectedNucleobase on MainViewModel
        If Me.MainViewModel.SelectNucleobase(positionIndex, SpeciesIndex) Then
            Me.MainViewModel.CloseFindDialog()
        Else
            MessageBox.Show(String.Format("A match was found at position {0} but this position is not currently being displayed", positionIndex), "Match found", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

#End Region

    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
