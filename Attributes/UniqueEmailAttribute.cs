using System.ComponentModel.DataAnnotations;
using Workflows.Data;
using Workflows.Models;

namespace Workflows.Attributes
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var _context = (WorkflowsContext)validationContext.GetService(typeof(WorkflowsContext));
            var intern = validationContext.ObjectInstance as Intern;

            if (intern == null)
            {
                throw new ArgumentException("This attribute can only be used on Intern objects");
            }

            // Check if any other intern (excluding the current one) has this email
            var entity = _context.Intern.FirstOrDefault(e => e.Email == value.ToString() && e.Id != intern.Id);

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
