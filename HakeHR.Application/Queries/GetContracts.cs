using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Repositories.Interfaces;
using Mapster;
using MediatR;
using Serilog;

namespace HakeHR.Application.Queries
{
    public class GetContracts
    {
        public class Query : IRequest<ResponseObject<ICollection<ContractDto>>>
        {
            public Query(int? recordsPerPage, int pageNumber)
            {
                RecordsPerPage = recordsPerPage;
                PageNumber = pageNumber;
            }
            public int? RecordsPerPage { get; }
            public int PageNumber { get; }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<ICollection<ContractDto>>>
        {
            private readonly IContractRepository contractRepository;

            public Handler(IContractRepository contractRepository)
            {
                this.contractRepository = contractRepository;
            }

            public async Task<ResponseObject<ICollection<ContractDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<ICollection<ContractDto>>();

                try
                {
                    ICollection<Persistence.Models.Contract> contracts = await contractRepository.GetRecordsAsync(request.RecordsPerPage, request.PageNumber);
                    response.Object = contracts.Adapt<ICollection<ContractDto>>();
                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetContracts query", response);
                return response;
            }
        }
    }
}
