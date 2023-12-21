using CodeChallenge.Models;
using CodeCodeChallenge.Tests.Integration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Net;
using System.Text;
using CodeCodeChallenge.Tests.Integration.Extensions;
using System.Collections.Generic;

namespace CodeChallenge.Tests.Integration
{
    [TestClass]
    public class CompensationControllerTests
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
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            var compensation = new Compensation()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f",
                Salary = 500000,
                EffectiveDate = DateTime.Parse("2023-12-21T00:00:00")
            };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Act
            var postRequestTask = _httpClient.PostAsync($"api/compensation/",
                new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var resultBody = response.DeserializeContent<Compensation>();
            Assert.AreEqual(resultBody.EmployeeId, compensation.EmployeeId);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("BAD_ID")]
        public void CreateCompensation_Returns_BadRequest(string employeeId)
        {
            // Arrange
            var compensation = new Compensation()
            {
                EmployeeId = employeeId
            };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Act
            var postRequestTask = _httpClient.PostAsync($"api/compensation/",
                new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void GetSingleCompensation_Returns_Ok()
        {
            // Arrange
            var compensation = new Compensation()
            {
                EmployeeId = "62c1084e-6e34-4630-93fd-9153afb65309",
                Salary = 500000,
                EffectiveDate = DateTime.Parse("2023-12-21T00:00:00")
            };
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Act
            // doing this since we want the tests to be independent of one another
            // ideally we have a test server or test data for every collection already set up that resets before every test
            var postRequestTask = _httpClient.PostAsync($"api/compensation/",
                new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var postResponse = postRequestTask.Result;

            var getRequestTask = _httpClient.GetAsync($"api/compensation/{compensation.EmployeeId}");
            var getResponse = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            var getResultBody = getResponse.DeserializeContent<IList<Compensation>>();
            Assert.AreEqual(getResultBody.Count, 1);
            Assert.IsNotNull(getResultBody[0].CompensationId);
            Assert.AreEqual(compensation.EmployeeId, getResultBody[0].EmployeeId);
            Assert.AreEqual(compensation.Salary, getResultBody[0].Salary);
            Assert.AreEqual(compensation.EffectiveDate, getResultBody[0].EffectiveDate);
        }

        [TestMethod]
        public void GetMultipleCompensations_Returns_Ok()
        {
            // Arrange
            var compensationOne = new Compensation()
            {
                EmployeeId = "b7839309-3348-463b-a7e3-5de1c168beb3",
                Salary = 500000,
                EffectiveDate = DateTime.Parse("2023-12-21T00:00:00")
            };
            var compensationTwo = new Compensation()
            {
                EmployeeId = "b7839309-3348-463b-a7e3-5de1c168beb3",
                Salary = 33300,
                EffectiveDate = DateTime.Parse("2023-12-21T00:00:00")
            };
            var requestContentOne = new JsonSerialization().ToJson(compensationOne);
            var requestContentTwo = new JsonSerialization().ToJson(compensationTwo);

            // Act
            var postCompensationOneTask = _httpClient.PostAsync($"api/compensation/",
                new StringContent(requestContentOne, Encoding.UTF8, "application/json"));
            var postCompensationTwoTask = _httpClient.PostAsync($"api/compensation/",
                new StringContent(requestContentTwo, Encoding.UTF8, "application/json"));

            var getRequestTask = _httpClient.GetAsync($"api/compensation/{compensationOne.EmployeeId}");
            var getResponse = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            var getResultBody = getResponse.DeserializeContent<IList<Compensation>>();
            Assert.AreEqual(getResultBody.Count, 2);
        }
    }
}