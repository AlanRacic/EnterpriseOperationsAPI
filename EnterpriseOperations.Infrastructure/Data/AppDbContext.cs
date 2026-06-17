using EnterpriseOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<OperationTask> OperationTasks => Set<OperationTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
