using System;
using System.Collections.Generic;

namespace AirQualityInformationSystem.Helpers
{
    public interface IUndoRedoAction
    {
        void Execute();
        void Undo();
    }

    public class UndoRedoManager
    {
        private readonly Stack<IUndoRedoAction> _undoStack = new Stack<IUndoRedoAction>();
        private readonly Stack<IUndoRedoAction> _redoStack = new Stack<IUndoRedoAction>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void ExecuteAction(IUndoRedoAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var action = _undoStack.Pop();
            action.Undo();
            _redoStack.Push(action);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var action = _redoStack.Pop();
            action.Execute();
            _undoStack.Push(action);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }

    // Akcija za dodavanje
    public class AddAction<T> : IUndoRedoAction
    {
        private readonly IList<T> _collection;
        private readonly T _item;

        public AddAction(IList<T> collection, T item)
        {
            _collection = collection;
            _item = item;
        }

        public void Execute()
        {
            _collection.Add(_item);
        }

        public void Undo()
        {
            _collection.Remove(_item);
        }
    }

    // Akcija za brisanje
    public class RemoveAction<T> : IUndoRedoAction
    {
        private readonly IList<T> _collection;
        private readonly T _item;
        private int _index;

        public RemoveAction(IList<T> collection, T item)
        {
            _collection = collection;
            _item = item;
        }

        public void Execute()
        {
            _index = _collection.IndexOf(_item);
            _collection.Remove(_item);
        }

        public void Undo()
        {
            _collection.Insert(_index, _item);
        }
    }

    // Akcija za izmenu
    public class ModifyAction<T> : IUndoRedoAction
    {
        private readonly Action _executeAction;
        private readonly Action _undoAction;

        public ModifyAction(Action executeAction, Action undoAction)
        {
            _executeAction = executeAction;
            _undoAction = undoAction;
        }

        public void Execute()
        {
            _executeAction();
        }

        public void Undo()
        {
            _undoAction();
        }
    }
}