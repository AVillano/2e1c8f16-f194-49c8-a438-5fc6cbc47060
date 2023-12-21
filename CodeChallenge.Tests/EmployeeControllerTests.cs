
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;
using CodeChallenge.Views;
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

        #region ReportingStructure Tests
        [TestMethod]
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", 4)] // John Lennon
        // we don't want tests to be affected by other tests but since we don't reset the state between tests, UpdateEmployee_Returns_Ok affects the below test case
        // the direct reports of Ringo Starr will also point to Pete Best if the replace test happened
        // not sure if that is the behavior we would want but at the same time I'm not really a fan of replacing a full employee even being a thing
        [DataRow("03aa1462-ffa9-4978-901b-7c001562cf6f", 2)] // Ringo Starr, who may be Pete Best now depending on the order the tests ran in
        [DataRow("b7839309-3348-463b-a7e3-5de1c168beb3", 0)] // Paul McCartney
        public void GetReportingStructure_Returns_Ok(string employeeId, int expectedNumberOfReports)
        {
            // Arrange

            // Act
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingstructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(employeeId, reportingStructure.Employee.EmployeeId);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_Returns_NotFound()
        {
            // Arrange
            var employeeId = "INVALID_ID";

            // Act
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reportingstructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // like mentioned above, this test also has effects on other tests, so some may fail depending on the order
        // once again we would ideally want to reset the db between each test
        //[TestMethod]
        public void GetReportingStructureOnCircularRelationship_Returns_Ok()
        {
            // Arrange
            var johnLennonsEmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedNumberOfReports = 4;
            // make George Harrison the manager of John Lennon, making a loop
            // get John Lennon
            var georgeHarrisonsEmployeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c";
            var getJohnLennonTask = _httpClient.GetAsync($"api/employee/{johnLennonsEmployeeId}");
            var getJohnLennonResponse = getJohnLennonTask.Result;
            var johnLennon = getJohnLennonResponse.DeserializeContent<Employee>();
            johnLennon.ManagersEmployeeId = georgeHarrisonsEmployeeId;
            // now replace him to update the ManagersEmployeeId
            // I think I mentioned it elsewhere, but again ideally we would have different functionality for this
            // and we would not just be replacing an entire Employee to update it
            var requestContent = new JsonSerialization().ToJson(johnLennon);
            var putJohnLennonTask = _httpClient.PutAsync($"api/employee/{johnLennonsEmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putJohnLennonResponse = putJohnLennonTask.Result;

            // Act
            var getRequestTask = _httpClient.GetAsync($"api/employee/{johnLennonsEmployeeId}/reportingstructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(johnLennonsEmployeeId, reportingStructure.Employee.EmployeeId);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }
        #endregion
    }
}
