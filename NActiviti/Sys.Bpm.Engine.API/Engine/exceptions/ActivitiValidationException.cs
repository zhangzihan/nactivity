using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sys.Workflow.Validation;

namespace Sys.Workflow.Engine.Exceptions
{
    [Serializable]
    public class ActivitiValidationException : Exception
    {
        private IList<ValidationError> validationErrors;

        public ActivitiValidationException(IList<ValidationError> validationErrors)
        {
            this.validationErrors = validationErrors;
        }

        public IList<ValidationError> ValidationErrors => validationErrors;
    }
}