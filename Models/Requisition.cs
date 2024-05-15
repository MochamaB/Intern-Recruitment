using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workflows.Models
{
    public class Requisition
    {
        public int Id { get; set; }

        [Column("departmentCode")]
        public int DepartmentCode { get; set; }

        [Column("PayrollNo")]
        public string PayrollNo { get; set; } = null!;

        public int Intern_id { get; set; }
        public string? Station {  get; set; }

        public string? Status {  get; set; }

        [Display(Name = "Start Date")]
        public DateTime Start_Date { get; set; }

        [Display(Name = "End Date")]
        public DateTime End_Date { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        ////Relationships Department
        //     [ForeignKey("Department")]
        public Department Department { get; set; }

        //
        ////Relationships Employee


        ////Relationships
        //   [ForeignKey("Intern")]
        //    [Column("Intern_id")]
        //    [Display(Name = "Intern")]

        //     public Intern Intern { get; set; }

    }
}
