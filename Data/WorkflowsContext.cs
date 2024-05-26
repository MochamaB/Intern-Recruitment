using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workflows.Models;

namespace Workflows.Data
{
    public class WorkflowsContext : DbContext
    {
        public WorkflowsContext (DbContextOptions<WorkflowsContext> options)
            : base(options)
        {
        }

        public DbSet<Workflows.Models.Intern> Intern { get; set; } = default!;
        public DbSet<Workflows.Models.Requisition> Requisition { get; set; } = default!;
        public DbSet<Workflows.Models.Approval> Approval { get; set; } = default!;
    }
}
