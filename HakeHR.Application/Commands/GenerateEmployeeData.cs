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
    public class GenerateEmployeeData
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
            private readonly IEmployeeRepository employeeRepository;
            public Handler(IEmployeeRepository employeeRepository)
            {
                this.employeeRepository = employeeRepository;
            }

            public async Task<ResponseObject> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject();

                try
                {
                    new CommandValidator().ValidateAndThrow(request);

                    Faker<Employee> generator = new Faker<Employee>()
                        .RuleFor(e => e.Firstname, (f, e) => f.Name.FirstName())
                        .RuleFor(e => e.Lastname, (f, e) => f.Name.LastName())
                        .RuleFor(e => e.Email, (f, e) => f.Internet.Email())
                        .RuleFor(e => e.Birthdate, (f, e) => f.Date.PastOffset(65, DateTime.Now.AddYears(-18)).Date)
                        .RuleFor(e => e.Address, (f, e) => f.Address.FullAddress())
                        .RuleFor(e => e.PhoneNumber, (f, e) => f.Phone.PhoneNumber("(###) ### #####"));

                    var employees = Enumerable.Range(0, request.Count)
                        .Select(_ => generator.Generate())
                        .ToList();

                    await employeeRepository.BulkInsertAsync(employees);
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GenerateEmployeeData command", response);
                return response;
            }
        }
    }
}
