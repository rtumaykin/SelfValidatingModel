using System;
using System.Collections.Generic;
using System.Linq;

namespace SelfValidatingModel
{
    public sealed class ValidationException : Exception
    {   
        /// <summary>
        /// Internal constructor - the initialization of this exception may only be done internally from the ModelWithValidation class
        /// </summary>
        /// <param name="validationErrors"></param>
        internal ValidationException(IDictionary<string, IList<string>> validationErrors)
            : base((validationErrors == null || validationErrors.Count == 0)
                       ? ""
                       : (
                             validationErrors.Count == 1 && validationErrors.FirstOrDefault().Value.Count == 1
                                 ? String.Format("Validation of property \"{0}\" failed with reason: \"{1}\".",
                                                 validationErrors.FirstOrDefault().Key, validationErrors.FirstOrDefault().Value[0])
                                 : "Multiple validation errors occurred"))
        {
            if (validationErrors == null || validationErrors.Count == 0 || validationErrors.FirstOrDefault().Value.Count == 0)
            {
                throw new ArgumentNullException("validationErrors", "Should be not null or empty");
            }

            foreach (var _validationError in validationErrors)
            {
                Data.Add(_validationError.Key, _validationError.Value);
            }

        }
    }
}
