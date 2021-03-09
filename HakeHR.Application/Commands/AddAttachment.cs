using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

namespace HakeHR.Application.Commands
{
    public class AddAttachment
    {
        public class Command : IRequest<ResponseObject>
        {
            public int Id { get; set; }
            public Attachment[] Attachments { get; set; }
            public AttachmentFor AttachmentsFor { get; set; }

            public class Attachment
            {
                public string Filename { get; set; }
                public int ContentLength { get; set; }
                public string ContentType { get; set; }
                public Stream FileContent { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).GreaterThan(0);
                //File is less than 10MB
                RuleForEach(c => c.Attachments).Must(att
                   => att.ContentLength < 1024 * 1024 * 10)
                    .WithMessage("File is too large. Attachments should be under 10 MB");
                RuleForEach(c => c.Attachments).Must(att
                   => att.FileContent.Length != 0)
                    .WithMessage("Added file is empty.");
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
                    if (blobStorageConn.StorageAccountConn is null || blobStorageConn.ContainerName is null)
                        throw new InvalidOperationException("Adding photos is unavailable");
                    new CommandValidator().ValidateAndThrow(request);

                    var blobStorageHandler = new BlobStorageHandler(blobStorageConn.StorageAccountConn,
                                    blobStorageConn.ContainerName);

                    string basePath = ConfigurationManager.AppSettings["AttachmentsPath"];
                    request.Attachments = AddBasePathToFilenames(basePath, request.Attachments);

                    (bool exists, string filename) attachment = await FilenameExistsInBlobAsync(blobStorageHandler, request.Attachments);
                    if (attachment.exists)
                    {
                        throw new ArgumentException($"Attachment {attachment.filename.Replace(basePath, "")} name is not unique. Change name and try again.");
                    }

                    object entity = null;
                    switch (request.AttachmentsFor)
                    {
                        case AttachmentFor.Contract:
                            entity = await contractRepository.GetRecordByIdAsync(request.Id);
                            break;
                        case AttachmentFor.Employee:
                            entity = await employeeRepository.GetRecordByIdAsync(request.Id);
                            break;
                        case AttachmentFor.Organization:
                            entity = await organizationRepository.GetRecordByIdAsync(request.Id);
                            break;
                        case AttachmentFor.Team:
                            entity = await teamRepository.GetRecordByIdAsync(request.Id);
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported attachment owner");
                    }

                    if (entity is null)
                    {
                        throw new ArgumentException($"{request.AttachmentsFor} by id is not found.", "id");
                    }

                    await UploadFilesToBlobAsync(blobStorageHandler, request.Attachments);
                    await attachmentRepository.InserAttachmentsAsync(request.Attachments.Adapt<Attachment[]>(), request.AttachmentsFor, request.Id);

                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from AddAttachment command", response);
                return response;
            }
            private async Task<(bool exists, string filename)> FilenameExistsInBlobAsync(BlobStorageHandler blobStorageHandler, Command.Attachment[] attachments)
            {
                for (int i = 0; i < attachments.Length; i++)
                {
                    if (await blobStorageHandler.FileExists(attachments[i].Filename))
                    {
                        return (true, attachments[i].Filename);
                    }
                }
                return (false, string.Empty);
            }

            private static Command.Attachment[] AddBasePathToFilenames(string basePath, Command.Attachment[] attachments)
            {
                for (int i = 0; i < attachments.Length; i++)
                {
                    attachments[i].Filename = basePath + attachments[i].Filename;
                }
                return attachments;
            }

            private async Task UploadFilesToBlobAsync(BlobStorageHandler blobStorageHandler, Command.Attachment[] attachmentsToInsert)
            {
                for (int i = 0; i < attachmentsToInsert.Length; i++)
                {
                    bool uploadSuccesful = await blobStorageHandler.UploadFileAsync(attachmentsToInsert[i].FileContent,
                            attachmentsToInsert[i].Filename, attachmentsToInsert[i].ContentType);

                    if (!uploadSuccesful)
                    {
                        Log.Error($"File upload failed. Filepath: {attachmentsToInsert[i].Filename}, Type: {attachmentsToInsert[i].ContentType}");
                        throw new InvalidOperationException($"File upload failed. Filepath: {attachmentsToInsert[i].Filename}, Type: {attachmentsToInsert[i].ContentType}");
                    }
                }
            }
        }
    }

}

