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
    public class AssignTeamToOrganization
    {
        public class Command : IRequest<ResponseObject>
        {
            public int OrganizationId { get; set; }
            public int TeamId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.OrganizationId).GreaterThan(0);
                RuleFor(x => x.TeamId).GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<Command, ResponseObject>
        {
            private readonly IOrganizationRepository organizationRepository;
            public Handler(IOrganizationRepository organizationRepository)
            {
                this.organizationRepository = organizationRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    await organizationRepository.AddTeamAsync(request.Adapt<TeamOrganization>());
                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from AssignTeamToOrganization command", response);
                return response;
            }
        }
    }
}
