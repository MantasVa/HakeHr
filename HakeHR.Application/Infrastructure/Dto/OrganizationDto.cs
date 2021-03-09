using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Application.Infrastructure.Dto
{
    public class OrganizationDto : IDto
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; }
        public string PhotoPath { get; set; }
        public TeamDto[] Teams { get; set; }
        public ICollection<Link> Links { get; set; } = new List<Link>();
    }
}
