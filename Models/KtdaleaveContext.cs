using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Workflows.Models;

public partial class KtdaleaveContext : DbContext
{
    public KtdaleaveContext()
    {
    }

    public KtdaleaveContext(DbContextOptions<KtdaleaveContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<EmployeeBkp> EmployeeBkps { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning    To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=KTDALeave;Persist Security Info=True;User ID=sa;Password=P@ssw0rd;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentHd).HasDefaultValue("");
        });

        modelBuilder.Entity<EmployeeBkp>(entity =>
        {
            entity.HasKey(e => new { e.PayrollNo, e.RollNo }).HasName("PK_Employee_bkp_1");

            entity.Property(e => e.RollNo).IsFixedLength();
            entity.Property(e => e.EmpisCurrActive).HasDefaultValue(0);
            entity.Property(e => e.Fullname).HasComputedColumnSql("(isnull([Surname],'')+isnull([othernames],''))", false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
