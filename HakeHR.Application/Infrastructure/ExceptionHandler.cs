using System;
using System.Data.SqlClient;
using System.Net;
using FluentValidation;
using HakeHR.Application.Infrastructure.Dto;
using Serilog;

namespace HakeHR.Application.Infrastructure
{
    internal static class ExceptionHandler
    {
        public static ResponseObject<T> FormatErrorResponse<T>(Exception ex, ResponseObject<T> response) where T : class => Format(ex, response) as ResponseObject<T>;

        public static ResponseObject FormatErrorResponse(Exception ex, ResponseObject response) => Format(ex, response) as ResponseObject;
        private static IResponseObject Format(Exception ex, IResponseObject response)
        {
            Log.Information(ex, $"ExceptionHandler Format method is called with {ex.GetType()} exception");

            if (ex is SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    case -1:
                        response.ErrorMessage = "Server failed, please try again later.";
                        response.ResponseCode = HttpStatusCode.InternalServerError;
                        break;
                    case 201:
                        response.ErrorMessage = "Required parameter was not supplied";
                        response.ResponseCode = HttpStatusCode.BadRequest;
                        break;
                    default:
                        response.ErrorMessage = sqlException.Message;
                        response.ResponseCode = HttpStatusCode.BadRequest;
                        break;
                }
                Log.Information($"Sql exception occured with {sqlException.Number} code.");
            }
            else if (ex is ArgumentException argumentIdException && argumentIdException.ParamName == "id")
            {
                response.ResponseCode = HttpStatusCode.NotFound;
            }
            else if (ex is ArgumentException argumentException)
            {
                response.ErrorMessage = argumentException.Message;
                response.ResponseCode = HttpStatusCode.BadRequest;
            }
            else if (ex is ValidationException validationException)
            {
                response.ErrorMessage = validationException.Message;
                response.ResponseCode = HttpStatusCode.BadRequest;
            }
            else if (ex is InvalidOperationException invalidOperationException)
            {
                response.ErrorMessage = invalidOperationException.Message;
                response.ResponseCode = HttpStatusCode.InternalServerError;
            }
            else
            {
                response.ErrorMessage = "Something went wrong";
                response.ResponseCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }
    }
}
