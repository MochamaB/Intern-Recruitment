using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Workflows.Models;

[Table("Department")]
public partial class Department
{
    [Key]
    [Column("departmentCode")]
    public int DepartmentCode { get; set; }

    [Column("DepartmentID")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DepartmentId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string DepartmentName { get; set; } = null!;

    [Column("DepartmentHD")]
    [StringLength(50)]
    public string? DepartmentHd { get; set; }

    [StringLength(50)]
    public string? Emailaddress { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? UserName { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? OrgCode { get; set; }

    [NotMapped]
    public EmployeeBkp? Employee { get; set; }

    // Navigation property for the Interns associated with this Department
    //  public ICollection<Intern> Interns { get; set; }
}
