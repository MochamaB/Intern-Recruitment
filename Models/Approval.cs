using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class Approval
    {
        public int Id { get; set; }

        public int Requisition_id { get; set; }

        [Column("departmentCode")]
        [Display(Name = "Department")]
        public int DepartmentCode { get; set; }

        [Display(Name = "Approval Step")]
        public string ApprovalStep { get; set; } = null!;
        public string? PayrollNo { get; set; } = null!;

        public string? ApprovalStatus { get; set; }

        public string? ApprovalComment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
