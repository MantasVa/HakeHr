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
    public class DeleteTeam
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
            private readonly ITeamRepository teamRepository;
            private readonly BlobStorageConnection blobStorageConn;
            public Handler(ITeamRepository teamRepository, BlobStorageConnection blobStorageConn)
            {
                this.teamRepository = teamRepository;
                this.blobStorageConn = blobStorageConn;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    new CommandValidator().ValidateAndThrow(request);

                    Team team = await teamRepository.GetRecordByIdAsync(request.Id);
                    bool rowDeleted = false;
                    if(team != null)
                    {
                        if(!string.IsNullOrWhiteSpace(team?.PhotoPath))
                        {
                            if(string.IsNullOrWhiteSpace(blobStorageConn.StorageAccountConn) ||
                               string.IsNullOrWhiteSpace(blobStorageConn.ContainerName))
                            {
                                Log.Warning($"Unable to delete team photo, because connection info unavailable. Filename: {team.PhotoPath}");
                            }

                            var blobHandler = new BlobStorageHandler(blobStorageConn.StorageAccountConn, blobStorageConn.ContainerName);
                            _ = await blobHandler.DeleteIfExists(team.PhotoPath);
                        }
                        rowDeleted = await teamRepository.DeleteRecordAsync(request.Id);
                    }
                    response.ResponseCode = rowDeleted ? HttpStatusCode.NoContent : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from DeleteTeam command with id {request.Id}", response);
                return response;
            }

        }
    }
}
