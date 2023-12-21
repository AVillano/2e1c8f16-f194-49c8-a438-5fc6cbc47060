using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Models
{
    public class Employee
    {
        public String EmployeeId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Position { get; set; }
        public String Department { get; set; }
        public String ManagersEmployeeId { get; set; }
        // DirectReports only stays as a property so that I can set up the data without changing the data
        // with my approach this field would have never existed in the first place
        public List<Employee> DirectReports { get; set; }
    }
}
