using System.ComponentModel.DataAnnotations;

namespace Workflows.Attributes
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (DateTime)value;

            var comparisonValue = (DateTime)validationContext.ObjectType.GetProperty(_comparisonProperty).GetValue(validationContext.ObjectInstance);

            if (currentValue < comparisonValue)
            {
                return new ValidationResult(ErrorMessage = "End date must be greater than start date");
            }

            return ValidationResult.Success;
        }
    }

}
