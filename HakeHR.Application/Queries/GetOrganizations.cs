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
    public class GetOrganizations
    {
        public class Query : IRequest<ResponseObject<ICollection<OrganizationDto>>>
        {
            public Query(int? recordsPerPage, int pageNumber)
            {
                RecordsPerPage = recordsPerPage;
                PageNumber = pageNumber;
            }
            public int? RecordsPerPage { get; }
            public int PageNumber { get; }
        }

        public class Handler : IRequestHandler<Query, ResponseObject<ICollection<OrganizationDto>>>
        {
            private readonly IOrganizationRepository organizationRepository;

            public Handler(IOrganizationRepository organizationRepository)
            {
                this.organizationRepository = organizationRepository;
            }

            public async Task<ResponseObject<ICollection<OrganizationDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var response = new ResponseObject<ICollection<OrganizationDto>>();

                try
                {
                    ICollection<Organization> organizations = await organizationRepository.GetRecordsAsync(request.RecordsPerPage, request.PageNumber);
                    response.Object = organizations.Adapt<ICollection<OrganizationDto>>();
                    response.ResponseCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler.FormatErrorResponse(ex, response);
                }

                Log.Information($"Returning response HTTP {response.ResponseCode} from GetOrganizations query", response);
                return response;
            }
        }
    }
}
