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
    public class GetOrganizationById
    {
        public class Query : IRequest<ResponseObject<OrganizationDto>>
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

        public class Handler : IRequestHandler<Query, ResponseObject<OrganizationDto>>
        {
            private readonly IOrganizationRepository organizationepository;
            private readonly ITeamRepository teamRepository;

            public Handler(IOrganizationRepository organizationepository, ITeamRepository teamRepository)
            {
                this.organizationepository = organizationepository;
                this.teamRepository = teamRepository;
            }

            public async Task<ResponseObject<OrganizationDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<OrganizationDto>();

                try
                {
                    new QueryValidator().ValidateAndThrow(request);
                    Organization organization = await organizationepository.GetRecordByIdAsync(request.Id);

                    response.ResponseCode = HttpStatusCode.OK;
                    response.Object = organization.Adapt<OrganizationDto>();
                    response.Object.Teams = await GetOrganizationTeamsAsync(organization.Id);

                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetOrganizationById query with id {request.Id}", response);
                return response;
            }

            private async Task<TeamDto[]> GetOrganizationTeamsAsync(int organizationId)
            {
                IList<Team> teams = await teamRepository.GetOrganizationTeamsAsync(organizationId);
                TeamDto[] teamsDto = new TeamDto[teams.Count];
                return teams.Adapt(teamsDto);
            }

        }
    }

}
