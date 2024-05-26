using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workflows.ViewModels
{
    public class RequisitionViewModel
    {
        public int InternId { get; set; }
        public int DepartmentCode { get; set; }
        public SelectList DepartmentItems { get; set; }

    }
}
