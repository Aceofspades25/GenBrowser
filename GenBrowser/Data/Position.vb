Imports System.ComponentModel

Public Class Position
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public Property FileIndex As Integer
    Public Property DisplayIndex As Integer
    Private _Highlight As Boolean = False
    Public Property Highlight As Boolean
        Get
            Return _Highlight
        End Get
        Set(value As Boolean)
            _Highlight = value
            NotifyPropertyChanged("Highlight")
        End Set
    End Property
    Public Property Species As List(Of Nucleobase)

    Public ReadOnly Property ToolTip As String
        Get
            Return "Position: " & DisplayIndex.ToString(",0") & " (" & Me.FileIndex.ToString(",0") & ")"
        End Get
    End Property

    Public Sub New(ByVal Index As Integer)
        Me.FileIndex = Index
        Me.Species = New List(Of Nucleobase)
    End Sub

    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
