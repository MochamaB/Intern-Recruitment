using Microsoft.EntityFrameworkCore;
using Workflows.Data;
using Workflows.Models;

namespace Workflows.Services
{
    public interface IApprovalService
    {
        List<Approval> CreateApprovalSteps(Requisition requisition, string HOD, string HROfficerPayrollNO, string HOH);
        Task HandleApprovalFlow(Approval approval);
    }
    public class ApprovalService : IApprovalService
    {
        private readonly WorkflowsContext _context;
        private readonly IEmailService _emailService;

        public ApprovalService(WorkflowsContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // The functions to create the approvals
        public List<Approval> CreateApprovalSteps(Requisition requisition, string HOD, string HROfficerPayrollNO, string HOH)
        {
            var approvalSteps = new List<Approval>
        {
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                StepNumber = 1,
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
                StepNumber = 2,
                ApprovalStep = "HR Officer Approval",
                PayrollNo = HROfficerPayrollNO,
                ApprovalStatus = "Not Started",
                CreatedAt = DateTime.Now
            },
            new Approval
            {
                Requisition_id = requisition.Id,
                DepartmentCode = requisition.DepartmentCode,
                StepNumber = 3,
                ApprovalStep = "HOH Approval",
                PayrollNo = HOH,
                ApprovalStatus = "Not Started",
                CreatedAt = DateTime.Now
            }
        };

            return approvalSteps;
        }

        // Functions to process approvals

        public async Task HandleApprovalFlow(Approval currentApproval)
        {
            using var db = new KtdaleaveContext();
            var currentRequisition = await _context.Requisition
                .FirstOrDefaultAsync(r => r.Id == currentApproval.Requisition_id);

            if (currentRequisition != null)
            {

                var employee = await db.EmployeeBkps
                  .Where(d => d.PayrollNo == currentRequisition.PayrollNo)
                  .Select(e => new { e.EmailAddress, e.Fullname })
                  .FirstOrDefaultAsync();

                if (currentApproval.ApprovalStatus == "Approved")
                {
                    await HandleApproved(currentApproval, currentRequisition, employee,db);
                }
                else if (currentApproval.ApprovalStatus == "Rejected")
                {
                    await HandleRejected(currentApproval, currentRequisition, employee);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleApproved(Approval currentApproval, Requisition currentRequisition, dynamic employee, KtdaleaveContext db)
        {
            var currentApprover = await db.EmployeeBkps
                .Where(d => d.PayrollNo == currentApproval.PayrollNo)
                .Select(e => e.Fullname)
                .FirstOrDefaultAsync();

            if (currentApprover != null)
            {
                string currentStep = $"{currentApproval.StepNumber}. {currentApproval.ApprovalStep}";
                await _emailService.SendApprovalMadeNotificationAsync(employee.EmailAddress, currentApprover, currentStep, currentRequisition.Id);
            }

            var nextApproval = await _context.Approval
                .FirstOrDefaultAsync(a => a.Requisition_id == currentApproval.Requisition_id && a.StepNumber == currentApproval.StepNumber + 1);

            if (nextApproval != null)
            {
                await HandleNextApproval(nextApproval, db);
            }
            else
            {
                await FinalizeApproval(currentRequisition);
            }
        }

        private async Task HandleNextApproval(Approval nextApproval, KtdaleaveContext db)
        {
            nextApproval.ApprovalStatus = "Pending";
            nextApproval.UpdatedAt = DateTime.Now;
            _context.Update(nextApproval);

            var approverEmail = await db.EmployeeBkps
                .Where(d => d.PayrollNo == nextApproval.PayrollNo)
                .Select(e => e.EmailAddress)
                .FirstOrDefaultAsync();

            if (approverEmail != null)
            {
                await _emailService.SendApprovalPendingNotificationAsync(approverEmail, nextApproval.Requisition_id);
            }
        }

        private async Task FinalizeApproval(Requisition requisition)
        {
            requisition.Status = "Active";
            requisition.UpdatedAt = DateTime.Now;
            _context.Update(requisition);

            var intern = await _context.Intern.FindAsync(requisition.Intern_id);
            if (intern != null)
            {
                intern.Status = "Active";
                intern.UpdatedAt = DateTime.Now;
                _context.Update(intern);
            }
        }

        private async Task HandleRejected(Approval currentApproval, Requisition currentRequisition, dynamic employee)
        {
            await _emailService.SendApprovalRejectedNotificationAsync(employee.EmailAddress, currentRequisition.Id);

            var subsequentApprovals = await _context.Approval
                .Where(a => a.Requisition_id == currentApproval.Requisition_id && a.StepNumber > currentApproval.StepNumber)
                .ToListAsync();

            foreach (var approval in subsequentApprovals)
            {
                approval.ApprovalStatus = "Cancelled";
                approval.UpdatedAt = DateTime.Now;
                _context.Update(approval);
            }
        }
    }
}
