using Microsoft.AspNetCore.Mvc;
using MissionControl.Infrastructure;
using MissionControl.Application;
using MissionControl.WebAPI.DTO;


namespace MissionControl.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MissionControlManagerController : Controller
    {
        private readonly MissionControlsManagerService _missionControlManager;
        private readonly IServiceProvider _serviceProvider;

        public MissionControlManagerController(MissionControlsManagerService missionControlManager, IServiceProvider serviceProvider)
        {
            _missionControlManager = missionControlManager;
            _serviceProvider = serviceProvider;
        }

        [HttpPost("addMissionControl")]
        public ActionResult<string> CreateMissionControl([FromBody] MissionConfig missionConfig)
        {
            try
            {
                // Create a new MissionControlService using the factory pattern
                var missionControl = ActivatorUtilities.CreateInstance<MissionControlService>(_serviceProvider);

                string instanceId = _missionControlManager.AddService(missionControl, missionConfig.InstanceId);

                return Ok(new { InstanceId = instanceId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("instances")]
        public ActionResult<IEnumerable<string>> GetAllInstances()
        {
            try
            {
                return Ok(_missionControlManager.GetAllInstanceIds());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("instances/{instanceId}")]
        public ActionResult CheckInstance(string instanceId)
        {
            try
            {
                if (!_missionControlManager.ServiceExists(instanceId))
                {
                    return NotFound(new { Error = $"Mission control instance with ID '{instanceId}' not found." });
                }

                return Ok(new { InstanceId = instanceId, Status = "Active" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("instances/{instanceId}")]
        public ActionResult RemoveInstance(string instanceId)
        {
            try
            {
                _missionControlManager.RemoveService(instanceId);
                return Ok(new { Message = $"Mission control instance with ID '{instanceId}' removed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("instances/{instanceId}/startMission")]
        public ActionResult StartMission(string instanceId, [FromBody] CoordinatesDto coordinates)
        {
            try
            {
                var missionControl = _missionControlManager.GetService(instanceId);
                
                // Use Task.Run to execute the async method without awaiting
                Task.Run(() => {
                    // This would need to be replaced with proper conversion to the Coordinates type
                    // For now, we'll just log that the mission would be started
                    Console.WriteLine($"Starting mission for instance {instanceId} to coordinates: {coordinates.Latitude}, {coordinates.Longitude}");
                });
                
                return Ok(new { Message = "Mission start command sent." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    public class CoordinatesDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
