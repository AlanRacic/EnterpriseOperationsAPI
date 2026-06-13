using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Domain.Entities;
using EnterpriseOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Repositories
{
    public class OperationTaskRepository : IOperationTaskRepository
    {
        private readonly AppDbContext _context;

        public OperationTaskRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<OperationTask>> GetAllAsync() 
        {
            return await _context.OperationTasks
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OperationTask?> GetByIdAsync(int id) 
        {
            return await _context.OperationTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(task => task.Id == id);
        }

        public async Task<OperationTask> AddAsync(OperationTask operationTask) 
        {
            await _context.OperationTasks.AddAsync(operationTask);
            await _context.SaveChangesAsync();

            return operationTask;
        }
    }
}
