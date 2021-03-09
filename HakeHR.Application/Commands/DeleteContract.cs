using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Repositories.Interfaces;
using MediatR;
using Serilog;

namespace HakeHR.Application.Commands
{
    public class DeleteContract
    {
        public class Command : IRequest<ResponseObject>
        {
            public Command(int id)
            {
                Id = id;
            }
            public int Id { get; }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
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
                    bool rowDeleted = await contractRepository.DeleteRecordAsync(request.Id);
                    response.ResponseCode = rowDeleted ? HttpStatusCode.NoContent : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from DeleteEmployee command with id {request.Id}", response);
                return response;
            }
        }
    }
}
