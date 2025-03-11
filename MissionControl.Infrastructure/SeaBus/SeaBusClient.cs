using MissionControl.Application.Interfaces;
using MissionControl.Domain.Entities;
using MissionControl.Infrastructure.Interfaces;
using MissionControl.Shared.Enums;
using MissionControlLib.Waypoints;

namespace MissionControl.Infrastructure.SeaBus
{
    public class SeaBusClient : IProtocolService
    {
        private IComService _com;

        public SeaBusClient(IComService comService)
        {
            _com = comService;
        }

        public async Task<bool> StartMissionCommand(Coordinates destinationCoordinates)
        {
            var seaBusMessage = new SeaBusMessage(CommandCodeEnum.START_MISSION, destinationCoordinates.ToByteList());
            
            await SendCommand(seaBusMessage);
            var response = await _com.AwaitResponse(CommandCodeEnum.START_MISSION);

            await MissionStartConfirmationCommand();
            response = await _com.AwaitResponse(CommandCodeEnum.GREEN_LIGTH);

            return true;
        }

        public async Task MissionStartConfirmationCommand()
        {
            var seaBusMessage = new SeaBusMessage(CommandCodeEnum.GREEN_LIGTH);

            await SendCommand(seaBusMessage);
        }

        private async Task SendCommand(SeaBusMessage navMessage)
        {
            await _com.SendMessage(navMessage.GetMessageContentBytes());
        }
    }
}
