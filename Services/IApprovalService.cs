using Workflows.Models;

namespace Workflows.Services
{
    public interface IApprovalService
    {
        List<Approval> CreateApprovalSteps(Requisition requisition);
    }
    public class ApprovalService : IApprovalService
    {
        public List<Approval> CreateApprovalSteps(Requisition requisition)
        {
            var approvalSteps = new List<Approval>
        {
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HOD Approval",
                PayrollNo = requisition.PayrollNo,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            },
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HR Officer Approval",
                PayrollNo = requisition.PayrollNo,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            },
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HOH Approval",
                PayrollNo = requisition.PayrollNo,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            }
        };

            return approvalSteps;
        }
    }
}
