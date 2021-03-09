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
    public class GetTeamById
    {
        public class Query : IRequest<ResponseObject<TeamDto>>
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

        public class Handler : IRequestHandler<Query, ResponseObject<TeamDto>>
        {
            private readonly ITeamRepository teamRepository;
            private readonly IEmployeeRepository employeeRepository;

            public Handler(ITeamRepository teamRepository, IEmployeeRepository employeeRepository)
            {
                this.teamRepository = teamRepository;
                this.employeeRepository = employeeRepository;
            }

            public async Task<ResponseObject<TeamDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<TeamDto>();

                try
                {
                    new QueryValidator().ValidateAndThrow(request);
                    Team team = await teamRepository.GetRecordByIdAsync(request.Id);

                    response.ResponseCode = HttpStatusCode.OK;
                    response.Object = team.Adapt<TeamDto>();
                    response.Object.Employees = await GetTeamMembersAsync(team.Id);
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetTeamById query with id {request.Id}", response);
                return response;
            }

            private async Task<EmployeeDto[]> GetTeamMembersAsync(int teamId)
            {
                IList<Employee> employees = await employeeRepository.GetTeamMembersAsync(teamId);
                EmployeeDto[] employeesDto = new EmployeeDto[employees.Count];
                return employees.Adapt(employeesDto);
            }
        }
    }
}
