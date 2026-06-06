using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Operations
{
    public abstract class AbstractSaveOperation
    {
        protected readonly ILoggerService logger;

        protected AbstractSaveOperation(ILoggerService logger)
        {
            this.logger = logger;
        }

        public void TemplateMethod()
        {
            Validate();
            SaveToRepository();
            Log();
        }

        protected abstract void Validate();
        protected abstract void SaveToRepository();
        protected abstract void Log();
    }
}