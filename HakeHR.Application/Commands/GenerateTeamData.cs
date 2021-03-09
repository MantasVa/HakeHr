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
    public class GenerateTeamData
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

                    Faker<Team> generator = new Faker<Team>()
                        .RuleFor(e => e.TeamName, (f, e)
                            => $"{f.Commerce.ProductName()} {f.Random.Number(int.MaxValue)}");

                    var teams = Enumerable.Range(0, request.Count)
                        .Select(_ => generator.Generate())
                        .ToList();

                    await teamRepository.BulkInsertAsync(teams);
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GenerateTeamData command", response);
                return response;
            }
        }
    }
}
