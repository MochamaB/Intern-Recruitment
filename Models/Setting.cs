using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Workflows.Models
{
    public class Setting
    {
        public int Id { get; set; }

        public string? Category { get; set; }

        [Column("departmentCode")]
        [Display(Name = "Department")]
        public int? DepartmentCode { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
      
        public string? Description { get; set; }
    }
}
