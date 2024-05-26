using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workflows.ViewModels
{
    public class CreateInternViewModel
    {
          public int Id { get; set; }

      
        [Display(Name = "Department")]
        [Required]
        public int DepartmentCode { get; set; }


        [Display(Name = "ID Number")]
        [Required]
        public long Idnumber { get; set; }

        [Display(Name = "First Name")]
        [DataType(DataType.Text)]  // used to display the value as text
        [Required]
        public string? Firstname { get; set; }

        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        [Required]
     
        public string? Lastname { get; set; }
        public string? Othernames { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        [Required]
        public string? PhoneNumber { get; set;}
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public SelectList DepartmentItems { get; internal set; }
    }
}
