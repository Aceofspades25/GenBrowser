Imports System.ComponentModel

Public Class SequenceComparisonViewModel
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
    Public Property SpeciesIndex1 As Integer
    Public Property SpeciesIndex2 As Integer

#End Region

#Region " Command properties "

    Public Property RunCommand As ICommand

#End Region

    Public Sub New()
        Me.RunCommand = New CommandHandler(AddressOf OnRun, True)
        Try
            Me.SpeciesIndex2 = Me.SpeciesIndex1 + 1
        Catch ex As Exception
        End Try
    End Sub

#Region " Command handlers "

    Private Sub OnRun()
        Dim result As String = Me.MainViewModel.Data.Compare(Me.SpeciesIndex1, Me.SpeciesIndex2)
        MessageBox.Show(result)
    End Sub

#End Region

    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
