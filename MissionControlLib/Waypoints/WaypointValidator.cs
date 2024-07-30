namespace MissionControlLib.Waypoints
{
    public static class WaypointValidator
    {
        public const double MAX_LATITUDE = 90;
        public const double MIN_LATITUDE = -90;
        public const double MAX_LONGITUDE = 180;
        public const double MIN_LONGITUDE = -180;
        public static bool AreWaypointCoordinatesValid(double latitude, double longitude)
        {
            if (!IsLatitudeValid(latitude))
            {
                return false;
            }


            if (!IsLongitudeValid(longitude))
            {
                return false;
            }

            return true;
        }

        private static bool IsLatitudeValid(double latitude)
        {
            return latitude >= MIN_LATITUDE && latitude <= MAX_LATITUDE;
        }

        private static bool IsLongitudeValid(double longitude)
        {
            return longitude >= MIN_LONGITUDE && longitude <= MAX_LONGITUDE;
        }
    }
}
