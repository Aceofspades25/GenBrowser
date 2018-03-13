Imports System.ComponentModel
Imports Microsoft.Win32

Public Class SequenceOverviewGenerator
    Private MainViewModel As MainViewModel
    Private ImageGenerator As ImageGenerator

    'Private ReadOnly Property FindContext As FindViewModel
    '    Get
    '        Return Me.Resources.Item("FindContext")
    '    End Get
    'End Property

    Public Sub New(ByVal MainViewModel As MainViewModel)
        InitializeComponent()
        Me.MainViewModel = MainViewModel

        Dim dep As New DependencyObject()
        If Not DesignerProperties.GetIsInDesignMode(dep) Then
            ImageGenerator = New ImageGenerator(MainViewModel.Data)
            Me.OverviewImage.Source = ImageGenerator.LoadImage()
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
        Dim SaveFileDialog As New SaveFileDialog()
        SaveFileDialog.RestoreDirectory = False
        SaveFileDialog.InitialDirectory = Settings.LastSaveDirectory
        SaveFileDialog.AddExtension = True
        SaveFileDialog.OverwritePrompt = True
        SaveFileDialog.DefaultExt = ".png"
        SaveFileDialog.Filter = "png files (*.png)|*.png"
        If SaveFileDialog.ShowDialog() Then
            Me.ImageGenerator.SaveImage(SaveFileDialog.FileName)
        End If
    End Sub
End Class
