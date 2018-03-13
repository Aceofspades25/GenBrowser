Public Class CommandHandler
    Implements ICommand

    Private _action As Action
    Private _canExecute As Boolean

    Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
        Return _canExecute
    End Function

    Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged

    Public Sub Execute(parameter As Object) Implements ICommand.Execute
        _action()
    End Sub

    Public Sub New(action As Action, canExecute As Boolean)
        _action = action
        _canExecute = canExecute
    End Sub
End Class