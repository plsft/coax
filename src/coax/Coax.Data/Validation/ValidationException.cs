using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helix.Utility;

namespace Coax.Data.Validation
{
    public sealed class ValidationException : Exception
    {
        public ValidationException() : base()
        {
        }

        public ValidationException(string error, Exception ex) : base(error,ex)
        {
            Logger.Log("ValidationException", "{0}: {1}", Logger.LogType.Error, error, ex);
        }
    }
}
