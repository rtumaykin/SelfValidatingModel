using System.Collections.Generic;

namespace SelfValidatingModel.CascadeValidation
{
    public interface ICascadeValidator
    {
        IDictionary<string, IList<string>> ApplyCascadeValidationRules(SelfValidatingModelBase model);
    }
}
