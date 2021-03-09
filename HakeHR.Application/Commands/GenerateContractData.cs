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
    public class GenerateContractData
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
            private readonly IContractRepository contractRepository;
            public Handler(IContractRepository contractRepository)
            {
                this.contractRepository = contractRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    new CommandValidator().ValidateAndThrow(request);

                    //# TODO ADD RANDOM STATUS ID WHEN STATUS CRUD IS FINISHED
                    Faker<Contract> generator = new Faker<Contract>()
                        .RuleFor(e => e.Salary, (f, e) => f.Finance.Amount(550, 10500))
                        .RuleFor(e => e.StartDate, (f, e) => f.Date.RecentOffset(90).Date)
                        .RuleFor(e => e.ExpireDate, (f, e) => f.Date.Future(1))
                        .RuleFor(e => e.StatusId, (f, e) => 1);

                    var contracts = Enumerable.Range(0, request.Count)
                        .Select(_ => generator.Generate())
                        .ToList();

                    await contractRepository.BulkInsertAsync(contracts);
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GenerateContractData command", response);
                return response;
            }
        }
    }
}
