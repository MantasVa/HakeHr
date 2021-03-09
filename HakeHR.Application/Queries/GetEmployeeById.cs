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

namespace HakeHR.Application.Queries
{
    public class GetEmployeeById
    {
        public class Query : IRequest<ResponseObject<EmployeeDto>>
        {
            public Query(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
        }
        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<EmployeeDto>>
        {
            private readonly IEmployeeRepository employeeRepository;
            private readonly IContractRepository contractRepository;

            public Handler(IEmployeeRepository repository, IContractRepository contractRepository)
            {
                this.employeeRepository = repository;
                this.contractRepository = contractRepository;
            }

            public async Task<ResponseObject<EmployeeDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<EmployeeDto>();

                try
                {
                    new QueryValidator().ValidateAndThrow(request);
                    Employee employee = await employeeRepository.GetRecordByIdAsync(request.Id);

                    response.ResponseCode = HttpStatusCode.OK;
                    response.Object = employee.Adapt<EmployeeDto>();
                    response.Object.Manager = employee.ManagerId is null ? null : await GetEmployeeManagerAsync(employee.ManagerId);
                    response.Object.Contracts = await GetEmployeeContractsAsync(employee.Id);

                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetEmployeeById query with id {request.Id}", response);
                return response;
            }

            private async Task<ManagerDto> GetEmployeeManagerAsync(int? managerId)
            {
                Employee manager = await employeeRepository.GetRecordByIdAsync((int)managerId);
                return manager.Adapt<ManagerDto>();
            }

            private async Task<ContractDto[]> GetEmployeeContractsAsync(int employeeId)
            {
                IList<Contract> contracts = await contractRepository.GetEmployeeContractsAsync(employeeId);
                ContractDto[] contractsDto = new ContractDto[contracts.Count];
                return contracts.Adapt(contractsDto);
            }

        }
    }
}
