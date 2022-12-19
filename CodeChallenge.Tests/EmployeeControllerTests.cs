
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        #region Employee Tests
        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion Employee Tests

        #region DirectReports Tests
        /// <summary>
        /// This will test getting direct reports for an employee who has direct and indirect reports
        /// </summary>
        [TestMethod]
        public void GetDirectReports_Returns_OK()
        {
            // Arrange
            // Define the employee and all direct (and indirect) reports that will be returned
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                Department = "Engineering",
                FirstName = "John",
                LastName = "Lennon",
                Position = "Development Manager",
                DirectReports = new List<Employee>()
				{
                    new Employee()
					{
                        EmployeeId = "b7839309-3348-463b-a7e3-5de1c168beb3",
                        Department = "Engineering",
                        FirstName = "Paul",
                        LastName = "McCartney",
                        Position = "Developer I",
                        DirectReports = null,
                    },
                    new Employee()
                    {
                        EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                        Department = "Engineering",
                        FirstName = "Ringo",
                        LastName = "Starr",
                        Position = "Developer V",
                        DirectReports = new List<Employee>()
                        {
                            new Employee()
                            {
                                EmployeeId = "62c1084e-6e34-4630-93fd-9153afb65309",
                                Department = "Engineering",
                                FirstName = "Pete",
                                LastName = "Best",
                                Position = "Developer II",
                                DirectReports = null,
                            },
                            new Employee()
                            {
                                EmployeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c",
                                Department = "Engineering",
                                FirstName = "George",
                                LastName = "Harrison",
                                Position = "Developer III",
                                DirectReports = null,
                            },
                        },
                    },
                },
            };
            var directReports = new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = 4,
            };

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/{employee.EmployeeId}/reportingStructure");
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Verify everything matches, not pretty, but thorough
            var newDirectReports = response.DeserializeContent<ReportingStructure>();
            Assert.IsNotNull(newDirectReports.Employee);
            Assert.IsNotNull(newDirectReports.Employee.EmployeeId);

            // Assert that employee matches
            Assert.AreEqual(directReports.Employee.FirstName, newDirectReports.Employee.FirstName);
            Assert.AreEqual(directReports.Employee.LastName, newDirectReports.Employee.LastName);
            Assert.AreEqual(directReports.Employee.Department, newDirectReports.Employee.Department);
            Assert.AreEqual(directReports.Employee.Position, newDirectReports.Employee.Position);

            // Assert that employee direct reports match
			Assert.AreEqual(directReports.Employee.DirectReports[0].FirstName, newDirectReports.Employee.DirectReports[0].FirstName);
			Assert.AreEqual(directReports.Employee.DirectReports[0].LastName, newDirectReports.Employee.DirectReports[0].LastName);
			Assert.AreEqual(directReports.Employee.DirectReports[0].Department, newDirectReports.Employee.DirectReports[0].Department);
			Assert.AreEqual(directReports.Employee.DirectReports[0].Position, newDirectReports.Employee.DirectReports[0].Position);
			Assert.AreEqual(directReports.Employee.DirectReports[1].FirstName, newDirectReports.Employee.DirectReports[1].FirstName);
			Assert.AreEqual(directReports.Employee.DirectReports[1].LastName, newDirectReports.Employee.DirectReports[1].LastName);
			Assert.AreEqual(directReports.Employee.DirectReports[1].Department, newDirectReports.Employee.DirectReports[1].Department);
			Assert.AreEqual(directReports.Employee.DirectReports[1].Position, newDirectReports.Employee.DirectReports[1].Position);

            // Assert that employee indirect reports match
            Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[0].FirstName, newDirectReports.Employee.DirectReports[1].DirectReports[0].FirstName);
			Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[0].LastName, newDirectReports.Employee.DirectReports[1].DirectReports[0].LastName);
			Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[0].Department, newDirectReports.Employee.DirectReports[1].DirectReports[0].Department);
			Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[0].Position, newDirectReports.Employee.DirectReports[1].DirectReports[0].Position);
            Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[1].FirstName, newDirectReports.Employee.DirectReports[1].DirectReports[1].FirstName);
            Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[1].LastName, newDirectReports.Employee.DirectReports[1].DirectReports[1].LastName);
            Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[1].Department, newDirectReports.Employee.DirectReports[1].DirectReports[1].Department);
            Assert.AreEqual(directReports.Employee.DirectReports[1].DirectReports[1].Position, newDirectReports.Employee.DirectReports[1].DirectReports[1].Position);

            // Assert that the number of reports match
            Assert.AreEqual(directReports.NumberOfReports, newDirectReports.NumberOfReports);
        }

        /// <summary>
        /// This will test getting direct reports for an employee who has no direct reports
        /// </summary>
        [TestMethod]
        public void GetDirectReports_Returns_OK_NoDirectReports()
        {
            // Arrange
            // Define an employee ID who doesn't have direct reports
            var employee = new Employee()
            {
                EmployeeId = "b7839309-3348-463b-a7e3-5de1c168beb3",
                Department = "Engineering",
                FirstName = "Paul",
                LastName = "McCartney",
                Position = "Developer I",
                DirectReports = null,
            };
            var directReports = new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = 0,
            };

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/{employee.EmployeeId}/reportingStructure");
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Verify everything matches, not pretty, but thorough
            var newDirectReports = response.DeserializeContent<ReportingStructure>();
            Assert.IsNotNull(newDirectReports.Employee);
            Assert.IsNotNull(newDirectReports.Employee.EmployeeId);

            // Assert that employee matches
            Assert.AreEqual(directReports.Employee.FirstName, newDirectReports.Employee.FirstName);
            Assert.AreEqual(directReports.Employee.LastName, newDirectReports.Employee.LastName);
            Assert.AreEqual(directReports.Employee.Department, newDirectReports.Employee.Department);
            Assert.AreEqual(directReports.Employee.Position, newDirectReports.Employee.Position);
            Assert.AreEqual(directReports.Employee.DirectReports, newDirectReports.Employee.DirectReports);
            Assert.AreEqual(directReports.NumberOfReports, newDirectReports.NumberOfReports);
        }

        /// <summary>
        /// This will test getting direct reports for an employee who doesn't exist (should throw not found)
        /// </summary>
        [TestMethod]
        public void GetDirectReports_Returns_NotFound()
        {
            // Arrange
            // Define an employee ID who doesn't have direct reports
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };

            // Execute
            var postRequestTask = _httpClient.GetAsync($"api/employee/{employee.EmployeeId}/reportingStructure");
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion DirectReports Tests

        #region Compensation Tests
        /// <summary>
        /// This will test the Create Compensation endpoint's ability to successfully create and retrieve the new compensation
        /// </summary>
        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            // Define the employee object expected to be returned after creation, who's EmployeeId is to be passed to the request as a route parameter
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                Department = "Engineering",
                FirstName = "John",
                LastName = "Lennon",
                Position = "Development Manager",
            };

            // Define the compensation object to be passed in the body of the request and expected to be returned after creation
            var compensation = new Compensation()
            {
                Employee = null, // Employee starts out as null, and is set based on the {id} route parameter
                Salary = 1200000,
                EffectiveDate = DateTime.Today.AddDays(-2),
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{employee.EmployeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Add the employee object to compensation to match what is expected to be returned
            compensation.Employee = employee; 

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // Verify Compensation is correctly returned
            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.Employee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newCompensation.Employee.FirstName);
            Assert.AreEqual(employee.LastName, newCompensation.Employee.LastName);
            Assert.AreEqual(employee.Department, newCompensation.Employee.Department);
            Assert.AreEqual(employee.Position, newCompensation.Employee.Position);
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
        }

        /// <summary>
        /// This will test the Create Compensation endpoint's ability to throw UnprocessableEntity error if invalid employee ID is provided
        /// </summary>
        [TestMethod]
        public void CreateCompensation_Returns_UnprocessableEntity()
        {
            // Arrange
            // Define the employee object expected to be returned after creation, who's EmployeeId is to be passed to the request as a route parameter
            // In this case, it is a non-existent employee with an invalid ID
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            // Define the compensation object to be passed in the body of the request
            var compensation = new Compensation()
            {
                Employee = null, // Employee starts out as null, and is set based on the {id} route parameter
                Salary = 1200000,
                EffectiveDate = DateTime.Today,
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{employee.EmployeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert (Verify NotFound error is thrown)
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        /// <summary>
        /// This will test getting the correct (most recent, non-future) compensation for an employee
        /// </summary>
        [TestMethod]
        public void GetCompensation_Returns_OK()
        {
            // Arrange
            // Define the employee object expected to be in the returned Compensation object, who's EmployeeId is to be passed to the request as a route parameter
            var employee = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                Department = "Engineering",
                FirstName = "John",
                LastName = "Lennon",
                Position = "Development Manager",
            };

            // Need to verify that the correct compensation is returned (most recent, non-future)
            // In order to test this, we will add a compensation for yesterday, today, and tomorrow, the GET should retrieve today's compensation (correctCompensation)
            var correctCompensation = new Compensation()
            {
                Employee = null,
                Salary = 1450000,
                EffectiveDate = DateTime.Today,
            };
            var incorrectCompensation1 = new Compensation()
            {
                Employee = null,
                Salary = 1350000,
                EffectiveDate = DateTime.Today.AddDays(-1), // Yesterday
            };
            var incorrectCompensation2 = new Compensation()
            {
                Employee = null,
                Salary = 1550000,
                EffectiveDate = DateTime.Today.AddDays(1), // Tomorrow
            };

            var requestContent1 = new JsonSerialization().ToJson(correctCompensation);
            var requestContent2 = new JsonSerialization().ToJson(incorrectCompensation1);
            var requestContent3 = new JsonSerialization().ToJson(incorrectCompensation2);

            // Add the compensations first so that we may test getting the correct one
            _httpClient.PostAsync($"api/employee/{employee.EmployeeId}/compensation",
               new StringContent(requestContent1, Encoding.UTF8, "application/json"));
            _httpClient.PostAsync($"api/employee/{employee.EmployeeId}/compensation",
               new StringContent(requestContent2, Encoding.UTF8, "application/json"));
            _httpClient.PostAsync($"api/employee/{employee.EmployeeId}/compensation",
               new StringContent(requestContent3, Encoding.UTF8, "application/json"));

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employee.EmployeeId}/compensation");
            var response = getRequestTask.Result;
            
            // Add the employee object to compensation to match what is expected to be returned
            correctCompensation.Employee = employee;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Verify Compensation is correctly returned
            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.Employee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newCompensation.Employee.FirstName);
            Assert.AreEqual(employee.LastName, newCompensation.Employee.LastName);
            Assert.AreEqual(employee.Department, newCompensation.Employee.Department);
            Assert.AreEqual(employee.Position, newCompensation.Employee.Position);
            Assert.AreEqual(correctCompensation.Salary, newCompensation.Salary);
            Assert.AreEqual(correctCompensation.EffectiveDate, newCompensation.EffectiveDate);
        }

        /// <summary>
        /// This will test the Create Compensation endpoint's ability to throw NotFound error if a non-existing/invalid employee ID is provided
        /// </summary>
        [TestMethod]
        public void GetCompensation_Returns_NotFound_ForInvalidEmployee()
        {
            // Arrange
            // Define an invalid employee ID to pass as a router param
            var employeeId = "Invalid_Id";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/compensation");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// This will test the Create Compensation endpoint's ability to throw NotFound error if an existing employee doesn't have a non-future compensation associated with them
        /// </summary>
        [TestMethod]
        public void GetCompensation_Returns_NotFound()
        {
            // Arrange
            // Define the employee id to pass as a router parameter
            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            // Define a future compensation object to be added and to be tested against
            var compensation = new Compensation()
            {
                Employee = null,
                Salary = 1200000,
                EffectiveDate = DateTime.Today.AddDays(2),
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            _httpClient.PostAsync($"api/employee/{employeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/compensation");
            var response = getRequestTask.Result;

            // Assert (Verify NotFound error is thrown)
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion Compensation Tests
    }
}
