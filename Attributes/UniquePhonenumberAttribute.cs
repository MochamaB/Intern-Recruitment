using System.ComponentModel.DataAnnotations;
using Workflows.Data;
using Workflows.Models;

namespace Workflows.Attributes
{
   
    public class UniquePhonenumberAttribute : ValidationAttribute
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
            var intern = validationContext.ObjectInstance as Intern;

            if (intern == null)
            {
                throw new ArgumentException("This attribute can only be used on Intern objects");
            }
            var entity = _context.Intern.SingleOrDefault(e => e.PhoneNumber == value.ToString() && e.Id != intern.Id);

            if (entity != null)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return "Intern with this Phone Number already exists.";
        }
    }

}
