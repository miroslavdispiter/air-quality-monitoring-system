using System.Collections.Generic;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Commands
{
    public class CommandManager
    {
        private readonly Stack<IUndoableCommand> undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> redoStack = new Stack<IUndoableCommand>();

        public void ExecuteCommand(IUndoableCommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;

            var command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;

            var command = redoStack.Pop();
            command.Execute();
            undoStack.Push(command);
        }
    }
}