namespace MissionControl.Shared.Enums
{
    public enum CommandCodeEnum : byte
    {
        None = 0,
        START_MISSION = 0x01,
        GET_LOCATION = 0x02,
        GREEN_LIGTH = 0x03,
    }

    public enum BoatResponseCodeEnum
    {
        OK,
    }
}
