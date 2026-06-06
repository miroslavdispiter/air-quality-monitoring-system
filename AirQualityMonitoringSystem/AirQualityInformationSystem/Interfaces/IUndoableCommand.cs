namespace AirQualityInformationSystem.Interfaces
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }
}