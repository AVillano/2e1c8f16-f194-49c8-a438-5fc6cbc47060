// since it's not mapped to a db table, I think this should be separate from Models
// I'm not sure if "Views" would be the ideal term for this though
using CodeChallenge.Models;

namespace CodeChallenge.Views
{
    public class ReportingStructure
    {
        public Employee Employee { get; set; }

        public int NumberOfReports { get; set; }
    }
}
