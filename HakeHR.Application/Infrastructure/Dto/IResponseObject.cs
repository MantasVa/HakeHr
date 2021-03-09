using System.Net;

namespace HakeHR.Application.Infrastructure.Dto
{
    public interface IResponseObject
    {
        HttpStatusCode ResponseCode { get; set; }
        string ErrorMessage { get; set; }
    }
}
