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
            var _result = _validator.ApplyCascadeValidationRules(this);
            return _result;
        }

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

        public ValidationException Exception
        {
            get
            {
                var _validationErrors = ApplyValidationRules();
                return _validationErrors != null && _validationErrors.Count == 0 ? null : new ValidationException(_validationErrors);
            }
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

        private IDictionary<string, IList<ValidationRule>> validationRules;
        private IDictionary<string, IList<ValidationRule>> ValidationRules
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref validationRules, () => new Dictionary<string, IList<ValidationRule>>());
                return validationRules;
            }
        }

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

        protected abstract void CreateValidationRules();

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
