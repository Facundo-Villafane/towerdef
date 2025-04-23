/// <summary>
/// Command pattern interface for player actions
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command
    /// </summary>
    void Execute();
    
    /// <summary>
    /// Undoes the command (if possible)
    /// </summary>
    void Undo();
    
    /// <summary>
    /// Checks if the command can be executed
    /// </summary>
    bool CanExecute();
}