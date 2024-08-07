using Microsoft.AspNetCore.Mvc.Rendering;
using Workflows.Models;

namespace Workflows.ViewModels
{

    public class RequisitionFilterViewModel
    {
        public int? DepartmentCode { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<SelectListItem>? StatusList { get; set; }
        public List<SelectListItem>? DepartmentList { get; set; }
    }
    public class RequisitionIndexViewModel
    {
        public RequisitionFilterViewModel Filter { get; set; }
        public List<Requisition> Requisitions { get; set; }
    }

    public class InternFilterViewModel
    {
        public int? DepartmentCode { get; set; }
        public string? Status { get; set; }
        public string? School { get; set; }

        public List<SelectListItem>? StatusList { get; set; }
        public List<SelectListItem>? DepartmentList { get; set; }
        public List<SelectListItem>? SchoolList { get; set; }
    }
    public class InternIndexViewModel
    {
        public InternFilterViewModel Filter { get; set; }
        public List<Intern> Interns { get; set; }
    }

    public class ApprovalFilterViewModel
    {
        public int? DepartmentCode { get; set; }
        public string? Status { get; set; }
        public string? ApprovalStep { get; set; }

        public List<SelectListItem>? StatusList { get; set; }
        public List<SelectListItem>? DepartmentList { get; set; }
        public List<SelectListItem>? ApprovalStepList { get; set; }
    }
    public class ApprovalIndexViewModel
    {
        public ApprovalFilterViewModel Filter { get; set; }
        public List<Approval> Approvals { get; set; }
    }

    public class DocumentFilterViewModel
    {
        public int? DepartmentCode { get; set; }
        public int? DocumentTypeId { get; set; }
     
        public List<SelectListItem>? DepartmentList { get; set; }
        public List<SelectListItem>? TypeList { get; set; }
    }
    public class DocumentIndexViewModel
    {
        public DocumentFilterViewModel Filter { get; set; }
        public List<DocumentViewModel> Documents { get; set; }
    }

    public class EmployeeFilterViewModel
    {
        public string? Department { get; set; }
        public string? Station { get; set; }
        public string? Role { get; set; }
        public string? Scale { get; set; }

        public List<SelectListItem>? DepartmentList { get; set; }
        public List<SelectListItem>? StationList { get; set; }
        public List<SelectListItem>? RoleList { get; set; }
        public List<SelectListItem>? ScaleList { get; set; }
    }
    public class EmployeeIndexViewModel
    {
        public EmployeeFilterViewModel Filter { get; set; }
        public List<EmployeeViewModel> Employees { get; set; }
    }
}
