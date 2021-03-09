using System.Net;

namespace HakeHR.Application.Infrastructure.Dto
{
    public class ResponseObject<T> : IResponseObject where T : class
    {
        public T Object { get; set; }
        public HttpStatusCode ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ResponseObject : IResponseObject
    {
        public HttpStatusCode ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
