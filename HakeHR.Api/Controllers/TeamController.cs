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
    /// Team Controller 
    /// </summary>
    [RoutePrefix("api/teams")]
    public class TeamController : BaseController
    {
        private readonly IMediator mediator;

        /// <summary>
        /// </summary>
        /// <param name="mediator">mediator for loose communication with queries and commands</param>
        public TeamController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get teams list, if no params passed - all teams will be returned. 
        /// You can pass records per page and page number to get paginated response
        /// </summary>
        /// <param name="recordsPerPage">records to return per page, if null then all records are returned in one page</param>
        /// <param name="pageNumber">Page number to return</param>
        /// <returns>Collection of TeamDto</returns>
        [HttpGet, Route("", Name = "GetTeams")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ICollection<TeamDto>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetList(int? recordsPerPage = null, int pageNumber = 1)
            => FormatResponse(await mediator.Send(new GetTeams.Query(recordsPerPage, pageNumber)));

        /// <summary>
        /// Get team by id
        /// </summary>
        /// <param name="id">team id</param>
        /// <returns>TeamDto</returns>
        [HttpGet, Route("{id:int}", Name = "GetTeamById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(TeamDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Team with this Id not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetById(int id)
            => FormatResponse(await mediator.Send(new GetTeamById.Query(id)));

        /// <summary>
        /// Adds new team
        /// </summary>
        /// <param name="command">Command properties needed for insert action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("", Name = "InsertTeam")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Insert(InsertTeam.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Assigns employee to the team
        /// </summary>
        /// <param name="command">Command properties needed to assign employee to the team</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("AddEmployee", Name = "AddEmployee")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Parameters for this action are incorrect")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> AddEmployee(AssignEmployeeToTeam.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Adds collection of teams
        /// </summary>
        /// <param name="command">Command properties needed for inserting team collection</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Collection", Name = "BulkInsertTeams")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkInsert(BulkInsertTeam.Command command) => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Generates random team data for testing purposes
        /// </summary>
        /// <param name="count">Count how much team records to generate</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Generate", Name = "GenerateTeams")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully generated")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GenerateTeams([FromUri] int count)
            => FormatResponse(await mediator.Send(new GenerateTeamData.Command(count)));

        /// <summary>
        /// Updates existing team
        /// </summary>
        /// <param name="command">Command properties needed for update action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPut, Route("", Name = "UpdateTeam")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, "Team to be updated is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Update(UpdateTeam.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Deletes team by id
        /// </summary>
        /// <param name="id">team id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("{id:int}", Name = "DeleteTeam")]
        [SwaggerResponse(HttpStatusCode.NoContent, "Successfully deleted")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Team with provided id is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Delete(int id)
            => FormatResponse(await mediator.Send(new DeleteTeam.Command(id)));

        /// <summary>
        /// Bulk delete teams by their ids, or by specifying range from to which delete
        /// </summary>
        /// <param name="command">Collection of team ids to delete,range from id - to id delete teams</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("Collection", Name = "BulkDeleteTeams")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Collection or range is not provided, or incorrectly")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkDelete(BulkDeleteTeams.Command command)
            => FormatResponse(await mediator.Send(command));
    }
}
