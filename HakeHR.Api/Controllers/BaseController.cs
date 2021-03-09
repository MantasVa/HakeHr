using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using HakeHR.Api.Hateoas;
using HakeHR.Application.Infrastructure.Dto;
using HakeHR.Application.Infrastructure.Models;

namespace HakeHR.Api.Controllers
{
    /// <summary>
    /// Base Controller
    /// Formats response, adds links for other controllers
    /// </summary>
    public abstract class BaseController : ApiController
    {

        /// <summary>
        /// Formats response from application layer
        /// </summary>
        /// <param name="response">non-generic response object</param>
        protected IHttpActionResult FormatResponse(ResponseObject response)
        {
            switch (response.ResponseCode)
            {
                case HttpStatusCode.OK:
                    return Ok();
                case HttpStatusCode.Created:
                    return StatusCode(HttpStatusCode.Created);
                case HttpStatusCode.NoContent:
                    return StatusCode(HttpStatusCode.NoContent);
                default:
                    return Format(response);
            }
        }

        /// <summary>
        /// Formats response from application layer
        /// </summary>
        /// <param name="response">generic response object</param>
        protected IHttpActionResult FormatResponse<T>(ResponseObject<T> response) where T : class
        {
            switch (response.ResponseCode)
            {
                case HttpStatusCode.OK:
                    return Ok(GenerateHateoas(response.Object));
                default:
                    return Format(response);
            }
        }

        private IHttpActionResult Format(IResponseObject response)
        {
            switch (response.ResponseCode)
            {
                case HttpStatusCode.BadRequest:
                    return BadRequest(response.ErrorMessage);
                case HttpStatusCode.NotFound:
                    return NotFound();
                case HttpStatusCode.InternalServerError:
                    return InternalServerError(new Exception(response.ErrorMessage));
                default:
                    return InternalServerError();
            }
        }


        /// <summary>
        /// Method generates hypermedia links for queries returning objects 
        /// </summary>
        /// <typeparam name="T">Generic response(some kind of Dto)</typeparam>
        /// <param name="responseObject">Object returned from query</param>
        /// <returns></returns>
        private T GenerateHateoas<T>(T responseObject) where T : class
        {
            var hateoasHandler = new HateoasHandler();
            if (responseObject is IDto dto)
            {
                List<(string RouteName, object[] RouteParameters, string Rel, string Method)> routes = hateoasHandler.GetRoutes(dto, false);
                AddLinks(dto, routes);
            }
            else if (responseObject is IEnumerable enumerable)
            {
                var dtoCollection = enumerable.Cast<IDto>().ToList();
                foreach (IDto singleDto in dtoCollection)
                {
                    List<(string RouteName, object[] RouteParameters, string Rel, string Method)> routes = hateoasHandler.GetRoutes(singleDto, true);
                    AddLinks(singleDto, routes);
                }
            }

            return responseObject;
        }

        /// <summary>
        /// Helper for GenerateHateoas method, this method creates link and adds it to Link collection of Dto
        /// </summary>
        /// <param name="dto">Dto to add link to</param>
        /// <param name="routes">List of routes to add</param>
        private void AddLinks(IDto dto, List<(string RouteName, object[] RouteParameters, string Rel, string Method)> routes)
        {
            foreach ((string RouteName, object[] RouteParameters, string Rel, string Method) route in routes)
            {
                dto.Links.Add(new Link(Url.Link(route.RouteName, new { entity = route.RouteParameters?[0], id = route.RouteParameters?[1] }), route.Rel, route.Method));
            }
        }

    }
}