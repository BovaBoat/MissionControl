namespace Waypoint
{
    public class Coordinates
    {
        private double _latitude;
        private double _longitude;

        public Coordinates(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        public double GetLatitude() { return _latitude; }
        public double GetLongitude() { return _longitude; }

        public double GetDistanceTo(Coordinates other)
        {
            uint EARTH_RADIUS_IN_METERS = 6371000;
            uint R = EARTH_RADIUS_IN_METERS;

            double dLat = this.toRadian(other._latitude - _latitude);
            double dLon = this.toRadian(other._longitude - _longitude);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.toRadian(_latitude)) * Math.Cos(this.toRadian(other._longitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;

            return d;
        }

        private double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

        public List<byte> ToByteList()
        {
            var lattitudeInBytes = BitConverter.GetBytes(_latitude);
            var longitudeInBytes = BitConverter.GetBytes(_longitude);

            var coordinatesByteList = new List<byte>();

            coordinatesByteList.AddRange(lattitudeInBytes);
            coordinatesByteList.AddRange(longitudeInBytes);

            return coordinatesByteList;
        }
    }
}
