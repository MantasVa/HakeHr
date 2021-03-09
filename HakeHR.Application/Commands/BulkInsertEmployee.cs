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
    public class BulkInsertEmployee
    {
        public class Command : IRequest<ResponseObject>
        {
            public ICollection<EmployeeDto> Employees { get; set; }

            public class EmployeeDto
            {
                public string Firstname { get; set; }
                public string Lastname { get; set; }
                public string Email { get; set; }
                public DateTime Birthdate { get; set; }
                public string Address { get; set; }
                public string PhoneNumber { get; set; }
                public string Certifications { get; set; }
                public int? ManagerId { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Employees).NotEmpty().Must(x => x.Count > 1)
                    .WithMessage("Collection should contain more than 1 employee");
            }
        }

        public class EmployeeDtoValidator : AbstractValidator<Command.EmployeeDto>
        {
            public EmployeeDtoValidator()
            {
                RuleFor(x => x.Firstname).NotNull().Length(1, 50);
                RuleFor(x => x.Lastname).NotNull().Length(1, 50);
                RuleFor(x => x.Email).Length(5, 100);
                RuleFor(x => x.Birthdate).Must(x => x >= new DateTime(1900, 1, 1) && x <= DateTime.UtcNow.AddYears(-18));
                RuleFor(x => x.Address).Length(5, 200);
                RuleFor(x => x.PhoneNumber).NotNull().Length(5, 15);
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
                    ValidateAndThrow(request);

                    await employeeRepository.BulkInsertAsync(request.Employees.Adapt<ICollection<Employee>>());
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from BulkInsertEmployee command", response);
                return response;
            }

            private static void ValidateAndThrow(Command request)
            {
                new CommandValidator().ValidateAndThrow(request);
                var employeeDtoValidator = new EmployeeDtoValidator();
                foreach (Command.EmployeeDto employeeDto in request.Employees)
                {
                    employeeDtoValidator.ValidateAndThrow(employeeDto);
                }
            }
        }
    }
}
