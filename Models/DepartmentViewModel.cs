using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workflows.Models
{
    public class DepartmentViewModel
    {
        public int SelectedDepartmentId { get; set; }
        public SelectList DepartmentItems { get; set; }
    }
}
