using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Application.Infrastructure.Models;
using HakeHR.Persistence.Infrastructure.Enums;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using Mapster;
using MediatR;
using Serilog;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HakeHR.Application.Commands
{
    public class RemoveAttachment
    {
        public class Command : IRequest<ResponseObject>
        {
            public AttachmentFor AttachmentsFor { get; set; }
            public int OwnerId { get; set; }
            public int AttachmentId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.AttachmentId).GreaterThan(0);
                RuleFor(c => c.OwnerId).GreaterThan(0);
            }
        }
        public class Handler : IRequestHandler<Command, ResponseObject>
        {
            private readonly BlobStorageConnection blobStorageConn;
            private readonly IAttachmentRepository attachmentRepository;

            private readonly IContractRepository contractRepository;
            private readonly IEmployeeRepository employeeRepository;
            private readonly IOrganizationRepository organizationRepository;
            private readonly ITeamRepository teamRepository;

            public Handler(BlobStorageConnection blobStorageConn, IAttachmentRepository attachmentRepository,
                IContractRepository contractRepository, IEmployeeRepository employeeRepository,
                IOrganizationRepository organizationRepository, ITeamRepository teamRepository)
            {
                this.blobStorageConn = blobStorageConn;
                this.attachmentRepository = attachmentRepository;
                this.contractRepository = contractRepository;
                this.employeeRepository = employeeRepository;
                this.organizationRepository = organizationRepository;
                this.teamRepository = teamRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    if(blobStorageConn.StorageAccountConn is null || blobStorageConn.ContainerName is null)
                        throw new InvalidOperationException("Adding photos is unavailable");

                    new CommandValidator().ValidateAndThrow(request);

                    object entity = null;
                    switch(request.AttachmentsFor)
                    {
                        case AttachmentFor.Contract:
                            entity = await contractRepository.GetRecordByIdAsync(request.OwnerId);
                            break;
                        case AttachmentFor.Employee:
                            entity = await employeeRepository.GetRecordByIdAsync(request.OwnerId);
                            break;
                        case AttachmentFor.Organization:
                            entity = await organizationRepository.GetRecordByIdAsync(request.OwnerId);
                            break;
                        case AttachmentFor.Team:
                            entity = await teamRepository.GetRecordByIdAsync(request.OwnerId);
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported attachment owner");
                    }

                    if(entity is null)
                    {
                        throw new ArgumentException($"{request.AttachmentsFor} by id is not found.", "id");
                    }

                    string filename = await attachmentRepository.RemoveAttachmentAsync(request.AttachmentsFor,
                        request.OwnerId, request.AttachmentId);

                    if(!string.IsNullOrWhiteSpace(filename))
                    {
                        var blobStorageHandler = new BlobStorageHandler(blobStorageConn.StorageAccountConn,
                                    blobStorageConn.ContainerName);
                        _ = await blobStorageHandler.DeleteIfExists(filename);
                    }

                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch(Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from AddAttachment command", response);
                return response;
            }
        }
    }
}
