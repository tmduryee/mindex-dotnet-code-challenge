using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        /// <summary>
        /// Retreives the full reporting structure for an employee and the total count of direct and indirect reporting employees
        /// </summary>
        /// <param name="id">The employee ID to retrieve the reporting structure for</param>
        /// <returns>Status response and ReportingStructure if successful</returns>
		[HttpGet("{id}/reportingStructure")]
        public IActionResult GetReportingStructureForEmployee(String id)
		{
            _logger.LogDebug($"Recieved reporting structure get request for employee '{id}'");

			// Ensure the employee exists first!
			var existingEmployee = _employeeService.GetById(id);
			if (existingEmployee == null)
				return NotFound();

            // Retrieve the reporting structure for the employee
			var reportingStructure = _employeeService.GetReportingStructureByEmployeeId(id);

            return Ok(reportingStructure);
		}

        /// <summary>
        /// Adds a compensation record for an employee indicating their salary and its effective date
        /// </summary>
        /// <param name="id">The employee ID to add a salary to</param>
        /// <param name="compensation">The compensation object, which should contain 2 properties: salary (double) and effectiveDate (datetime)</param>
        /// <returns>Status response and Compensation object (with populated Employee property) if successful</returns>
		[HttpPost("{id}/compensation")]
        public IActionResult AddCompensation(string id, [FromBody]Compensation compensation)
		{
            _logger.LogDebug($"Recieved compensation create request for employee '{id}'");

            // Ensure the employee exists first!
            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return UnprocessableEntity(); // If ID is invalid, throw 422

            // Add the employee object to the compensation object
            compensation.Employee = existingEmployee;

            // Create the compensation record
            _employeeService.CreateCompensation(compensation);

            // Return the compensation
            return CreatedAtRoute("getCompensationByEmployeeId", new { id = compensation.Employee.EmployeeId }, compensation);
        }

        /// <summary>
        /// Gets the latest, non-future compensation for an employee. If employee or compensation don't exist, a NotFound error will be thrown.
        /// </summary>
        /// <param name="id">The employee ID to search by</param>
        /// <returns>Status response and Compensation object</returns>
        [HttpGet("{id}/compensation", Name = "getCompensationByEmployeeId")]
        public IActionResult GetCompensation(string id)
        {
            _logger.LogDebug($"Recieved compensation create request for employee '{id}'");

            // Check if employee exists
            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            // Check if valid compensation record exists
            var compensation = _employeeService.GetCompensationForEmployee(id);
            if (compensation == null)
                return NotFound();

            // Return lastest, non-future compensation object
            return Ok(compensation);
        }
    }
}
