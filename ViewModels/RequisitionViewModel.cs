using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Workflows.ViewModels
{
    public class RequisitionViewModel
    {
        public int InternId { get; set; }
        public int DepartmentCode { get; set; }
        public SelectList DepartmentItems { get; set; }

    }

    public class CheckDepartmentViewModel
    {
        [Required(ErrorMessage = "Department code is required.")]
        public int DepartmentCode { get; set; }
    }
}
