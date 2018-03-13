Imports System.ComponentModel

Public Class GapInserterViewModel
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
    Public Property GapCount As Integer

#End Region

#Region " Command properties "

    Public Property RunCommand As ICommand

#End Region

    Public Sub New()
        Me.RunCommand = New CommandHandler(AddressOf OnRun, True)
    End Sub

#Region " Command handlers "

    Private Sub OnRun()
        Me.MainViewModel.Data.InsertGaps(Me.MainViewModel.SelectedNucleobase.SpeciesIndex, Me.MainViewModel.SelectedPosition.FileIndex, Me.GapCount)
        'Me.MainViewModel.Data.PivotData()
        Me.MainViewModel.RefreshWithoutReloading()
    End Sub

#End Region

    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
