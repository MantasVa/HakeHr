using System;
using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Application.Infrastructure.Dto
{
    public class EmployeeDto : IDto
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Certifications { get; set; }
        public string PhotoPath { get; set; }
        public ManagerDto Manager { get; set; }
        public ContractDto[] Contracts { get; set; }
        public ICollection<Link> Links { get; set; } = new List<Link>();


    }
}
