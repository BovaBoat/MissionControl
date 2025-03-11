using MissionControl.Domain.Entities;
using MissionControl.Shared.Enums;

namespace MissionControl.Infrastructure.Interfaces
{
    public interface IComService
    {
        bool ConnectToBroker();
        bool SendBytes(List<byte> bytes);
        Task<SeaBusMessage> AwaitResponse(CommandCodeEnum commandCode);
        Task SendMessage(List<byte> payload, bool isResponseExpected = false);
    }
}
