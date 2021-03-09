using System.Collections.Generic;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Application.Infrastructure.Dto
{
    public interface IDto
    {
        int Id { get; set; }
        ICollection<Link> Links { get; set; }
    }
}
