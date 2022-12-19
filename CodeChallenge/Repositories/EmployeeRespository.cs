using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CodeChallenge.Data;

namespace CodeChallenge.Repositories
{
    public class EmployeeRespository : IEmployeeRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<IEmployeeRepository> _logger;

        public EmployeeRespository(ILogger<IEmployeeRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        public Employee GetById(string id)
        {
            return _employeeContext.Employees.SingleOrDefault(e => e.EmployeeId == id);
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Employee Remove(Employee employee)
        {
            return _employeeContext.Remove(employee).Entity;
        }

        /// <summary>
        /// Retrieves full reporting structure for an employee
        /// </summary>
        /// <param name="id">The employee ID to retrieve the reporting structure for</param>
        /// <returns>The ReportingStructure object</returns>
        public ReportingStructure GetReportingStructureByEmployeeId(string id)
		{
            // Build the ReportingStructure and return it
            return new ReportingStructure
            {
                Employee = GetById(id), // Get the employee object based on the employee ID
                NumberOfReports = GetNumberOfReports(id), // Get the total sum of employees that report, directly and indirectly, to this employee
            };
		}

        /// <summary>
        /// Gets sum of direct reports for employee and recursively searches for indirect reports to add to the sum
        /// </summary>
        /// <param name="id">The employee ID to retrieve direct (and indirect) reports for</param>
        /// <returns>The sum of all reporting employees, direct and indirect</returns>
        private int GetNumberOfReports(string id)
		{
            // Get the direct reports for the employee
            var directReports = _employeeContext.Employees.Where(e => e.EmployeeId == id).Select(e => e.DirectReports).FirstOrDefault();

            // If there are no direct reports, simply return 0
            if (directReports == null)
			{
                return 0;
			}

            // Get the count of direct employees and add to the count by searching for indirect employees
            int count = directReports.Count;
            foreach (Employee employee in directReports)
            {
                count += GetNumberOfReports(employee.EmployeeId);
            }

            // Return the total count of reporting employees
            return count;
		}

        /// <summary>
        /// Creates a compensation record for an employee
        /// </summary>
        /// <param name="compensation">The Compensation object, which contains Employee, Salary, and EffectiveDate properties</param>
        /// <returns>The created Compensation object</returns>
        public Compensation CreateCompensation(Compensation compensation)
		{
            // Add a compensation record to the in-memory DB and return the Compensation object
            _employeeContext.Compensations.Add(compensation);
            return compensation;
        }

        /// <summary>
        /// Gets a valid (most recent, non-future) compensation record for an employee
        /// </summary>
        /// <param name="id">The employee ID to search by</param>
        /// <returns>The Compensation object</returns>
        public Compensation GetCompensationForEmployee(string id)
        {
            // Return the latest compensation record for the employee that is not in the future
            return _employeeContext.Compensations
                .OrderByDescending(c => c.EffectiveDate)
                .Where(c => c.Employee.EmployeeId == id && c.EffectiveDate <= DateTime.Now)
                .FirstOrDefault();
        }
    }
}
