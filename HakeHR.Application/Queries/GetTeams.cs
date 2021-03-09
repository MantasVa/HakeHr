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
    public class GetTeams
    {
        public class Query : IRequest<ResponseObject<ICollection<TeamDto>>>
        {
            public Query(int? recordsPerPage, int pageNumber)
            {
                RecordsPerPage = recordsPerPage;
                PageNumber = pageNumber;
            }
            public int? RecordsPerPage { get; }
            public int PageNumber { get; }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<ICollection<TeamDto>>>
        {
            private readonly ITeamRepository teamRepository;

            public Handler(ITeamRepository teamRepository)
            {
                this.teamRepository = teamRepository;
            }

            public async Task<ResponseObject<ICollection<TeamDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<ICollection<TeamDto>>();

                try
                {
                    ICollection<Team> teams = await teamRepository.GetRecordsAsync(request.RecordsPerPage, request.PageNumber);
                    response.Object = teams.Adapt<ICollection<TeamDto>>();
                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetTeams query", response);
                return response;
            }
        }
    }
}
