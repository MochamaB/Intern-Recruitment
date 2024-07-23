
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workflows.Attributes;

namespace Workflows.Models
{
    public class Intern
    {
        public int Id { get; set; }

        [Column("departmentCode")]
        [Display(Name = "Department")]
        [Required]
        public int DepartmentCode { get; set; }


        [Display(Name = "ID Number")]
        [Required]
        [UniqueIdNumber]
        public long? Idnumber { get; set; } 

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
        [UniqueEmail]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        [Required]
        [UniquePhonenumber]
        public string? PhoneNumber { get; set;}
        public string? Status { get; set; }


        [Display(Name = "Certification")]
        [Required]
        public string? Certification { get; set; }

        [Display(Name = "Course")]
        [Required]
        public string? Course { get; set; }

        [Display(Name = "School")]
        [Required]
        public string? School { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public List<Requisition>? Requisition { get; set; }


    }
}
