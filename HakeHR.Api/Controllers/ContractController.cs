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
    /// Contract Controller
    /// </summary>
    [RoutePrefix("api/contracts")]
    public class ContractController : BaseController
    {
        private readonly IMediator mediator;

        /// <summary>
        /// </summary>
        /// <param name="mediator">mediator for loose communication with queries and commands</param>
        public ContractController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get contracts list, if no params passed - all contracts will be returned. 
        /// You can pass records per page and page number to get paginated response
        /// </summary>
        /// <param name="recordsPerPage">records to return per page, if null then all records are returned in one page</param>
        /// <param name="pageNumber">Page number to return</param>
        /// <returns>Collection of ContractDto</returns>
        [HttpGet, Route("", Name = "GetContracts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ICollection<ContractDto>))]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetList(int? recordsPerPage = null, int pageNumber = 1)
            => FormatResponse(await mediator.Send(new GetContracts.Query(recordsPerPage, pageNumber)));

        /// <summary>
        /// Get contract by id
        /// </summary>
        /// <param name="id">contract id</param>
        /// <returns>ContractDto</returns>
        [HttpGet, Route("{id:int}", Name = "GetContractById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ContractDto))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Contract with this Id not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GetById(int id)
            => FormatResponse(await mediator.Send(new GetContractById.Query(id)));

        /// <summary>
        /// Adds new contract
        /// </summary>
        /// <param name="command">Command properties needed for insert action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("", Name = "InsertContract")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Insert(InsertContract.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Adds collection of contracts
        /// </summary>
        /// <param name="command">Command properties needed for inserting contract collection</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Collection", Name = "BulkInsertContracts")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully inserted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkInsert(BulkInsertContract.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Generates random contract data for testing purposes
        /// </summary>
        /// <param name="count">Count how much contract records to generate</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("Generate", Name = "GenerateContracts")]
        [SwaggerResponse(HttpStatusCode.Created, "Successfully generated")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> GenerateContracts([FromUri] int count)
            => FormatResponse(await mediator.Send(new GenerateContractData.Command(count)));

        /// <summary>
        /// Updates existing contract
        /// </summary>
        /// <param name="command">Command properties needed for update action</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPut, Route("", Name = "UpdateContract")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound, "Contract to be updated is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Update(UpdateContract.Command command)
            => FormatResponse(await mediator.Send(command));

        /// <summary>
        /// Deletes contract by id
        /// </summary>
        /// <param name="id">contract id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("{id:int}", Name = "DeleteContract")]
        [SwaggerResponse(HttpStatusCode.NoContent, "Successfully deleted")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Contract with provided id is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Id is less than 1")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> Delete(int id)
            => FormatResponse(await mediator.Send(new DeleteContract.Command(id)));

        /// <summary>
        /// Bulk delete contracts by their ids, or by specifying range from to which delete
        /// </summary>
        /// <param name="command">Collection of contract ids to delete,range from id - to id delete contracts</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("Collection", Name = "BulkDeleteContracts")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Collection or range is not provided, or incorrectly")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> BulkDelete(BulkDeleteContracts.Command command)
            => FormatResponse(await mediator.Send(command));
    }
}
