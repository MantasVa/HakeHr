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
    /// Organization Controller 
    /// </summary>
    [RoutePrefix("api/organizations")]
    public class OrganizationController : BaseController
    {
        private readonly IMediator mediator;

        /// <summary>
        /// </summary>
        /// <param name="mediator">mediator for loose communication with queries and commands</param>
        public OrganizationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get organizations list, if no params passed - all organization will be returned. 
        /// You can pass records per page and page number to get paginated response
        /// </summary>
        /// <param name="recordsPerPage">records to return per page, if null then all records are returned in one page</param>
        /// <param name="pageNumber">Page number to return</param>
        /// <returns>Collection of OrganizationDto</returns>
        [HttpGet, Route("", Name = "GetOrganizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ICollection<OrganizationDto>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetList(int? recordsPerPage = null, int pageNumber = 1)
            => FormatResponse(await mediator.Send(new GetOrganizations.Query(recordsPerPage, pageNumber)));

        /// <summary>
        /// Get organization by id
        /// </summary>
        /// <param name="id">organization id</param>
        /// <returns>OrganizationDto</returns>
        [HttpGet, Route("{id:int}", Name = "GetOrganizationById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Organization with this Id not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetById(int id)
            => FormatResponse(await mediator.Send(new GetOrganizationById.Query(id)));

        /// <summary>
        /// Adds new organization
        /// </summary>
        /// <param name="command">Command properties needed for insert action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("", Name = "InsertOrganization")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Insert(InsertOrganization.Command command) => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Adds collection of organizations
        /// </summary>
        /// <param name="command">Command properties needed for inserting organization collection</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Collection", Name = "BulkInsertOrganizations")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkInsert(BulkInsertOrganization.Command command) => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Generates random organization data for testing purposes
        /// </summary>
        /// <param name="count">Count how much organization records to generate</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Generate", Name = "GenerateOrganizations")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully generated")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GenerateOrganizations([FromUri] int count)
            => FormatResponse(await mediator.Send(new GenerateOrganizationData.Command(count)));

        /// <summary>
        /// Assigns team to the organization
        /// </summary>
        /// <param name="command">Command properties needed to assign team to the organization</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("AddTeam", Name = "AddTeam")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Parameters for this action are incorrect")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> AddTeam(AssignTeamToOrganization.Command command) => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Updates existing organization
        /// </summary>
        /// <param name="command">Command properties needed for update action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPut, Route("", Name = "UpdateOrganization")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, "Organization to be updated is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Update(UpdateOrganization.Command command) => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Deletes organization by id
        /// </summary>
        /// <param name="id">organization id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("{id:int}", Name = "DeleteOrganization")]
        [SwaggerResponse(HttpStatusCode.NoContent, "Successfully deleted")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Organization with provided id is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Delete(int id) => FormatResponse(await mediator.Send(new DeleteOrganization.Command(id)));

        /// <summary>
        /// Bulk delete organizations by their ids, or by specifying range from to which delete
        /// </summary>
        /// <param name="command">Collection of organization ids to delete,range from id - to id delete organization</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("Collection", Name = "BulkDeleteOrganizations")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Collection or range is not provided, or incorrectly")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkDelete(BulkDeleteOrganizations.Command command)
            => FormatResponse(await mediator.Send(command));
    }
}
