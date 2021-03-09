using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Application.Infrastructure.Dto
{
    public class TeamDto : IDto
    {
        public int Id { get; set; }
        public string TeamName { get; set; }
        public string PhotoPath { get; set; }
        public EmployeeDto[] Employees { get; set; }
        public ICollection<Link> Links { get; set; } = new List<Link>();
    }
}
