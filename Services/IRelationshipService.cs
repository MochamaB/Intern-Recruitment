﻿using Microsoft.EntityFrameworkCore;
using Workflows.Data;
using Workflows.Models;

namespace Workflows.Services
{
    public interface IRelationshipService
    {
        Task<Requisition> GetRequisitionWithRelatedDataAsync(int requisitionId);
        Task<Intern> GetInternWithRelatedDataAsync(int internId);
        Task<IEnumerable<Approval>> GetApprovalsForRequisitionAsync(int requisitionId);
        Task<Approval> GetApprovalWithRelatedDataAsync(int approvalId);
        Task<Document> GetDocumentWithRelatedDataAsync(int documentId);
        Task<Setting> GetSettingWithRelatedDataAsync(int settingID);
        Task<Department> GetDepartmentWithRelatedDataAsync(int settingID);
    }

    public class RelationshipService : IRelationshipService
    {

        private readonly WorkflowsContext _context;

        public RelationshipService(WorkflowsContext context)
        {
            _context = context;
        }

        public async Task<Requisition> GetRequisitionWithRelatedDataAsync(int requisitionId)
        {
            using (var db = new KtdaleaveContext())
            {
               
                var requisition = await _context.Requisition.FindAsync(requisitionId);
                if (requisition == null) return null;

                requisition.Department = db.Departments.FirstOrDefault(d => d.DepartmentCode == requisition.DepartmentCode);
                requisition.Intern = await _context.Intern.FindAsync(requisition.Intern_id);
                requisition.Employee = db.EmployeeBkps.FirstOrDefault(e => e.PayrollNo == requisition.PayrollNo);
                requisition.Approvals = await _context.Approval
                    .Where(a => a.Requisition_id == requisitionId)
                    .ToListAsync();

                return requisition;
            }
        }

        public async Task<Intern> GetInternWithRelatedDataAsync(int internId)
        {
            var intern = await _context.Intern.FindAsync(internId);
            if (intern == null) return null;

            intern.Requisition = await _context.Requisition
                .Where(r => r.Intern_id == internId)
                .ToListAsync();

            return intern;
        }

        public async Task<IEnumerable<Approval>> GetApprovalsForRequisitionAsync(int requisitionId)
        {
            return await _context.Approval
                .Where(a => a.Requisition_id == requisitionId)
                .ToListAsync();
        }

        public async Task<Approval> GetApprovalWithRelatedDataAsync(int approvalId)
        {
            using (var db = new KtdaleaveContext())
            {
                var approval = await _context.Approval.FindAsync(approvalId);
                if (approval == null) return null;

                approval.Department = db.Departments.FirstOrDefault(d => d.DepartmentCode == approval.DepartmentCode);
                approval.Employee = db.EmployeeBkps.FirstOrDefault(e => e.PayrollNo == approval.PayrollNo);

                return approval;
            }
        }

        public async Task<Document> GetDocumentWithRelatedDataAsync(int documentId)
        {
            using (var db = new KtdaleaveContext())
            {
                var document = await _context.Document
                    .Include(d => d.DocumentType)
                   .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null) return null;

                document.Intern = await _context.Intern.FindAsync(document.Intern_id);
                document.Department = db.Departments.FirstOrDefault(d => d.DepartmentCode == document.DepartmentCode);


                return document;
            }
        }

        public async Task<Setting> GetSettingWithRelatedDataAsync(int settingID)
        {
            using (var db = new KtdaleaveContext())
            {
                var setting = await _context.Setting.FindAsync(settingID);
                if (setting == null) return null;

                setting.Department = db.Departments.FirstOrDefault(d => d.DepartmentCode == setting.DepartmentCode);
              

                return setting;
            }
        }

        public async Task<Department> GetDepartmentWithRelatedDataAsync(int departmentCode)
        {
            using (var db = new KtdaleaveContext())
            {
                var department = await db.Departments.FindAsync(departmentCode);
                if (department == null) return null;

                department.Employee = db.EmployeeBkps.FirstOrDefault(e => e.PayrollNo == department.DepartmentHd);


                return department;
            }
        }
    }
}
