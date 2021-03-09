using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using Mapster;
using MediatR;
using Serilog;

namespace HakeHR.Application.Commands
{
    public class UpdateTeam
    {
        public class Command : IRequest<ResponseObject>
        {
            public int Id { get; set; }
            public string TeamName { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
                RuleFor(x => x.TeamName).Length(1, 50);
            }
        }

        public class Handler : IRequestHandler<Command, ResponseObject>
        {
            private readonly ITeamRepository teamRepository;
            public Handler(ITeamRepository teamRepository)
            {
                this.teamRepository = teamRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    new CommandValidator().ValidateAndThrow(request);
                    bool rowUpdated = await teamRepository.UpdateRecordAsync(request.Adapt<Team>());
                    response.ResponseCode = rowUpdated ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from UpdateTeam command with id - {request.Id}", response);
                return response;
            }

        }
    }
}
