using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Repositories.Interfaces;
using MediatR;
using Serilog;


namespace HakeHR.Application.Commands
{
    public class BulkDeleteTeams
    {
        public class Command : IRequest<ResponseObject>
        {
            public int? StartIndex { get; set; }
            public int? EndIndex { get; set; }
            public ICollection<int> TeamIds { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            const string ValidationMessage = "Collection or range should not be empty";
            public CommandValidator()
            {
                RuleFor(c => c.TeamIds).NotEmpty()
                    .WithMessage(ValidationMessage)
                    .When(c => c.StartIndex is null && c.EndIndex is null);
                RuleFor(c => c.EndIndex).GreaterThan(c => c.StartIndex)
                    .When(c => c.StartIndex != null && c.EndIndex != null);
                RuleForEach(c => c.TeamIds).Must(id => id > 0)
                    .WithMessage("Collection Ids should be greater than 0");
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

                    await teamRepository.BulkDeleteAsync(request.StartIndex, request.EndIndex, request.TeamIds);
                    response.ResponseCode = HttpStatusCode.NoContent;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from BulkDeleteTeam command", response);
                return response;
            }
        }
    }
}
