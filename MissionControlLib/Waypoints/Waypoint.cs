namespace MissionControlLib.Waypoints
{
    public class Waypoint
    {
        double MINIMUM_DISTANCE_BETWEEN_WAYPOINTS_METERS = 5;

        public Coordinates Coordinates { get; }

        public Waypoint(Coordinates waypointCoordinates, WaypointType? waypointType)
        {
            if (!WaypointValidator.AreWaypointCoordinatesValid(waypointCoordinates.GetLatitude(), waypointCoordinates.GetLongitude()))
            {
                throw new InvalidWaypointCoordinatesException(waypointCoordinates.GetLatitude(), waypointCoordinates.GetLongitude(), "Invalid coordinates specified");
            }

            Coordinates = waypointCoordinates;
        }

        public double GetDistanceTo(Waypoint other)
        {
            double distanceInMeters = Coordinates.GetDistanceTo(other.Coordinates);

            if (distanceInMeters < MINIMUM_DISTANCE_BETWEEN_WAYPOINTS_METERS)
            {
                throw new Exception();
            }

            return distanceInMeters;
        }
    }

    public enum WaypointType
    {
        Standard,
        Start,
        Destination,
    }
}
