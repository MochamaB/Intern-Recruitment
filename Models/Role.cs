using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Workflows.Models;

[Table("Role")]
public partial class Role
{
    [StringLength(20)]
    [Unicode(false)]
    public string? RoleName { get; set; }

    [Key]
    [Column("RoleID")]
    public int RoleId { get; set; }
}
