using System.ComponentModel.DataAnnotations;
using Workflows.Data;

namespace Workflows.Attributes
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                // You can return ValidationResult.Success if you want to consider null as a valid value,
                // or return new ValidationResult(GetErrorMessage()) if you want to consider null as an invalid value.
                return ValidationResult.Success;
            }
            var _context = (WorkflowsContext)validationContext.GetService(typeof(WorkflowsContext));
            var entity = _context.Intern.SingleOrDefault(e => e.Email == value.ToString());

            if (entity != null)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return "Intern with this Email already exists.";
        }
    }

}
