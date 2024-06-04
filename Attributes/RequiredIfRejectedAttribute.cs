using System.ComponentModel.DataAnnotations;
using Workflows.Models;

namespace Workflows.Attributes
{
    public class RequiredIfRejectedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var approval = (Approval)validationContext.ObjectInstance;
            if (approval.ApprovalStatus != null && approval.ApprovalStatus == "Rejected" && string.IsNullOrWhiteSpace((string)value))
            {
                return new ValidationResult("Approval comment is required when status is Rejected.");
            }
            return ValidationResult.Success;
        }
    }
}
