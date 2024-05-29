using Workflows.Models;

namespace Workflows.ViewModels
{
    public class SummaryViewModel
    {
        public Intern Intern { get; set; }
        public Requisition Requisition { get; set; }
        public List<Approval> ApprovalSteps { get; set; }
        public List<Document> DocumentList { get; set; }
    }
}
