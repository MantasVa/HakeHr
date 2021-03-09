using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Application.Infrastructure.Models;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using MediatR;
using Serilog;

namespace HakeHR.Application.Commands
{
    public class DeleteOrganization
    {
        public class Command : IRequest<ResponseObject>
        {
            public Command(int id)
            {
                Id = id;
            }
            public int Id { get; }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<Command, ResponseObject>
        {
            private readonly IOrganizationRepository organizationRepository;
            private readonly BlobStorageConnection blobStorageConn;
            public Handler(IOrganizationRepository organizationRepository, 
                BlobStorageConnection blobStorageConn)
            {
                this.organizationRepository = organizationRepository;
                this.blobStorageConn = blobStorageConn;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    new CommandValidator().ValidateAndThrow(request);

                    Organization organization = await organizationRepository.GetRecordByIdAsync(request.Id);
                    bool rowDeleted = false;
                    if(organization != null)
                    {
                        if(!string.IsNullOrWhiteSpace(organization?.PhotoPath))
                        {
                            if(string.IsNullOrWhiteSpace(blobStorageConn.StorageAccountConn) ||
                               string.IsNullOrWhiteSpace(blobStorageConn.ContainerName))
                            {
                                Log.Warning($"Unable to delete organization photo, because connection info unavailable. Filename: {organization.PhotoPath}");
                            }

                            var blobHandler = new BlobStorageHandler(blobStorageConn.StorageAccountConn, blobStorageConn.ContainerName);
                            _ = await blobHandler.DeleteIfExists(organization.PhotoPath);
                        }
                        rowDeleted = await organizationRepository.DeleteRecordAsync(request.Id);
                    }
                    response.ResponseCode = rowDeleted ? HttpStatusCode.NoContent : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from DeleteOrganization command with id {request.Id}", response);
                return response;
            }

        }
    }
}
