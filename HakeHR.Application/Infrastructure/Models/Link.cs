namespace HakeHR.Application.Infrastructure.Models
{

    /// <summary>
    /// Link model is used to represent each hypermedia option provided for returned 
    /// response object in api
    /// </summary>
    public class Link
    {

        /// <summary>
        /// Constructor used to initialize Link, this model is immutable after creation.
        /// </summary>
        /// <param name="href">URL to another action that can be taken</param>
        /// <param name="rel">Relationship to this resource that action has</param>
        /// <param name="method">Http method to use</param>
        public Link(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
        /// <summary>
        /// URL to another action that can be taken
        /// </summary>
        public string Href { get; }
        /// <summary>
        /// Relationship to this resource that action has
        /// </summary>
        public string Rel { get; }
        /// <summary>
        /// Http method to use
        /// </summary>
        public string Method { get; }
    }
}