using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Workflows.Models;

[PrimaryKey("PayrollNo", "RollNo")]
[Table("Employee_bkp")]
public partial class EmployeeBkp
{
    [StringLength(50)]
    [Unicode(false)]
    public string? Designation { get; set; }

    [Column("Email_address")]
    [StringLength(100)]
    [Unicode(false)]
    public string? EmailAddress { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? SurName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? OtherNames { get; set; }

    [Key]
    [StringLength(8)]
    [Unicode(false)]
    public string PayrollNo { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? Station { get; set; }

    [Column("password_p")]
    [StringLength(50)]
    [Unicode(false)]
    public string? PasswordP { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Hod { get; set; }

    [Column("supervisor")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Supervisor { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Role { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string? OtherName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Department { get; set; }

    [Column("EmployeID")]
    [StringLength(50)]
    [Unicode(false)]
    public string? EmployeId { get; set; }

    [Column("contractEnd", TypeName = "datetime")]
    public DateTime? ContractEnd { get; set; }

    [Column("hire_date", TypeName = "datetime")]
    public DateTime? HireDate { get; set; }

    [Column("Service_years")]
    public int? ServiceYears { get; set; }

    [Column("username")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Username { get; set; }

    [Column("Retire_date", TypeName = "datetime")]
    public DateTime? RetireDate { get; set; }

    [Column("pass")]
    [MaxLength(50)]
    public byte[]? Pass { get; set; }

    [Key]
    [StringLength(5)]
    [Unicode(false)]
    public string RollNo { get; set; } = null!;

    [Column("Last_Pay", TypeName = "datetime")]
    public DateTime? LastPay { get; set; }

    [Column("scale")]
    [StringLength(50)]
    public string? Scale { get; set; }

    public int? EmpisCurrActive { get; set; }

    [Column("org_group")]
    public int? OrgGroup { get; set; }

    [Column("fullname")]
    [StringLength(100)]
    [Unicode(false)]
    public string Fullname { get; set; } = null!;

    
}
