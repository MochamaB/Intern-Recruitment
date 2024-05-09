﻿
using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class Intern
    {
        public int Id { get; set; }
        public string? Department_id { get; set; }

        [Display(Name = "First Name")]
        [DataType(DataType.Text)]  // used to display the value as text
        [Required]
        public string? Firstname { get; set; }

        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        [Required]
        [EmailAddress]
        public string? Lastname { get; set; }
        public string? Othernames { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [Required]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone]
        [Required]
        public string? PhoneNumber { get; set;}
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
