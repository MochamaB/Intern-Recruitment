using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workflows.Attributes;

namespace Workflows.Models
{
    public class Requisition
    {
        public int Id { get; set; }

        [Column("departmentCode")]
        [Display(Name = "Department")]
        [Required]
        public int DepartmentCode { get; set; }

        [Column("PayrollNo")]
        [Display(Name = "Added By")]
        public string? PayrollNo { get; set; }

        [Display(Name = "Intern")]
        [Required]
        public int Intern_id { get; set; }

        [Required]
        public string? Station {  get; set; }

        public string? Status {  get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [Required]
        public DateTime Start_Date { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [Required]
        [DateGreaterThan("Start_Date")]
        public DateTime End_Date { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Non-mapped properties for related data
        [NotMapped]
        public Intern? Intern { get; set; }

        [NotMapped]
        public Department? Department { get; set; }

        [NotMapped]
        public EmployeeBkp? Employee { get; set; }

        [NotMapped]
        public List<Approval>? Approvals { get; set; }



    }
}
