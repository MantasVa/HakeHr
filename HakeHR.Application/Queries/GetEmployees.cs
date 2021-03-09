using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HakeHR.Application.Infrastructure;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Persistence.Models;
using HakeHR.Persistence.Repositories.Interfaces;
using Mapster;
using MediatR;
using Serilog;

namespace HakeHR.Application.Queries
{
    public class GetEmployees
    {
        public class Query : IRequest<ResponseObject<ICollection<EmployeeDto>>>
        {
            public Query(int? recordsPerPage, int pageNumber)
            {
                RecordsPerPage = recordsPerPage;
                PageNumber = pageNumber;
            }
            public int? RecordsPerPage { get; }
            public int PageNumber { get; }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<ICollection<EmployeeDto>>>
        {
            private readonly IEmployeeRepository employeeRepository;

            public Handler(IEmployeeRepository employeeRepository)
            {
                this.employeeRepository = employeeRepository;
            }

            public async Task<ResponseObject<ICollection<EmployeeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<ICollection<EmployeeDto>>();

                try
                {
                    ICollection<Employee> employees = await employeeRepository.GetRecordsAsync(request.RecordsPerPage, request.PageNumber);
                    response.Object = employees.Adapt<ICollection<EmployeeDto>>();
                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetEmployees query", response);
                return response;
            }
        }
    }
}
