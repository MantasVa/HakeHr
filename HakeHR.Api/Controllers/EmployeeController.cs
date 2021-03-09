using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using HakeHR.Application.Commands;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Application.Queries;
using MediatR;
using Swashbuckle.Swagger.Annotations;

namespace HakeHR.Api.Controllers
{
    /// <summary>
    /// Employee Controller
    /// </summary>
    [RoutePrefix("api/employees")]
    public class EmployeeController : BaseController
    {
        private readonly IMediator mediator;

        /// <summary>
        /// </summary>
        /// <param name="mediator"> mediator for loose communication with queries and commands</param>
        public EmployeeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get employees list, if no params passed - all employees will be returned. 
        /// You can pass records per page and page number to get paginated response
        /// </summary>
        /// <param name="recordsPerPage">records to return per page, if null then all records are returned in one page</param>
        /// <param name="pageNumber">Page number to return</param>
        /// <returns>Collection of EmployeeDto</returns>
        [HttpGet, Route("", Name = "GetEmployees")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ICollection<EmployeeDto>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetList(int? recordsPerPage = null, int pageNumber = 1)
            => FormatResponse(await mediator.Send(new GetEmployees.Query(recordsPerPage, pageNumber)));

        /// <summary>
        /// Get employee by id
        /// </summary>
        /// <param name="id">employee id</param>
        /// <returns>EmployeeDto</returns>
        [HttpGet, Route("{id:int}", Name = "GetEmployeeById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(EmployeeDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Employee with this Id not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetById(int id)
            => FormatResponse(await mediator.Send(new GetEmployeeById.Query(id)));

        /// <summary>
        /// Adds new employee
        /// </summary>
        /// <param name="command">Command properties needed for insert action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("", Name = "InsertEmployee")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Insert(InsertEmployee.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Adds collection of employees
        /// </summary>
        /// <param name="command">Command properties needed for inserting employee collection</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Collection", Name = "BulkInsertEmployees")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkInsert(BulkInsertEmployee.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Generates random employees data for testing purposes
        /// </summary>
        /// <param name="count">Count how much employee records to generate</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Generate", Name = "GenerateEmployees")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully generated")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GenerateEmployees([FromUri] int count)
            => FormatResponse(await mediator.Send(new GenerateEmployeeData.Command(count)));


        /// <summary>
        /// Adds new contract for employee
        /// </summary>
        /// <param name="command">Command properties needed to add contract for employee</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("AddContract", Name = "AddContract")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Parameters for this action are incorrect")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> AddContract(AssignContractToEmployee.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Updates existing employee
        /// </summary>
        /// <param name="command">Command properties needed for update action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPut, Route("", Name = "UpdateEmployee")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, "Employee to be updated is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Update(UpdateEmployee.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Deletes employee by id
        /// </summary>
        /// <param name="id">employee id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("{id:int}", Name = "DeleteEmployee")]
        [SwaggerResponse(HttpStatusCode.NoContent, "Successfully deleted")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Employee with provided id is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Delete(int id)
            => FormatResponse(await mediator.Send(new DeleteEmployee.Command(id)));

        /// <summary>
        /// Bulk delete employees by their ids, or by specifying range from to which delete
        /// </summary>
        /// <param name="command">Collection of employee ids to delete,range from id - to id delete employees</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("Collection", Name = "BulkDeleteEmployees")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Collection or range is not provided, or incorrectly")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkDelete(BulkDeleteEmployees.Command command)
            => FormatResponse(await mediator.Send(command));
    }
}
