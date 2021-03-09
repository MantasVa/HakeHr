using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HakeHR.Application.Commands;
using HakeHR.Persistence.Infrastructure.Enums;
using MediatR;
using Swashbuckle.Swagger.Annotations;

namespace HakeHR.Api.Controllers
{
    /// <summary>
    /// Attachment Controller
    /// </summary>
    [RoutePrefix("api")]
    public class AttachmentController : BaseController
    {
        private readonly IMediator mediator;
        private readonly IDictionary<string, AttachmentFor> availableEntities =
                new Dictionary<string, AttachmentFor>()
                {
                                {"CONTRACTS", AttachmentFor.Contract },
                                {"EMPLOYEES", AttachmentFor.Employee },
                                {"TEAMS", AttachmentFor.Team },
                                {"ORGANIZATIONS", AttachmentFor.Organization }
                };

        /// <summary>
        /// </summary>
        /// <param name="mediator">mediator for loose communication with queries and commands</param>
        public AttachmentController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Adds attachment
        /// </summary>
        /// <param name="entity">Entity to add attachment to(Teams, Contractc, Employees, Organizations)</param>
        /// <param name="id">Id to add attachment to</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPost, Route("{entity}/{id:int}/Attachments", Name = "AddAttachment")]
        [SwaggerResponse(HttpStatusCode.OK, "Successfully added attachment")]
        [SwaggerResponse(HttpStatusCode.UnsupportedMediaType, "Request content is not MIME multipart content")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> AddAttachment(string entity, int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            HttpFileCollection filesCollection = HttpContext.Current.Request.Files;

            if (filesCollection.Count < 1)
            {
                return BadRequest("No attachments inserted!");
            }

            IDictionary<string, AttachmentFor> availableEntities =
                new Dictionary<string, AttachmentFor>()
                {
                    {"CONTRACTS", AttachmentFor.Contract },
                    {"EMPLOYEES", AttachmentFor.Employee },
                    {"TEAMS", AttachmentFor.Team },
                    {"ORGANIZATIONS", AttachmentFor.Organization }
                };

            if (availableEntities.TryGetValue(entity.ToUpper(), out AttachmentFor attachmentFor) == false)
            {
                return BadRequest($"Bad route: {entity} is not a correct entity name.");
            }

            var attachments = new AddAttachment.Command.Attachment[filesCollection.Count];
            for (int i = 0; i < filesCollection.Count; i++)
            {
                attachments[i] = new AddAttachment.Command.Attachment
                {
                    Filename = filesCollection[i].FileName,
                    ContentLength = filesCollection[i].ContentLength,
                    ContentType = filesCollection[i].ContentType,
                    FileContent = filesCollection[i].InputStream
                };
            }

            return FormatResponse(await mediator.Send(
                new AddAttachment.Command
                {
                    Id = id,
                    Attachments = attachments,
                    AttachmentsFor = attachmentFor
                }));
        }

        /// <summary>
        /// Removes attachment from entity
        /// if no other entities found attached to file, then also removes from blob storage
        /// </summary>
        /// <param name="entity">Entity to remove attachment from(Teams, Contracts, Employees, Organizations)</param>
        /// <param name="ownerId">Id to remove attachment from</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpDelete, Route("{entity}/{ownerId:int}/Attachments/{attachmentId:int}", Name = "RemoveAttachmentNewHello")]
        [SwaggerResponse(HttpStatusCode.OK, "Successfully removed attachment")]
        [SwaggerResponse(HttpStatusCode.UnsupportedMediaType, "Request content is not MIME multipart content")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> RemoveAttachment(string entity, int ownerId, int attachmentId)
        {
            return availableEntities.TryGetValue(entity.ToUpper(), out AttachmentFor attachmentFor)
                ? FormatResponse(await mediator.Send(
                    new RemoveAttachment.Command
                    {
                        AttachmentsFor = attachmentFor,
                        OwnerId = ownerId,
                        AttachmentId = attachmentId
                    }))
                : BadRequest($"Bad route: {entity} is not a correct entity name.");
        }

        /// <summary>
        /// Attaches photo to existing entity
        /// Attached photo should be provided in request 
        /// </summary>
        /// <param name="entity">Entity to add photo to(Team,Employee,Organization)</param>
        /// <param name="id">Object id</param>
        /// <returns>Status code and message in case of bad request/internal server error</returns>
        [HttpPut, Route("{entity}/{id:int}/AddPhoto", Name = "AddPhoto")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.UnsupportedMediaType, "Request content is not MIME multipart content")]
        [SwaggerResponse(HttpStatusCode.NotFound, "Object to add photo to is not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Missing parameters/invalid parameters")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, "System error")]
        public async Task<IHttpActionResult> AddPhotoAsync(string entity, int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            HttpFileCollection filesCollection = HttpContext.Current.Request.Files;

            if (filesCollection.Count != 1 || filesCollection[0].ContentLength == 0)
            {
                return BadRequest("One Photo should be inserted!");
            }
            HttpPostedFile postedFile = filesCollection[0];


            IDictionary<string, AttachmentFor> availableEntities =
                new Dictionary<string, AttachmentFor>()
                {
                    {"EMPLOYEES", AttachmentFor.Employee },
                    {"TEAMS", AttachmentFor.Team },
                    {"ORGANIZATIONS", AttachmentFor.Organization }
                };

            return availableEntities.TryGetValue(entity.ToUpper(), out AttachmentFor attachmentFor) == false
                ? BadRequest("Bad route.")
                : FormatResponse(await mediator.Send(
                new AddPhoto.Command
                {
                    Id = id,
                    Filename = postedFile.FileName,
                    ContentLength = postedFile.ContentLength,
                    ContentType = postedFile.ContentType,
                    Photo = postedFile.InputStream,
                    PhotoFor = attachmentFor
                }));
        }

    }
}
