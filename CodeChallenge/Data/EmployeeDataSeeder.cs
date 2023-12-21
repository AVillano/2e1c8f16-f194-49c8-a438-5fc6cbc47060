using CodeChallenge.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Data
{
    public class EmployeeDataSeeder
    {
        private EmployeeContext _employeeContext;
        private const String EMPLOYEE_SEED_DATA_FILE = "resources/EmployeeSeedData.json";

        public EmployeeDataSeeder(EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
        }

        public async Task Seed()
        {
            if(!_employeeContext.Employees.Any())
            {
                List<Employee> employees = LoadEmployees();
                _employeeContext.Employees.AddRange(employees);

                await _employeeContext.SaveChangesAsync();
            }
        }

        private List<Employee> LoadEmployees()
        {
            using (FileStream fs = new FileStream(EMPLOYEE_SEED_DATA_FILE, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();

                List<Employee> employees = serializer.Deserialize<List<Employee>>(jr);
                //FixUpReferences(employees);
                LinkEmployeesToManager(employees);

                return employees;
            }
        }

        private void LinkEmployeesToManager(List<Employee> employees)
        {
            // this may be a bit nasty but I didn't want to mess with the input data
            // use the map to tell us which employee maps to which manager
            var employeeToManagerMap = new Dictionary<string, string>();
            foreach (var employee in employees)
            {
                if (employee.DirectReports != null)
                {
                    foreach (var directReport in employee.DirectReports)
                    {
                        employeeToManagerMap.Add(directReport.EmployeeId, employee.EmployeeId);
                    }
                }
            }
            // iterate again to actually set the connection
            foreach (var employee in employees)
            {
                employee.ManagersEmployeeId = employeeToManagerMap.ContainsKey(employee.EmployeeId) ? employeeToManagerMap[employee.EmployeeId] : null;
                employee.DirectReports = null; // ideally I would not have this at all but as stated, I want to keep input data unchanged
            }

            /* Here is a bit of an "essay" as to why I'm doing things this way:
             * Once I realized that I was being given a bit of free reign, I thought about how I would actually want to do this.
             * 
             * Having an Employee have a list of Employee under DirectReports was not the ideal, nor really even a good, design to me.
             * Firstly, let's say we went the converter route. What would that look like? Well we could save a list of EmployeeIds to the table.
             * Then, we could have the read converter just pull in each Employee from that list of ids. This should work, but it's quite bad.
             * If we did this, we are adding potentially a ton of db reads that are likely unnecessary without knowing how many DirectReports there could even be.
             * There could be other complaints with this method, but the currently unlimited DirectReports list was my biggest concern.
             * 
             * With that out of consideration, there are two methods that I would see as viable and reasonable.
             * What we have here is a one-to-many relationship, so we could create a reference(sorry if this is the wrong terminology) table to track that.
             * This table would at its most basic have one column for the manager employee and one for a direct report.
             * So for the example of John Lennon having Paul McCartney and Ringo Starr as direct reports, this new table would create two rows.
             * If we want to know how many direct reports someone has, we search for rows with that EmployeeId as the ManagerId.
             * If we want to know who the manager of someone is, we search for the row with that EmployeeId as the DirectReportId.
             * I did not go with this because while it may be better larger scale, I think it's overengineering a solution for this specific case.
             * 
             * Instead, I went with the following approach.
             * In the one-to-many relationship, we could instead have the many keep track of who their one is.
             * This means that any given employee would hold the id of their manager.
             * A big concern when doing something like this is whether or not how many of those columns would just be null.
             * If we take a standard company, I think the only employees that would not have a manager are c-levels and/or the owner.
             * Having only a few rows with that column being null out of a potentially massive company is something I am more than happy with.
             * Now if we want to hold more data about this relationship or maybe expand to use it in different ways, I think the previous solution is better.
             * Since we are not doing that and this is just a test, I think not doing that is more than fine.
             * There are too many variables to be able to definitively say which solution is best overall, in my opinion, and I think it does vary.
             */
        }

        //private void FixUpReferences(List<Employee> employees)
        //{
        //    var employeeIdRefMap = from employee in employees
        //                        select new { Id = employee.EmployeeId, EmployeeRef = employee };

        //    employees.ForEach(employee =>
        //    {

        //        if (employee.DirectReports != null)
        //        {
        //            var referencedEmployees = new List<Employee>(employee.DirectReports.Count);
        //            employee.DirectReports.ForEach(report =>
        //            {
        //                var referencedEmployee = employeeIdRefMap.First(e => e.Id == report.EmployeeId).EmployeeRef;
        //                referencedEmployees.Add(referencedEmployee);
        //            });
        //            employee.DirectReports = referencedEmployees;
        //        }
        //    });
        //}
    }
}
