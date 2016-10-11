using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;

namespace LibA.Controllers
{
    [RoutePrefix("api/app")]
    public class AController : ApiController
    {
        private static readonly ILog Logger;
        static AController()
        {
            Logger = LogProvider.GetCurrentClassLogger();
            Logger.Debug($"{nameof(AController)}: Static Constructor");
        }

        public AController()
        {
            Logger.Debug($"{nameof(AController)}: Constructor");
        }


        [HttpGet, Route("test")]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Debug($"{nameof(AController)}: Get()");

            return Ok($"Hello from {nameof(AController)}");
        }
    }
}
