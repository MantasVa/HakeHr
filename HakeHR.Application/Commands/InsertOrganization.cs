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
    public class InsertOrganization
    {
        public class Command : IRequest<ResponseObject>
        {
            public string OrganizationName { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.OrganizationName).NotNull().Length(1, 50);
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
                    new CommandValidator().ValidateAndThrow(request);
                    await organizationRepository.InsertRecordAsync(request.Adapt<Organization>());
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from InsertOrganization command", response);
                return response;
            }
        }
    }
}
