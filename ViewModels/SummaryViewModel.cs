using Workflows.Models;

namespace Workflows.ViewModels
{

    public class SummaryViewModel
    {
        public Intern Intern { get; set; }
        public Requisition Requisition { get; set; }
        public List<ApprovalViewModel> ApprovalSteps { get; set; }
        public List<DocumentViewModel> DocumentList { get; set; }

        // Additional property for DepartmentName
        public string DepartmentName { get; set; }
    }
    public class ApprovalViewModel
    {
        public Approval Approval { get; set; }
        public string EmployeeName { get; set; } // EmployeeName
    }



}
