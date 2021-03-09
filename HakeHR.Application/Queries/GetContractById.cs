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

namespace HakeHR.Application.Queries
{
    public class GetContractById
    {
        public class Query : IRequest<ResponseObject<ContractDto>>
        {
            public int Id { get; set; }

            public Query(int id)
            {
                Id = id;
            }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<ContractDto>>
        {
            private readonly IContractRepository contractRepository;

            public Handler(IContractRepository contractRepository)
            {
                this.contractRepository = contractRepository;
            }
            public async Task<ResponseObject<ContractDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<ContractDto>();

                try
                {
                    new QueryValidator().ValidateAndThrow(request);
                    Contract contract = await contractRepository.GetRecordByIdAsync(request.Id);
                    response.ResponseCode = HttpStatusCode.OK;

                    if (response.ResponseCode == HttpStatusCode.OK)
                    {
                        response.Object = contract.Adapt<ContractDto>();
                    }
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetContractById query with id {request.Id}", response);
                return response;
            }
        }
    }
}
