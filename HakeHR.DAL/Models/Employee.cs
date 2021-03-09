using System;

namespace HakeHR.Persistence.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Certifications { get; set; }
        public int? ManagerId { get; set; }
        public string PhotoPath { get; set; }
    }
}
