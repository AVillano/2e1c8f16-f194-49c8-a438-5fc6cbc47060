using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;
using CodeChallenge.Views;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure GenerateReportingStructure(Employee employee)
        {
            if (employee == null)
            {
                return null;
            }

            var reportingStructure = new ReportingStructure()
            {
                Employee = employee,
                NumberOfReports = 0
            };
            // there may be an optimization here with the HashSet
            // each string will be an EmployeeId, which will be 36 bytes in string form with the hyphens
            // making the HashSet be of type employee MAY be better if the following is true
            // 1. if what is being added is simply a pointer to that object in memory, which would be 8 bytes on a 64 bit machine
            // 2. the equality checker checks equality based on that address and does not individually check every property of the Employee
            // but since I am not certain I'll just leave it as a set of EmployeeIds
            reportingStructure.NumberOfReports = CountNumberOfReports(employee, new HashSet<string>());

            return reportingStructure;
        }

        private int CountNumberOfReports(Employee employee, HashSet<string> visited)
        {
            // having the visited set removes potential infinite cycles
            // I don't think this should be handled here and should be checked when you set direct reports for an Employee
            // but adding/removing/replacing direct reports is outside of scope so I'll just put the check here for the time being
            if (visited.Contains(employee.EmployeeId)
                || employee == null)
            {
                // need to subtract 1 because the employee has already been counted as a direct report from the parent
                return -1;
            }

            _logger.LogDebug("EmployeeService.CountNumberOfReports: Counting reports for employee id - " + employee.EmployeeId);

            visited.Add(employee.EmployeeId);
            var directReports = _employeeRepository.GetDirectReportsByManagerId(employee.EmployeeId);

            if (directReports.Count() == 0)
            {
                return 0;
            }

            var totalReports = directReports.Count();
            foreach (var emp in directReports)
            {
                totalReports += CountNumberOfReports(emp, visited);
            }

            return totalReports;
        }
    }
}
