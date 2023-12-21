using CodeChallenge.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public interface ICompensationRepository
    {
        Task SaveAsync();
        Compensation GetById(string id);
        Compensation Add(Compensation compensation);
        Compensation Remove(Compensation compensation);
        IEnumerable<Compensation> GetByEmployeeId(string id);
    }
}