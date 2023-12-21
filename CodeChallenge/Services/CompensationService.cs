using CodeChallenge.Models;
using CodeChallenge.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CodeChallenge.Services
{
    public class CompensationService : ICompensationService
    {
        private readonly ICompensationRepository _compensationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<CompensationService> _logger;

        public CompensationService(ILogger<CompensationService> logger,
                                   ICompensationRepository compensationRepository,
                                   IEmployeeRepository employeeRepository)
        {
            _logger = logger;
            _compensationRepository = compensationRepository;
            _employeeRepository = employeeRepository;
        }

        public IEnumerable<Compensation> GetByEmployeeId(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                return _compensationRepository.GetByEmployeeId(id);
            }

            return null;
        }

        public Compensation Create(Compensation compensation)
        {
            if (compensation != null)
            {
                if (_employeeRepository.GetById(compensation.EmployeeId) == null)
                    return null;
                _compensationRepository.Add(compensation);
                _compensationRepository.SaveAsync().Wait();
            }

            return compensation;
        }
    }
}