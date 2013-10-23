using System.Collections.Generic;

namespace SelfValidatingModel.CascadeValidation
{
    public interface ICascadeValidator
    {
        IDictionary<string, SelfValidatingModelBase> GetSelfValidatingProperties(SelfValidatingModelBase model);
    }
}
