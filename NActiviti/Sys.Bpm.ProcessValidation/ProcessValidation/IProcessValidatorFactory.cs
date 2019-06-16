namespace Sys.Workflow.Validation
{
    public interface IProcessValidatorFactory
    {
        IProcessValidator CreateProcessValidator();
    }
}