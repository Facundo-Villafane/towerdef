using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages command execution and undo functionality
/// </summary>
public class CommandManager : Singleton<CommandManager>
{
    // Stack for tracking executed commands (for undo)
    private Stack<ICommand> commandHistory = new Stack<ICommand>();
    
    // Maximum number of commands to remember for undo
    [SerializeField] private int maxUndoCommands = 20;
    
    /// <summary>
    /// Executes a command
    /// </summary>
    public bool ExecuteCommand(ICommand command)
    {
        if (command.CanExecute())
        {
            command.Execute();
            
            // Add to history for undo
            commandHistory.Push(command);
            
            // Trim history if it exceeds maximum size
            if (commandHistory.Count > maxUndoCommands)
            {
                // Create a new stack with only the newest commands
                Stack<ICommand> newHistory = new Stack<ICommand>();
                ICommand[] commands = commandHistory.ToArray();
                
                for (int i = 0; i < maxUndoCommands; i++)
                {
                    newHistory.Push(commands[i]);
                }
                
                commandHistory = newHistory;
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Undoes the last command
    /// </summary>
    public bool Undo()
    {
        if (commandHistory.Count > 0)
        {
            ICommand command = commandHistory.Pop();
            command.Undo();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Clears command history
    /// </summary>
    public void ClearHistory()
    {
        commandHistory.Clear();
    }
    
    /// <summary>
    /// Gets whether undo is available
    /// </summary>
    public bool CanUndo()
    {
        return commandHistory.Count > 0;
    }
}