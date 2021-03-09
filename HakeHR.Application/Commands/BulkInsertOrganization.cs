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
    public class BulkInsertOrganization
    {
        public class Command : IRequest<ResponseObject>
        {
            public ICollection<OrganizationDto> Organizations { get; set; }

            public class OrganizationDto
            {
                public string OrganizationName { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Organizations).NotEmpty().Must(x => x.Count > 1).WithMessage("Collection should contain more than 1 organization");
                RuleForEach(x => x.Organizations).Must(organizations
                   => organizations.OrganizationName != null &&
                     organizations.OrganizationName.Length > 0 && organizations.OrganizationName.Length <= 50)
                    .WithMessage("Some organizations names are not in correct format. Name length should be between 1 and 50");
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
                    await organizationRepository.BulkInsertAsync(request.Organizations.Adapt<ICollection<Organization>>());
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from BulkInsertOrganization command", response);
                return response;
            }
        }
    }
}
