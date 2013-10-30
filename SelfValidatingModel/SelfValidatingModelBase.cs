using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SelfValidatingModel.CascadeValidation;

namespace SelfValidatingModel
{
    public abstract class SelfValidatingModelBase
    {
        /// <summary>
        /// static collection of type-specific classes that implement interface which validates child properties of a model
        /// </summary>
        private static ConcurrentDictionary<Type, ICascadeValidator> subModelValidators;

        private static ConcurrentDictionary<Type, ICascadeValidator> SubModelValidators
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref subModelValidators,
                    () => new ConcurrentDictionary<Type, ICascadeValidator>());
                return subModelValidators;
            }
        }

        /// <summary>
        /// Retrieves the saved instance of a type-specific validator or creates new instance, saves it and does validation
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, IList<string>> ValidateSubModels()
        {
            var _validator = EnsureValidator();
            var _result = _validator.GetSelfValidatingProperties(this);
            var _returnObject = new Dictionary<string, IList<string>>();
            if (_result != null)
            {
                foreach (var _modelBase in _result.Where(model => model.Value != null))
                {
                    foreach (var _validationRule in _modelBase.Value.ApplyValidationRules())
                    {
                        _returnObject.Add(_modelBase.Key + "." + _validationRule.Key, _validationRule.Value);
                    }
                }
            }
            return _returnObject;
        }

        /// <summary>
        /// Ensures that the validator for this class exists in the static collection. If it does not, then it creates and adds validator to the collection for reuse.
        /// </summary>
        /// <returns></returns>
        private ICascadeValidator EnsureValidator()
        {
            ICascadeValidator _subModelValidator;
            var _type = GetType();
            if (!SubModelValidators.TryGetValue(_type, out _subModelValidator))
            {
                _subModelValidator = CascadeValidatorCodeFactory.CompileAndGetInstanceOfValidator(_type);
                if (!SubModelValidators.TryAdd(_type, _subModelValidator))
                {
                    var _savedValidator = _subModelValidator;
                    // What else can happen if not someone has already inserted validator instance?
                    if (!SubModelValidators.TryGetValue(_type, out _subModelValidator))
                    {
                        // if for any reason it fails, return this validator, so at least we worked here not for just nothing
                        return _savedValidator;
                    }
                }
            }
            return _subModelValidator;
        }

        /// <summary>
        /// Throws a validation error if the model is invalid
        /// </summary>
        public void ThrowIfInvalid()
        {
            var _validationErrors = ApplyValidationRules();
            if (_validationErrors != null && _validationErrors.Count > 0)
                throw new ValidationException(_validationErrors);
        }

        protected SelfValidatingModelBase()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            CreateValidationRules();
        }

        private class ValidationRule
        {
            internal Func<bool> ValidationMethod { get; set; }
            internal Func<string> ErrorMessageMethod { get; set; }
        }

        /// <summary>
        /// Holds a list of validation rules per public property
        /// </summary>
        private IDictionary<string, IList<ValidationRule>> validationRules;
        private IDictionary<string, IList<ValidationRule>> ValidationRules
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref validationRules, () => new Dictionary<string, IList<ValidationRule>>());
                return validationRules;
            }
        }

        /// <summary>
        /// Adds a new validation rule to the collection
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="validationMethod">method to perform validation</param>
        /// <param name="errorMessageMethod">error message that will be returned if validation fails</param>
        protected void AddValidationRule(string propertyName, Func<bool> validationMethod, Func<string> errorMessageMethod)
        {
            if (ValidationRules.ContainsKey(propertyName))
            {
                ValidationRules[propertyName].Add(new ValidationRule { ErrorMessageMethod = errorMessageMethod, ValidationMethod = validationMethod });
            }
            else
            {
                var _validationRulesPerProperty = new List<ValidationRule>
                    {
                        new ValidationRule {ErrorMessageMethod = errorMessageMethod, ValidationMethod = validationMethod}
                    };
                ValidationRules.Add(propertyName, _validationRulesPerProperty);
            }
        }

        /// <summary>
        /// Abstract method that needs to be overwritten in each derived class to create the set of validation rules
        /// </summary>
        protected abstract void CreateValidationRules();

        /// <summary>
        /// Applies defined validation rules, plus processes all public properties that are also derived from the SelfValidatingModelBase class
        /// </summary>
        /// <returns>A dictionary with field name / list of error messages</returns>
        private IDictionary<string, IList<string>> ApplyValidationRules()
        {
            var _returnObject = new Dictionary<string, IList<string>>();

            foreach (var _validationRule in ValidationRules)
            {
                foreach (var _rule in _validationRule.Value.Where(rule => rule.ValidationMethod()))
                {
                    if (_returnObject.ContainsKey(_validationRule.Key))
                    {
                        _returnObject[_validationRule.Key].Add(_rule.ErrorMessageMethod());
                    }
                    else
                    {
                        _returnObject.Add(_validationRule.Key, new List<string> { _rule.ErrorMessageMethod() });
                    }
                }
            }

            foreach (var _subModelValidationResult in ValidateSubModels())
            {
                _returnObject.Add(_subModelValidationResult.Key, _subModelValidationResult.Value);
            }

            return _returnObject;
        }

        public bool IsValid
        {
            get
            {
                return ApplyValidationRules().Count == 0;
            }
        }
    }
}
