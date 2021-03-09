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
    public class UpdateEmployee
    {
        public class Command : IRequest<ResponseObject>
        {
            public int Id { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Email { get; set; }
            public DateTime? Birthdate { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public string Certifications { get; set; }
            public int? ManagerId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
                RuleFor(x => x.Firstname).Length(1, 50);
                RuleFor(x => x.Lastname).Length(1, 50);
                RuleFor(x => x.Email).Length(5, 100);
                RuleFor(x => x.Birthdate).Must(x => x is null || (x >= new DateTime(1900, 1, 1) && x <= DateTime.UtcNow.AddYears(-18)));
                RuleFor(x => x.Address).Length(5, 200);
                RuleFor(x => x.PhoneNumber).Length(5, 15);
                RuleFor(x => x.Certifications).Length(1, 1000);
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
                    bool rowUpdated = await employeeRepository.UpdateRecordAsync(request.Adapt<Employee>());
                    response.ResponseCode = rowUpdated ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from UpdateEmployee command with id - {request.Id}", response);
                return response;
            }

        }
    }
}
