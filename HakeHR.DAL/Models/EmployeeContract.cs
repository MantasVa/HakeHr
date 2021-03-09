namespace HakeHR.Persistence.Models
{
    public class EmployeeContract
    {
        public int EmployeeId { get; set; }
        public int ContractId { get; set; }
        public bool IsCurrent { get; set; }
    }
}
