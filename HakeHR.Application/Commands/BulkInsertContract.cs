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
    public class BulkInsertContract
    {
        public class Command : IRequest<ResponseObject>
        {
            public ICollection<ContractDto> Contracts { get; set; }

            public class ContractDto
            {
                public decimal Salary { get; set; }
                public DateTime StartDate { get; set; }
                public DateTime? ExpireDate { get; set; }
                public int? StatusId { get; set; }
            }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Contracts).NotEmpty().Must(x => x.Count > 1).WithMessage("Collection should contain more than 1 organization");
            }
        }

        public class ContractDtoValidator : AbstractValidator<Command.ContractDto>
        {
            public ContractDtoValidator()
            {
                RuleFor(x => x.Salary).NotNull().GreaterThan(0);
                RuleFor(x => x.StartDate).Must(x => x >= new DateTime(1900, 1, 1));
                RuleFor(x => x.ExpireDate).GreaterThan(x => x.StartDate).When(x => x.ExpireDate != null)
                    .WithMessage("Expire date should be in the future of Start date");
                RuleFor(x => x.StatusId).GreaterThan(0);
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
                    ValidateAndThrow(request);
                    await contractRepository.BulkInsertAsync(request.Contracts.Adapt<ICollection<Contract>>());
                    response.ResponseCode = HttpStatusCode.Created;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from BulkInsertOrganization command", response);
                return response;
            }

            private static void ValidateAndThrow(Command request)
            {
                new CommandValidator().ValidateAndThrow(request);
                var contractDtoValidator = new ContractDtoValidator();
                foreach (Command.ContractDto contractDto in request.Contracts)
                {
                    contractDtoValidator.ValidateAndThrow(contractDto);
                }
            }
        }
    }
}
