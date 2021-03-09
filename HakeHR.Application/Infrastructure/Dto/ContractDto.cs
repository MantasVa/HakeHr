using System;
using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Application.Infrastructure.Dto
{
    public class ContractDto : IDto
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public int StatusId { get; set; }
        public ICollection<Link> Links { get; set; } = new List<Link>();
    }
}
