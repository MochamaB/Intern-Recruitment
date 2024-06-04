using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Workflows.Attributes;

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

        [Display(Name = "Approver")]
        public string? PayrollNo { get; set; } = null!;

        [Display(Name = "Status")]
        public string? ApprovalStatus { get; set; }

        [Display(Name = "Comment")]
        [RequiredIfRejected]
        public string? ApprovalComment { get; set; }

        public DateTime CreatedAt { get; set; }


        [Display(Name = "Approval Date")]
        [DataType(DataType.Date)]
        public DateTime? UpdatedAt { get; set; }
    }
}
