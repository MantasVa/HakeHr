using System;
using System.Collections.Generic;
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
    public class BulkInsertTeam
    {
        public class Command : IRequest<ResponseObject>
        {
            public ICollection<TeamDto> Teams { get; set; }

            public class TeamDto
            {
                public string TeamName { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Teams).NotEmpty().Must(x => x.Count > 1).WithMessage("Collection should contain more than 1 team");
                RuleForEach(x => x.Teams).Must(teams
                   => teams.TeamName != null &&
                     teams.TeamName.Length > 0 && teams.TeamName.Length <= 50)
                    .WithMessage("Some team names are not in correct format. Name length should be between 1 and 50");
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
                    await teamRepository.BulkInsertAsync(request.Teams.Adapt<ICollection<Team>>());
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from BulkInsertTeam command", response);
                return response;
            }
        }
    }
}
