using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Sys.Workflow.Validation;

namespace Sys.Workflow.Engine.Exceptions
{
    [Serializable]
    public class ActivitiValidationException : Exception
    {
        private readonly IList<ValidationError> validationErrors;

        public ActivitiValidationException(IList<ValidationError> validationErrors) :
            base(string.Join("\r\n", validationErrors.Select(x => $"{x.ActivityName}-{x.DefaultDescription}")))
        {
            this.validationErrors = validationErrors;
        }

        public IList<ValidationError> ValidationErrors => validationErrors;

        protected ActivitiValidationException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}