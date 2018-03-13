Imports Microsoft.Win32

Public Class Main
    Dim SelectedBlock As TextBlock
    Dim SelectedHeader As TextBlock

    Private ReadOnly Property MainContext As MainViewModel
        Get
            Return Me.Resources.Item("MainContext")
        End Get
    End Property

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        AddHandler Me.MainContext.SelectedPositionChanged, AddressOf UpdateSelectedPosition
    End Sub

    Private Sub ResetStyle()
        Dim bsc As New BaseToStyleConverter()
        Me.SelectedBlock.Style = bsc.Convert(Me.SelectedBlock.Text, Nothing, Nothing, Nothing)
        Me.SelectedHeader.Text = ""
    End Sub

    Private Sub UpdateSelectionDisplay(ByVal T As TextBlock)
        ' tb -> ContentPresenter -> StackPanel -> ItemsPresenter -> Border -> (ItemsControl) -> StackPanel (Vertical)
        If Not Me.SelectedBlock Is Nothing Then ResetStyle()
        Me.SelectedBlock = CType(T, TextBlock)
        Me.SelectedBlock.Style = Application.Current.FindResource("SelectedBlock")
        Dim parent As StackPanel = Common.GetAncestor(Of StackPanel)(Me.SelectedBlock, 2)
        Me.SelectedHeader = parent.Children(0)
        Me.SelectedHeader.Text = "ê"
        parent.BringIntoView()
    End Sub

    Private Sub BlockSelected(sender As Object, e As MouseButtonEventArgs)
        UpdateSelectionDisplay(sender)
        If My.Computer.Keyboard.ShiftKeyDown Or My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.InsertStart()
        End If

        Me.MainContext.SelectedPosition = Me.SelectedHeader.DataContext
        Me.MainContext.SelectedNucleobase = Me.SelectedBlock.DataContext

        If My.Computer.Keyboard.ShiftKeyDown Or My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.InsertEnd(True)            
        End If

        ' If the CTRL key was down then also toggle the range
        If My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.ToggleRange()
        End If
    End Sub

    Private Sub UpdateSelectedPosition()
        ' Need to somehow highlight the textblock bound to selected position???
        ' Perhaps this should be done differently??? (i.e. Selection should be displayed using binding)
        ' Or perhaps we could use an algorithm to search through every single visual element to find the one with the correct datacontext (how inefficient?)
        ' This second method is nicer because the form could be mode to scroll this visual element into view
        ' This is extremely vulnerable to any changes in the UI
        Dim panel As StackPanel = Common.GetFirstDescendant(Of StackPanel)(Me.PositionRowItemControl)
        For Each rowcp As ContentPresenter In panel.Children
            ' First jump straight to the correct row
            If rowcp.DataContext Is Me.MainContext.SelectedRow Then
                ' Then traverse the VisualTree from that row to find the TextBlock bound to this Nucleobase
                Dim tb As TextBlock = Common.GetDescendantBoundToObject(Of TextBlock)(rowcp, Me.MainContext.SelectedNucleobase)
                UpdateSelectionDisplay(tb)
                'tb.BringIntoView()
            End If
        Next rowcp
    End Sub

    Private Sub Main_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles Me.PreviewKeyDown
        If Me.MainContext.HasData Then
            If e.Key = Key.A Then
                If My.Computer.Keyboard.CtrlKeyDown Then
                    Me.MainContext.ToggleRange()
                Else
                    ' A toggles position highlighting
                    Me.MainContext.SelectedPosition.Highlight = Not Me.MainContext.SelectedPosition.Highlight
                End If
            End If
        End If
        If e.Key = Key.O And My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.OpenCommand.Execute(Nothing)
        End If
        If e.Key = Key.S And My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.SaveDatasetAsCommand.Execute(Nothing)
        End If
        If e.Key = Key.F And My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.FindCommand.Execute(Nothing)
        End If
        If e.Key = Key.I And My.Computer.Keyboard.CtrlKeyDown Then
            Me.MainContext.AboutCommand.Execute(Nothing)
        End If
    End Sub
End Class
