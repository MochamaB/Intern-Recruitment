using System.ComponentModel.DataAnnotations;

namespace Workflows.Attributes
{
    public class ValidFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file == null || file.Length == 0)
            {
                return new ValidationResult(ErrorMessage ?? "Please select a valid file.");
            }

            return ValidationResult.Success;
        }
    }
}
