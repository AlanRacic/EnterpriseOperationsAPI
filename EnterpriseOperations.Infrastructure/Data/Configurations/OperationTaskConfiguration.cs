using EnterpriseOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Data.Configurations
{
    public class OperationTaskConfiguration : IEntityTypeConfiguration<OperationTask>
    {
        public void Configure(EntityTypeBuilder<OperationTask> builder)
        {
            builder.HasIndex(task => task.IsCompleted);

            builder.HasIndex(task => task.CreatedAt);
        }
    }
}
