using CodeChallenge.Data;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CodeChallenge.Repositories
{
    public class CompensationRepository : ICompensationRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<ICompensationRepository> _logger;

        public CompensationRepository(ILogger<ICompensationRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Compensation GetById(string id)
        {
            return _employeeContext.Compensations.SingleOrDefault(c => c.CompensationId == id);
        }

        public IEnumerable<Compensation> GetByEmployeeId(string id)
        {
            return _employeeContext.Compensations.Where(c => c.EmployeeId == id);
        }

        // repositories would ideally have a parent repository class that would take care of methods such as these with generics
        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Compensation Add(Compensation compensation)
        {
            compensation.CompensationId = Guid.NewGuid().ToString();
            return _employeeContext.Compensations.Add(compensation).Entity;
        }

        public Compensation Remove(Compensation compensation)
        {
            return _employeeContext.Compensations.Remove(compensation).Entity;
        }
    }
}