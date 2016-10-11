using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;

namespace LibB.Controllers
{
    [RoutePrefix("api/app")]
    public class BController : ApiController
    {
        private static readonly ILog Logger;
        static BController()
        {
            Logger = LogProvider.GetCurrentClassLogger();
            Logger.Debug($"{nameof(BController)}: Static Constructor");
        }

        public BController()
        {
            Logger.Debug($"{nameof(BController)}: Constructor");
        }


        [HttpGet, Route("{*path}")]
        public async Task<IHttpActionResult> Get([FromUri] string path)
        {
            if (path == null)
            {
                path = Request.RequestUri.PathAndQuery.Split(new[] {"api/app/"}, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            Logger.Debug($"{nameof(BController)}: Get({path})");

            using (var client = new HttpClient {BaseAddress = new Uri("http://localhost:1234/api/app/")})
            {
                var result = await client.GetAsync(path);
                var content = await result.Content.ReadAsStringAsync();
                return Ok($"(From {nameof(BController)}): {content}");
            }
        }
    }
}
