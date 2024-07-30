namespace MissionControlLib.Waypoints
{
    public class InvalidWaypointCoordinatesException : Exception
    {
        public double Latitude;
        public double Longitude;
        public InvalidWaypointCoordinatesException(double latitude, double longitude, string message)
            : base(message)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public InvalidWaypointCoordinatesException(string message)
        : base(message)
        { }
    }
}
