namespace Sys.Workflow.validation
{
    public interface IProcessValidatorFactory
    {
        IProcessValidator CreateProcessValidator();
    }
}