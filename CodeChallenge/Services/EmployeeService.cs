using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;

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

        /// <summary>
        /// Retrieves full reporting structure for an employee
        /// </summary>
        /// <param name="id">The employee ID to retrieve the reporting structure for</param>
        /// <returns>The ReportingStructure object</returns>
        public ReportingStructure GetReportingStructureByEmployeeId(string id)
		{
            // If employee ID is not null, search for and return ReportingStructure for employee
            if (!String.IsNullOrEmpty(id))
			{
                return _employeeRepository.GetReportingStructureByEmployeeId(id);
            }

            return null;
		}

        /// <summary>
        /// Creates a compensation record for an employee
        /// </summary>
        /// <param name="compensation">The Compensation object, which contains Employee, Salary, and EffectiveDate properties</param>
        /// <returns>The created Compensation object</returns>
        public Compensation CreateCompensation(Compensation compensation)
		{
            // If compensation object is not null, create a record and save the DB
            if (compensation != null)
			{
                _employeeRepository.CreateCompensation(compensation);
                _employeeRepository.SaveAsync().Wait();
			}

            // Return the compensation object
            return compensation;
        }

        /// <summary>
        /// Gets a valid (most recent, non-future) compensation record for an employee
        /// </summary>
        /// <param name="id">The employee ID to search by</param>
        /// <returns>The Compensation object</returns>
        public Compensation GetCompensationForEmployee(string id)
		{
            // If employee ID is not null, search for and return valid compensation record
            if (!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetCompensationForEmployee(id);
            }

            return null;
        }
    }
}
