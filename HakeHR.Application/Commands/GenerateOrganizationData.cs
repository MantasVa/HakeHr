using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using MediatR;
using Serilog;

namespace HakeHR.Application.Commands
{
    public class GenerateOrganizationData
    {
        public class Command : IRequest<ResponseObject>
        {
            public Command(int count)
            {
                Count = count;
            }

            public int Count { get; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Count).GreaterThan(0);
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

                    Faker<Organization> generator = new Faker<Organization>()
                        .RuleFor(e => e.OrganizationName, (f, e)
                            => $"{f.Commerce.ProductName()} {f.Random.Number(int.MaxValue)}");

                    var organizations = Enumerable.Range(0, request.Count)
                        .Select(_ => generator.Generate())
                        .ToList();

                    await organizationRepository.BulkInsertAsync(organizations);
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GenerateOrganizationData command", response);
                return response;
            }
        }
    }
}
