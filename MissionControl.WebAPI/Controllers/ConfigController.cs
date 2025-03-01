using Microsoft.AspNetCore.Mvc;
using MissionControl.Application;

namespace MissionControl.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : Controller
    {
        private MissionControler _missionControler;

        public ConfigController(MissionControler missionController)
        {
            _missionControler = missionController;
        }

        [HttpPost]
        public ActionResult Post([FromBody] MissionConfig missionConfig)
        {
            _missionControler.Configure(missionConfig);

            return Ok();
        }
    }
}
