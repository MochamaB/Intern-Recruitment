using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Workflows.ViewModels
{
    public class EmployeeViewModel
    {
        public string? DepartmentName { get; set; }
        public string Fullname { get; set; } = null!;
        public string? Username { get; set; }
        public string? Designation { get; set; }
        public string? EmailAddress { get; set; }
        public string PayrollNo { get; set; } = null!;
        public string? Station { get; set; }
        public string? Hod { get; set; }
        public string? Supervisor { get; set; }
        public string? RoleName { get; set; }
        public string? Scale { get; set; }
        public string? EmployeId { get; set; }
        public DateTime? ContractEnd { get; set; }
        public DateTime? HireDate { get; set; }
      
        public DateTime? LastPay { get; set; }

        public int? EmpisCurrActive { get; set; }


       
    }
}
