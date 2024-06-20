using Workflows.Models;

namespace Workflows.ViewModels
{
    public class RequisitionDetailsViewModel
    {
        public Requisition? Requisition { get; set; }
        public Intern? Intern { get; set; }
        public List<ApprovalViewModel>? Approvals { get; set; }
        public List<DocumentViewModel>? Documents { get; set; }

        // Additional property for DepartmentName
        public string? DepartmentName { get; set; }
        public string? AddedBy { get; set; }
    }
}
