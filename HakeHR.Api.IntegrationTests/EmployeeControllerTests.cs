using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HakeHR.Persistence.Models;
using Newtonsoft.Json;
using Xunit;

namespace HakeHR.Api.IntegrationTests
{
    public class EmployeeControllerTests
    {
        private const string BaseUrl = "https://hakehr-api-prod-euw.azurewebsites.net/";
        private readonly HttpClient client = new HttpClient();


        [Fact]
        public async Task GetList_GetEmployeeList()
        {
            var request = CreateRequest("api/Employee/GetList", "application/json", HttpMethod.Get);

            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                Assert.NotNull(response.Content);
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                Assert.True(response.IsSuccessStatusCode);

                foreach (var employee in JsonConvert.DeserializeObject<List<Employee>>(await response.Content.ReadAsStringAsync()))
                {
                    Assert.NotEqual(0, employee.Id);
                    Assert.NotNull(employee.Firstname);
                    Assert.NotNull(employee.Lastname);
                    Assert.NotNull(employee.PhoneNumber);
                }
            }

            request.Dispose();
        }

        [Fact]
        public async Task GetById_ValidId_GetEmployeeById()
        {
            var request = CreateRequest("api/Employee/GetById/1", "application/json", HttpMethod.Get);

            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                Assert.NotNull(response.Content);
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                Assert.True(response.IsSuccessStatusCode);

                var employee = JsonConvert.DeserializeObject<Employee>(await response.Content.ReadAsStringAsync());
                Assert.Equal(1, employee.Id);
                Assert.NotNull(employee.Firstname);
                Assert.NotNull(employee.Lastname);
                Assert.NotNull(employee.PhoneNumber);

            }

            request.Dispose();
        }

        [Fact]
        public async Task GetById_NotValidId_ReturnsNotFound()
        {
            var request = CreateRequest("api/Employee/GetById/" + int.MaxValue, "application/json", HttpMethod.Get);

            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                Assert.NotNull(response.Content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            request.Dispose();
        }

        [Fact]
        public async Task InsertAndDelete_ValidEmployee_ReturnsHttp200()
        {
            DateTime a = DateTime.UtcNow.AddYears(-19);
            var insertedEmployee = new Employee
            {
                Firstname = "Name123",
                Lastname = "Lastname123",
                Address = "XXX Z-Street 32",
                Birthdate = new DateTime(a.Year, a.Month, a.Day, 0, 0, 0, DateTimeKind.Utc),
                Certifications = "ScrumScrum",
                Email = "Name123Lastname123@gmail.com",
                PhoneNumber = "+381521351"
            };

            var postRequest = CreateRequest("api/Employee/Insert", "application/json", HttpMethod.Post, insertedEmployee, new JsonMediaTypeFormatter());

            using (HttpResponseMessage response = await client.SendAsync(postRequest))
            {
                Assert.NotNull(response.Content);
                Assert.True(response.IsSuccessStatusCode);
            }
            var getRequest = CreateRequest("api/Employee/GetList", "application/json", HttpMethod.Get);

            int employeeId;
            using (HttpResponseMessage response = await client.SendAsync(getRequest))
            {
                var employees =
                    JsonConvert.DeserializeObject<List<Employee>>(await response.Content.ReadAsStringAsync());

                bool EmployeeFilter(Employee employee) => employee.Firstname == insertedEmployee.Firstname &&
                                                          employee.Lastname == insertedEmployee.Lastname &&
                                                          employee.Address == insertedEmployee.Address &&
                                                          employee.Birthdate == insertedEmployee.Birthdate &&
                                                          employee.Certifications == insertedEmployee.Certifications &&
                                                          employee.Email == insertedEmployee.Email &&
                                                          employee.PhoneNumber == insertedEmployee.PhoneNumber;

                Assert.Contains(employees, EmployeeFilter);
                employeeId = employees.Find(EmployeeFilter).Id;
            }

            var deleteRequest = CreateRequest("api/Employee/Delete/" + employeeId, "application/json", HttpMethod.Delete);
            using (HttpResponseMessage response = await client.SendAsync(deleteRequest))
            {
                Assert.NotNull(response.Content);
                Assert.True(response.IsSuccessStatusCode);
            }

            var getbyIdRequest = CreateRequest("api/Employee/GetById/" + employeeId, "application/json", HttpMethod.Get);
            using (HttpResponseMessage response = await client.SendAsync(getbyIdRequest))
            {
                Assert.NotNull(response.Content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            postRequest.Dispose();
            getRequest.Dispose();
            deleteRequest.Dispose();
            getbyIdRequest.Dispose();
        }

        [Fact]
        public async Task Update_ValidEmployee_UpdatesEmployeeSuccessfully()
        {
            DateTime a = DateTime.UtcNow.AddYears(-19);
            var insertedEmployee = new Employee
            {
                Firstname = "Update",
                Lastname = "Update",
                Address = "XXX Z-Street 32",
                Birthdate = new DateTime(a.Year - 1, a.Month, a.Day, 0, 0, 0, DateTimeKind.Utc),
                Certifications = "ScrumScrum",
                Email = "UpdateUpdate@gmail.com",
                PhoneNumber = "+381521351"
            };
            var client = new HttpClient();

            var postRequest = CreateRequest("api/Employee/Insert", "application/json", HttpMethod.Post, insertedEmployee, new JsonMediaTypeFormatter());
            using (HttpResponseMessage response = await client.SendAsync(postRequest))
            { }

            int employeeId;
            bool EmployeeFilter(Employee employee) => employee.Firstname == insertedEmployee.Firstname &&
                                                      employee.Lastname == insertedEmployee.Lastname &&
                                                      employee.Address == insertedEmployee.Address &&
                                                      employee.Birthdate == insertedEmployee.Birthdate &&
                                                      employee.Certifications == insertedEmployee.Certifications &&
                                                      employee.Email == insertedEmployee.Email &&
                                                      employee.PhoneNumber == insertedEmployee.PhoneNumber;

            var getRequest = CreateRequest("api/Employee/GetList", "application/json", HttpMethod.Get);
            using (HttpResponseMessage response = await client.SendAsync(getRequest))
            {
                var employees =
                    JsonConvert.DeserializeObject<List<Employee>>(await response.Content.ReadAsStringAsync());
                employeeId = employees.Find(EmployeeFilter).Id;
            }

            insertedEmployee.Id = employeeId;
            insertedEmployee.Firstname = "NewName";
            insertedEmployee.Lastname = "NewLastName";
            insertedEmployee.Email = "NewEmailNewEmail@gmail.com";

            var updateRequest = CreateRequest("api/Employee/Update", "application/json", HttpMethod.Put, insertedEmployee, new JsonMediaTypeFormatter());
            using (HttpResponseMessage response = await client.SendAsync(updateRequest))
            {
                Assert.NotNull(response.Content);
                Assert.True(response.IsSuccessStatusCode);
            }

            var getbyIdUpdateRequest = CreateRequest("api/Employee/GetById/" + employeeId, "application/json", HttpMethod.Get);
            using (HttpResponseMessage response = await client.SendAsync(getbyIdUpdateRequest))
            {
                Assert.NotNull(response.Content);
                Assert.True(response.IsSuccessStatusCode);

                var employee = JsonConvert.DeserializeObject<Employee>(await response.Content.ReadAsStringAsync());
                Assert.Equal(insertedEmployee.Id, employee.Id);
                Assert.Equal(insertedEmployee.Firstname, employee.Firstname);
                Assert.Equal(insertedEmployee.Lastname, employee.Lastname);
                Assert.Equal(insertedEmployee.Email, employee.Email);
            }

            var deleteRequest = CreateRequest("api/Employee/Delete/" + employeeId, "application/json", HttpMethod.Delete);
            using (HttpResponseMessage response = await client.SendAsync(deleteRequest))
            { }

            var getbyIdRequest = CreateRequest("api/Employee/GetById/" + employeeId, "application/json", HttpMethod.Get);
            using (HttpResponseMessage response = await client.SendAsync(getbyIdRequest))
            {
                Assert.NotNull(response.Content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            postRequest.Dispose();
            getRequest.Dispose();
            updateRequest.Dispose();
            getbyIdUpdateRequest.Dispose();
            deleteRequest.Dispose();
            getbyIdRequest.Dispose();
        }

        [Fact]
        public async Task Delete_NotValidId_ReturnsNotFound()
        {
            var request = CreateRequest("api/Employee/Delete/" + int.MaxValue, "application/json", HttpMethod.Delete);

            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                Assert.NotNull(response.Content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            request.Dispose();
        }

        private HttpRequestMessage CreateRequest(string url, string mthv, HttpMethod method)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri(BaseUrl + url),
                Method = method,
                Headers = { Accept = { new MediaTypeWithQualityHeaderValue(mthv) } }
            };
        }

        private HttpRequestMessage CreateRequest<T>(string url, string mthv, HttpMethod method, T content, MediaTypeFormatter formatter) where T : class
        {
            HttpRequestMessage request = CreateRequest(url, mthv, method);
            request.Content = new ObjectContent<T>(content, formatter);
            return request;
        }

    }
}
