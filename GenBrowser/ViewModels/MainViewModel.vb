Imports System.ComponentModel
Imports Microsoft.Win32
Imports System.Collections.ObjectModel
Imports System.Windows.Threading
Imports Microsoft.VisualBasic.ApplicationServices

Public Class MainViewModel
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
    Public Event SelectedPositionChanged()

    Private StartTime As DateTime

#Region " Bindable properties "

    Public Property Data As MasterRecord
    Public Property PositionRows As ObservableCollection(Of PositionRow)
    Public Property Species As List(Of String)
    Public Property StatusText As String = "Please select a file to load..."
    Public Property SpeciesCount As Integer
    Public Property TotalSequenceLength As Integer
    Public Property DisplayLength As Integer
    Public Property HasData As Boolean = False

    Private _SelectedNucleobase As Nucleobase
    Public Property SelectedNucleobase As Nucleobase
        Get
            Return _SelectedNucleobase
        End Get
        Set(value As Nucleobase)
            _SelectedNucleobase = value
            NotifyPropertyChanged("SelectedNucleobase")
            NotifyPropertyChanged("SelectedSpecies")
        End Set
    End Property
    Private _SelectedPosition As Position
    Public Property SelectedPosition As Position
        Get
            Return _SelectedPosition
        End Get
        Set(value As Position)
            _SelectedPosition = value
            NotifyPropertyChanged("SelectedPosition")
        End Set
    End Property
    Public Property SelectedRow As PositionRow
    Public ReadOnly Property SelectedSpecies As String
        Get
            If Me.Data Is Nothing OrElse Me.SelectedNucleobase Is Nothing Then Return Nothing
            Return Me.Data.Sequences(Me.SelectedNucleobase.SpeciesIndex).SpeciesName
        End Get
    End Property

#End Region

#Region " Windows "

    Private Find As Find

#End Region

#Region " Command properties "

    Public Property OpenCommand As ICommand
    Public Property SaveCommand As ICommand
    Public Property SaveVisibleAsCommand As ICommand
    Public Property SaveDatasetAsCommand As ICommand
    Public Property RefreshCommand As ICommand
    Public Property OverviewCommand As ICommand
    Public Property FindCommand As ICommand
    Public Property CropCommand As ICommand
    Public Property HighlightRangeCommand As ICommand
    Public Property DimRangeCommand As ICommand
    Public Property CopyRangeCommand As ICommand
    Public Property HighlightAllCommand As ICommand
    Public Property DimAllCommand As ICommand
    Public Property InsertGapsCommand As ICommand
    Public Property CountHighlightedCommand As ICommand
    Public Property CleanStringCommand As ICommand
    Public Property FixLineLengthCommand As ICommand
    Public Property QualityTrimmerCommand As ICommand
    Public Property SequenceCompareCommand As ICommand
    Public Property FindPairingsCommand As ICommand
    Public Property SequenceInvertCommand As ICommand
    Public Property CodonifyCommand As ICommand
    Public Property InsertStartCommand As ICommand
    Public Property InsertEndCommand As ICommand
    Public Property SelectStartPositionCommand As ICommand
    Public Property SelectEndPositionCommand As ICommand
    Public Property AboutCommand As ICommand

#End Region

    Public Sub New()
        Me.OpenCommand = New CommandHandler(AddressOf OnOpen, True)
        Me.SaveCommand = New CommandHandler(AddressOf OnSave, True)
        Me.SaveVisibleAsCommand = New CommandHandler(AddressOf OnSaveVisibleAs, True)
        Me.SaveDatasetAsCommand = New CommandHandler(AddressOf OnSaveDatasetAs, True)
        Me.RefreshCommand = New CommandHandler(AddressOf RefreshWithoutReloading, True)
        Me.OverviewCommand = New CommandHandler(AddressOf OnOverview, True)
        Me.FindCommand = New CommandHandler(AddressOf OnFind, True)
        Me.CropCommand = New CommandHandler(AddressOf RefreshWithoutReloading, True)
        Me.HighlightRangeCommand = New CommandHandler(AddressOf OnHighlightRange, True)
        Me.DimRangeCommand = New CommandHandler(AddressOf OnDimRange, True)
        Me.CopyRangeCommand = New CommandHandler(AddressOf OnCopyRange, True)
        Me.HighlightAllCommand = New CommandHandler(AddressOf OnHighlightAll, True)
        Me.DimAllCommand = New CommandHandler(AddressOf OnDimAll, True)
        Me.InsertGapsCommand = New CommandHandler(AddressOf OnInsertGaps, True)
        Me.CountHighlightedCommand = New CommandHandler(AddressOf OnCountHighlighted, True)
        Me.CleanStringCommand = New CommandHandler(AddressOf OnCleanString, True)
        Me.FixLineLengthCommand = New CommandHandler(AddressOf OnFixLineLength, True)
        Me.QualityTrimmerCommand = New CommandHandler(AddressOf OnQualityTrimmer, True)
        Me.SequenceCompareCommand = New CommandHandler(AddressOf OnSequenceCompare, True)
        Me.FindPairingsCommand = New CommandHandler(AddressOf OnFindPairings, True)
        Me.SequenceInvertCommand = New CommandHandler(AddressOf OnInvertSequence, True)
        Me.CodonifyCommand = New CommandHandler(AddressOf OnCodonify, True)
        Me.InsertStartCommand = New CommandHandler(AddressOf InsertStart, True)
        Me.InsertEndCommand = New CommandHandler(AddressOf InsertEnd, True)
        Me.SelectStartPositionCommand = New CommandHandler(AddressOf OnSelectStartPosition, True)
        Me.SelectEndPositionCommand = New CommandHandler(AddressOf OnSelectEndPosition, True)
        Me.AboutCommand = New CommandHandler(AddressOf OnAbout, True)
        Me.Data = New MasterRecord()

        Me.Data.SpecifiedStartPosition = 1
        Me.Data.SpecifiedEndPosition = Me.Data.SpecifiedStartPosition + Settings.InitialLoadLength - 1

        Me.Species = New List(Of String)

        ' If the application was started with parameters
        Dim dep As New DependencyObject()
        If Not DesignerProperties.GetIsInDesignMode(dep) Then
            Dim args = Environment.GetCommandLineArgs()
            If args.Length > 1 Then
                StartTime = DateTime.Now
                Me.Data.FileName = args(1)
                LoadData()
            End If
        End If

    End Sub

    Private Sub UpdateStatusBar()
        Me.StatusText = "File loaded (" & DateTime.Now.Subtract(StartTime).Seconds & " s)"
        Me.SpeciesCount = Me.Species.Count
        Me.TotalSequenceLength = Me.Data.FilePositionCount
        Me.DisplayLength = Me.Data.DisplayPositionCount
        NotifyPropertyChanged("StatusText")
        NotifyPropertyChanged("SpeciesCount")
        NotifyPropertyChanged("TotalSequenceLength")
        NotifyPropertyChanged("DisplayLength")
    End Sub

    Private Sub NotifyPopulating()
        Me.StatusText = "Populating..."
        NotifyPropertyChanged("StatusText")
        Me.HasData = True
        NotifyPropertyChanged("HasData")
    End Sub

#Region " Add rows asynchronously "

    Delegate Sub AddRowDelegate(ByVal row As PositionRow)
    Private Sub AddRow(ByVal row As PositionRow)
        If (Application.Current.Dispatcher.CheckAccess()) Then
            ' rows can only be added to an ObservableCollection on the Dispatcher's thread
            Me.PositionRows.Add(row)
        Else
            Dim method As New AddRowDelegate(AddressOf AddRow)
            Application.Current.Dispatcher.Invoke(method, DispatcherPriority.Send, row)
        End If
    End Sub

    Private InsertionIndex As Integer
    Private Sub RecursivelyAddRowsAsychronously()
        If InsertionIndex >= Me.Data.PositionRows.Count Then
            Me.NotifyPropertyChanged("PositionRows")
            UpdateStatusBar()
            Return
        End If

        ' Must add these on the UI thread - YES!!         
        If Application.Current Is Nothing Then Return
        AddRow(Me.Data.PositionRows(InsertionIndex))
        InsertionIndex += 1

        ' This is the major delay (only once this is finished do we want to add the next batch
        Dim method As New SingleStringDelegate(AddressOf NotifyPropertyChanged)
        If Application.Current Is Nothing Then Return
        Application.Current.Dispatcher.Invoke(method, DispatcherPriority.Send, "PositionRows")
        'While Not result.Status = DispatcherOperationStatus.Completed
        '    System.Threading.Thread.Sleep(10)
        'End While
        ' Wait 50 milliseconds to make the UI appear responsive      
        'System.Threading.Thread.Sleep(1)
        ' Now add the next row through the same function recursively...
        RecursivelyAddRowsAsychronously()
    End Sub

#End Region

#Region " Add rows "

    Delegate Sub MethodDelegate()
    Private Sub AddRows()
        For i As Integer = 0 To Me.Data.PositionRows.Count - 1
            Me.PositionRows.Add(Me.Data.PositionRows(i))
        Next i
        NotifyPropertyChanged("PositionRows")
        ' Only update the status bar once the layout is complete. Having a low priority (DispatcherPriority.ContextIdle) will ensure this is executed last
        Application.Current.Dispatcher.Invoke(New MethodDelegate(AddressOf UpdateStatusBar), DispatcherPriority.ContextIdle)
    End Sub

#End Region

#Region " Loading and saving data "

    Private Sub ResetData()
        If Not Me.PositionRows Is Nothing Then
            Me.PositionRows.Clear()
        End If
        If Not Me.Species Is Nothing Then
            Me.Species.Clear()
        End If
    End Sub

    Private Sub LoadSpecies()
        For Each s In Me.Data.Sequences
            Me.Species.Add(s.SpeciesName)
        Next
    End Sub

    Private Sub LoadViewModelData()
        Me.Data.PivotData()
        ResetData()
        LoadSpecies()

        If Settings.LoadAsynchronously Then
            ' Call RecursivelyAddRowsAsychronously on a separate thread (Not sure if I still want to do this)
            NotifyPopulating()
            Me.PositionRows = New ObservableCollection(Of PositionRow)
            InsertionIndex = 0
            Dim t As New System.Threading.Thread(AddressOf RecursivelyAddRowsAsychronously)
            t.Start()
        Else
            Me.PositionRows = New ObservableCollection(Of PositionRow)
            NotifyPopulating()
            Dim t As New System.Threading.Thread(AddressOf AddRows)
            t.Start()
        End If
    End Sub

    Private Sub LoadData()
        Me.Data.ReadSequences()
        LoadViewModelData()
    End Sub

    Private Sub OpenFile()
        Dim OpenFileDialog As New OpenFileDialog()
        OpenFileDialog.RestoreDirectory = False
        OpenFileDialog.InitialDirectory = Settings.LastOpenDirectory
        If OpenFileDialog.ShowDialog() Then
            Settings.LastOpenDirectory = Common.GetDirectory(OpenFileDialog.FileName)
            StartTime = DateTime.Now
            Me.Data.FileName = OpenFileDialog.FileName
            LoadData()
        End If
    End Sub

    Private Sub SaveFileAs(Optional ByVal JustVisibleRegion As Boolean = True)
        Dim SaveFileDialog As New SaveFileDialog()
        SaveFileDialog.RestoreDirectory = False
        SaveFileDialog.InitialDirectory = Settings.LastSaveDirectory
        SaveFileDialog.AddExtension = True
        SaveFileDialog.OverwritePrompt = True
        SaveFileDialog.DefaultExt = ".fasta"
        SaveFileDialog.Filter = "FASTA files (*.fasta)|*.fasta"
        If SaveFileDialog.ShowDialog() Then
            'SaveData(SaveFileDialog.FileName)
            Settings.LastSaveDirectory = Common.GetDirectory(SaveFileDialog.FileName)
            Me.Data.FileName = SaveFileDialog.FileName
            If JustVisibleRegion Then
                Me.Data.SaveVisibleToFile()
            Else
                Me.Data.SaveDatasetToFile()
            End If
        End If
    End Sub

    Private Sub SaveFile(Optional ByVal JustVisibleRegion As Boolean = True)
        If JustVisibleRegion Then
            Me.Data.SaveVisibleToFile()
        Else
            Me.Data.SaveDatasetToFile()
        End If
    End Sub

#End Region

    Public Function SelectNucleobase(ByVal positionIndex As Integer, ByVal speciesIndex As Integer) As Boolean
        ' If the position and species is available within the current dataset being displayed then select it and Return True else Return False
        If positionIndex >= Me.Data.DisplayStartPosition And positionIndex <= Me.Data.DisplayEndPosition Then
            ' We want to set SelectedPosition and SelectedNucleobase
            For Each row In Me.PositionRows
                ' For Each Position row
                If row.Positions(row.Positions.Count - 1).FileIndex >= positionIndex Then
                    ' If the last position in this row is gteq positionIndex then positionIndex is to be found in this row
                    For Each pos In row.Positions
                        If pos.FileIndex = positionIndex Then
                            Me.SelectedPosition = pos
                            Me.SelectedNucleobase = pos.Species(speciesIndex)
                            Me.SelectedRow = row
                            RaiseEvent SelectedPositionChanged()
                            Return True
                        End If
                    Next pos
                End If
            Next row

        End If
        Return False
    End Function

    Public Sub CloseFindDialog()
        If Not Me.Find Is Nothing Then
            Me.Find.Close()
        End If
    End Sub

#Region " Command handlers "

    Private Sub OnOpen()
        OpenFile()
    End Sub

    Private Sub OnSave()
        If Me.HasData Then
            SaveFile(False)
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnSaveVisibleAs()
        If Me.HasData Then
            SaveFileAs(True)
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnSaveDatasetAs()
        If Me.HasData Then
            SaveFileAs(False)
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Public Sub RefreshWithoutReloading()
        LoadViewModelData()
    End Sub

    Private Sub OnOverview()
        Dim win = New SequenceOverviewGenerator(Me)
        win.Show()
    End Sub

    Private Sub OnCleanString()
        Dim win = New CleanString()
        win.Show()
    End Sub

    Private Sub OnFixLineLength()
        Dim win = New FixLineLength()
        win.Show()
    End Sub

    Private Sub OnQualityTrimmer()
        Dim win = New QualityTrimmer()
        win.Show()
    End Sub

    Private Sub OnInvertSequence()
        Dim win = New SequenceInverter()
        win.Show()
    End Sub

    Private Sub OnCodonify()
        Dim win = New Codonify()
        win.Show()
    End Sub

    Private Sub OnFind()
        If Me.HasData Then
            Me.Find = New Find(Me)
            Me.Find.Show()
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnSequenceCompare()
        If Me.HasData Then
            'Me.Find = New Find(Me)
            'Me.Find.Show()
            Dim sc As New SequenceComparison(Me)
            sc.Show()
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnFindPairings()
        If Me.HasData Then            
            Me.Data.FindAllPairings()
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnHighlightRange()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        If Me.Data.SpecifiedStartPosition Is Nothing Or Me.Data.SpecifiedEndPosition Is Nothing Then
            MessageBox.Show("Please ensure that you have selected a start position and an end position", "One or more bounds not specified", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SetRangeHighlight(True)
    End Sub

    Private Sub OnDimRange()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        If Me.Data.SpecifiedStartPosition Is Nothing Or Me.Data.SpecifiedEndPosition Is Nothing Then
            MessageBox.Show("Please ensure that you have selected a start position and an end position", "One or more bounds not specified", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SetRangeHighlight(False)
    End Sub

    Private Sub OnCopyRange()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        If Me.Data.SpecifiedStartPosition Is Nothing Or Me.Data.SpecifiedEndPosition Is Nothing Then
            MessageBox.Show("Please ensure that you have selected a start position and an end position", "One or more bounds not specified", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        Dim speciesIndex As Integer = 0
        If Me.Data.Sequences.Count > 1 Then
            If Me.SelectedNucleobase Is Nothing Then
                MessageBox.Show("Please ensure that you have selected a sequence to copy from", "Sequence not selected", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If
            speciesIndex = Me.SelectedNucleobase.SpeciesIndex
        End If
        Me.Data.CopySequenceRangeToClipboard(speciesIndex)
    End Sub

    Public Sub ToggleRange()
        If Me.Data.SpecifiedStartPosition Is Nothing Or Me.Data.SpecifiedEndPosition Is Nothing Then
            MessageBox.Show("Please ensure that you have selected a start position and an end position", "One or more bounds not specified", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SetRangeHighlight(Not Me.SelectedPosition.Highlight)
    End Sub

    Private Sub OnHighlightAll()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SetAllHighlight(True)
    End Sub

    Private Sub OnDimAll()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SetAllHighlight(False)
    End Sub

    Private Sub OnInsertGaps()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        If Me.SelectedPosition Is Nothing Then
            MessageBox.Show("Please select a position to insert to insert gaps before first", "Please select a position first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Dim ge As New GapInserter(Me)
        ge.Show()
    End Sub

    Private Sub OnCountHighlighted()
        If Not Me.HasData Then
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.CountHighlighted()
        MessageBox.Show(String.Format("Out of a total of {0} positions being displayed...{1}{1}Highlighted: {2}{1}Dimmed: {3}", Me.Data.DisplayPositionCount, vbCrLf, Me.Data.HighlightCount, Me.Data.DimCount), "Highlight count", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub SwapSpecifiedPositions()
        Dim temp As Integer = Me.Data.SpecifiedEndPosition
        Me.Data.SpecifiedEndPosition = Me.Data.SpecifiedStartPosition
        Me.Data.SpecifiedStartPosition = temp
    End Sub

    Public Sub InsertStart(Optional ByVal SwapIfGreaterThan As Boolean = False)
        If Me.SelectedPosition Is Nothing Then
            MessageBox.Show("Please select a position to insert first", "No position selected", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SpecifiedStartPosition = Me.SelectedPosition.FileIndex

        If SwapIfGreaterThan And Me.Data.SpecifiedStartPosition > Me.Data.SpecifiedEndPosition Then
            ' If start is greater than end then swap them
            SwapSpecifiedPositions()
        End If
    End Sub

    Public Sub InsertEnd(Optional ByVal SwapIfLessThan As Boolean = False)
        If Me.SelectedPosition Is Nothing Then
            MessageBox.Show("Please select a position to insert first", "No position selected", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        Me.Data.SpecifiedEndPosition = Me.SelectedPosition.FileIndex

        If SwapIfLessThan And Me.Data.SpecifiedStartPosition > Me.Data.SpecifiedEndPosition Then
            ' If start is greater than end then swap them
            SwapSpecifiedPositions()
        End If
    End Sub

    Private Sub OnSelectStartPosition()
        If Me.HasData Then
            SelectNucleobase(Me.Data.SpecifiedStartPosition, 0)
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnSelectEndPosition()
        If Me.HasData Then
            SelectNucleobase(Me.Data.SpecifiedEndPosition, 0)
        Else
            MessageBox.Show("Please load data first", "Please load data first", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub

    Private Sub OnAbout()
        Dim win = New About()
        win.Show()
    End Sub

#End Region

    Delegate Sub SingleStringDelegate(ByVal str As String)
    Public Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
