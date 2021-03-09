using System.Web.Http;

namespace HakeHR.Api
{
    /// <summary>
    /// Optional file that contains code for responding to application-level and session-level events 
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {

        /// <summary>
        /// This event triggers on first request, first time app is accessed
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
