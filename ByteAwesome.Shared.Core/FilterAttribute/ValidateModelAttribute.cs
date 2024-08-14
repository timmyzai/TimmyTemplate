using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace ByteAwesome
{
    public class PositiveNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                return value is IConvertible convertible && Convert.ToDouble(convertible, CultureInfo.InvariantCulture) > 0;
            }
            catch
            {
                return false;
            }
        }
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a positive number.";
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAtLeastOneAttribute : ValidationAttribute
    {
        private readonly string[] _otherPropertyNames;

        public RequiredAtLeastOneAttribute(params string[] otherPropertyNames)
        {
            _otherPropertyNames = otherPropertyNames;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isAnyOtherPropertyNotNull = _otherPropertyNames.Any(propertyName =>
            {
                var propertyInfo = validationContext.ObjectType.GetProperty(propertyName);
                if (propertyInfo is null)
                {
                    throw new ArgumentException($"Property with name {propertyName} not found");
                }

                var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);
                return propertyValue is not null;
            });

            // If any of the listed properties are not null, the validation is successful
            if (isAnyOtherPropertyNotNull)
            {
                return ValidationResult.Success;
            }

            // If the current property is also null, then return a validation error
            if (value is null)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"At least one of the properties or {name} must be provided.";
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _triggerPropertyName;

        public ConditionalRequiredAttribute(string triggerPropertyName)
        {
            _triggerPropertyName = triggerPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var triggerPropertyInfo = validationContext.ObjectType.GetProperty(_triggerPropertyName);
            if (triggerPropertyInfo is null)
            {
                return ValidationResult.Success;
            }

            var triggerValue = triggerPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (triggerValue is not null && value is null)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"{name} is required when '{_triggerPropertyName}' is provided.";
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class EnsureNotEqualToAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public EnsureNotEqualToAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherPropertyInfo is null)
            {
                return new ValidationResult($"Property with name {_otherPropertyName} not found.");
            }

            var otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (value is not null && value.Equals(otherPropertyValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The value of {name} must not be the same as the value of {_otherPropertyName}.";
        }
    }

}