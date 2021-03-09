using System;
using System.Configuration;
using System.IO;
using System.Linq;
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
using MediatR;
using Serilog;

namespace HakeHR.Application.Commands
{
    public class AddPhoto
    {
        public class Command : IRequest<ResponseObject>
        {
            public int Id { get; set; }
            public string Filename { get; set; }
            public int ContentLength { get; set; }
            public string ContentType { get; set; }
            public Stream Photo { get; set; }
            public AttachmentFor PhotoFor { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Id).GreaterThan(0);
                //File is less than 1MB
                RuleFor(c => c.ContentLength).LessThan(1024 * 1024 * 1).WithMessage("File is too large. File size should be under 1MB");
                //Check that file is an image
                RuleFor(c => c.ContentType).Must(ct =>
                {
                    string[] imageFormats = new[] { "image/png", "image/jpeg" };
                    return imageFormats.Contains(ct);
                }).WithMessage("Bad file format. Photo should be .jpeg or .png");
            }
        }

        public class Handler : IRequestHandler<Command, ResponseObject>
        {
            private readonly BlobStorageConnection blobStorageConn;
            private readonly IEmployeeRepository employeeRepository;
            private readonly ITeamRepository teamRepository;
            private readonly IOrganizationRepository organizationRepository;

            public Handler(BlobStorageConnection blobStorageConn, IEmployeeRepository employeeRepository,
                ITeamRepository teamRepository, IOrganizationRepository organizationRepository)
            {
                this.blobStorageConn = blobStorageConn;
                this.employeeRepository = employeeRepository;
                this.teamRepository = teamRepository;
                this.organizationRepository = organizationRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    if (string.IsNullOrWhiteSpace(blobStorageConn.StorageAccountConn) ||
                        string.IsNullOrWhiteSpace(blobStorageConn.ContainerName))
                    {
                        throw new InvalidOperationException("Adding photos is unavailable");
                    }
                    new CommandValidator().ValidateAndThrow(request);

                    string extension = Path.GetExtension(request.Filename);
                    string basePath = string.Concat(ConfigurationManager.AppSettings["ImagesPath"], request.PhotoFor, "\\");
                    string newPhotoPath = null, oldPhotoPath = null;
                    object entity = null;

                    switch (request.PhotoFor)
                    {
                        case AttachmentFor.Employee:
                            Employee employee = await employeeRepository.GetRecordByIdAsync(request.Id);
                            newPhotoPath = string.Concat(basePath, employee?.Id, employee?.Firstname, employee?.Lastname, extension);
                            oldPhotoPath = employee?.PhotoPath;
                            entity = employee;
                            break;

                        case AttachmentFor.Team:
                            Team team = await teamRepository.GetRecordByIdAsync(request.Id);
                            newPhotoPath = string.Concat(basePath, team?.Id, team?.TeamName, extension);
                            oldPhotoPath = team?.PhotoPath;
                            entity = team;
                            break;

                        case AttachmentFor.Organization:
                            Organization organization = await organizationRepository.GetRecordByIdAsync(request.Id);
                            newPhotoPath = string.Concat(basePath, organization?.Id, organization?.OrganizationName, extension);
                            oldPhotoPath = organization?.PhotoPath;
                            entity = organization;
                            break;
                    }

                    if (entity != null)
                    {
                        await UploadPhotoToBlob(request.Photo, newPhotoPath, oldPhotoPath, request.ContentType);
                        switch (request.PhotoFor)
                        {
                            case AttachmentFor.Employee:
                                await employeeRepository.AddPhotoPathAsync(request.Id, newPhotoPath);
                                break;

                            case AttachmentFor.Team:
                                await teamRepository.AddPhotoPathAsync(request.Id, newPhotoPath);
                                break;

                            case AttachmentFor.Organization:
                                await organizationRepository.AddPhotoPathAsync(request.Id, newPhotoPath);
                                break;
                        }
                    }

                    response.ResponseCode = entity is null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from AddPhoto command", response);
                return response;
            }

            private async Task UploadPhotoToBlob(Stream newPhotoStream, string newPhotoPath,
                string oldPhotoPath, string contentType)
            {
                var blobHandler = new BlobStorageHandler(blobStorageConn.StorageAccountConn, blobStorageConn.ContainerName);
                await blobHandler.DeleteStoredBlobIfExtensionWillChangeAsync(newPhotoPath, oldPhotoPath);
                bool uploadSuccesful = await blobHandler.UploadFileAsync(newPhotoStream, newPhotoPath, contentType);

                if (!uploadSuccesful)
                    throw new InvalidOperationException("Photo upload failed");
            }
        }
    }
}
