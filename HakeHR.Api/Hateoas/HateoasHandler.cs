using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Dto;

namespace HakeHR.Api.Hateoas
{

    /// <summary>
    /// Handler for link generating, returns routes which should be added to Link collection of Dto
    /// </summary>
    internal class HateoasHandler
    {
        // Lists of valuetuples. Valuetuple contains information about route 
        private readonly List<(string RouteName, object[] RouteParameters, string Rel, string Method)> routes
            = new List<(string RouteName, object[] RouteParameters, string Rel, string Method)>();


        /// <summary>
        /// Entry method for link generating
        /// </summary>
        /// <param name="dto">Dto for link generating</param>
        /// <param name="isCollection">boolean to know if Dto is stored in collection or just a single</param>
        /// <returns></returns>
        public List<(string RouteName, object[] RouteParameters, string Rel, string Method)> GetRoutes(IDto dto, bool isCollection)
        {
            if (routes.Count > 0)
                routes.Clear();

            if (dto is EmployeeDto employeeDto)
            {
                SetupDtoRoutes(employeeDto, isCollection);
            }
            else if (dto is ContractDto contractDto)
            {
                SetupDtoRoutes(contractDto, isCollection);
            }
            else if (dto is TeamDto teamDto)
            {
                SetupDtoRoutes(teamDto, isCollection);
            }
            else if (dto is OrganizationDto organizationDto)
            {
                SetupDtoRoutes(organizationDto, isCollection);
            }

            return routes;
        }

        private void SetupDtoRoutes(EmployeeDto employee, bool isCollection)
        {
            AddDefaultRoutes("Employee", employee.Id, isCollection);
            AddRoute("AddContract", null, "edit", "POST");
            AddRoute("AddPhoto", new object[] { "Employees", employee.Id }, "edit", "PUT");
            AddRoute("AddAttachment", new object[] { "Employees", employee.Id }, "edit", "POST");
        }

        private void SetupDtoRoutes(ContractDto contract, bool isCollection)
        {
            AddDefaultRoutes("Contract", contract.Id, isCollection);
            AddRoute("AddAttachment", new object[] { "Contract", contract.Id }, "edit", "POST");
        }

        private void SetupDtoRoutes(TeamDto team, bool isCollection)
        {
            AddDefaultRoutes("Team", team.Id, isCollection);
            AddRoute("AddEmployee", null, "edit", "POST");
            AddRoute("AddPhoto", new object[] { "Teams", team.Id }, "edit", "PUT");
            AddRoute("AddAttachment", new object[] { "Teams", team.Id }, "edit", "POST");
        }

        private void SetupDtoRoutes(OrganizationDto organization, bool isCollection)
        {
            AddDefaultRoutes("Organization", organization.Id, isCollection);
            AddRoute("AddTeam", null, "edit", "POST");
            AddRoute("AddPhoto", new object[] { "Organizations", organization.Id }, "edit", "PUT");
            AddRoute("AddAttachment", new object[] { "Organizations", organization.Id }, "edit", "POST");
        }


        /// <summary>
        /// Method adds default routes, that every controller should contain
        /// </summary>
        /// <param name="dtoName">Dto name</param>
        /// <param name="id">Entity id</param>
        /// <param name="isCollection">boolean if dto is stored in colleciton</param>
        private void AddDefaultRoutes(string dtoName, int id, bool isCollection)
        {
            if (isCollection)
            {
                AddRoute($"Get{dtoName}ById", new object[] { null, id }, "self", "GET");
            }
            else
            {
                AddRoute($"Get{dtoName}s", null, "collection", "GET");
            }
            AddRoute($"Insert{dtoName}", null, "insert", "POST");
            AddRoute($"Update{dtoName}", null, "edit", "PUT");
            AddRoute($"Delete{dtoName}", new object[] { null, id }, "delete", "DELETE");
        }
        private void AddRoute(string routeName, object[] routeParameters, string rel, string method) => routes.Add((routeName, routeParameters, rel, method));

    }
}