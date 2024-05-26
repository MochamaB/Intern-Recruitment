using Workflows.Models;

namespace Workflows.Services
{
    public interface IApprovalService
    {
        List<Approval> CreateApprovalSteps(Requisition requisition, string HOD, string HROfficerPayrollNO, string HOH);
    }
    public class ApprovalService : IApprovalService
    {
        public List<Approval> CreateApprovalSteps(Requisition requisition, string HOD, string HROfficerPayrollNO, string HOH)
        {
            var approvalSteps = new List<Approval>
        {
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HOD Approval",
                PayrollNo = HOD,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HR Officer Approval",
                PayrollNo = HROfficerPayrollNO,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            },
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                ApprovalStep = "HOH Approval",
                PayrollNo = HOH,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            }
        };

            return approvalSteps;
        }
    }
}
