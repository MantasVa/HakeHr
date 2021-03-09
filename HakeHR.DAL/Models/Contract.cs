using System;

namespace HakeHR.Persistence.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public int? StatusId { get; set; }
    }
}
