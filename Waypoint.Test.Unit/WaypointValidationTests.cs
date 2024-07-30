using MissionControlLib.Waypoints;
using NUnit.Framework;

namespace MissionControlLib.Test.Unit
{
    [TestFixture]
    public class WaypointTests
    {
        private const double VALID_LATITUDE = 45;
        private const double VALID_LONGITUDE = 45;


        [Test]
        public void CreateNewWaypoint_WhenLatitudeIsOverMaxAllowed_ShouldThrow()
        {
            var latitudeOverMaxAllowed = WaypointValidator.MAX_LATITUDE + 1;
            var validLongitude = VALID_LONGITUDE;
            var coordinatesWithLatitudeOverMaxAllowed = new Coordinates(latitudeOverMaxAllowed, validLongitude);

            Assert.Throws<InvalidWaypointCoordinatesException>(() => new Waypoint(coordinatesWithLatitudeOverMaxAllowed, null));
        }

        [Test]
        public void CreateNewWaypoint_WhenLongitudeIsOverMaxAllowed_ShouldThrow()
        {
            var validLatitude = VALID_LATITUDE;
            var longitudeOverMaxAllowed = WaypointValidator.MAX_LONGITUDE + 1;
            var coordinatesWithLongitudeOverMaxAllowed = new Coordinates(validLatitude, longitudeOverMaxAllowed);

            Assert.Throws<InvalidWaypointCoordinatesException>(() => new Waypoint(coordinatesWithLongitudeOverMaxAllowed, null));
        }

        [Test]
        public void CreateNewWaypoint_WhenLatitudeIsUnderMinAllowed_ShouldThrow()
        {
            var latitudeUnderMinAllowed = WaypointValidator.MIN_LATITUDE - 1;
            var validLongitude = VALID_LONGITUDE;
            var coordinatesWithLatitudeUnderMinAllowed = new Coordinates(latitudeUnderMinAllowed, validLongitude);

            Assert.Throws<InvalidWaypointCoordinatesException>(() => new Waypoint(coordinatesWithLatitudeUnderMinAllowed, null));
        }

        [Test]
        public void CreateNewWaypoint_WhenLongitudeIsUnderMinAllowed_ShouldThrow()
        {
            var validLatitude = VALID_LATITUDE;
            var longitudeUnderMinAllowed = WaypointValidator.MIN_LONGITUDE - 1;
            var coordinatesWithLatitudeUnderMinAllowed = new Coordinates(validLatitude, longitudeUnderMinAllowed);

            Assert.Throws<InvalidWaypointCoordinatesException>(() => new Waypoint(coordinatesWithLatitudeUnderMinAllowed, null));
        }
    }
}