namespace MissionControl.WebApp.Services
{
    public class MissionControlDataService
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public void SetCoordinatesFromString(string lattitudeString, string longitudeString)
        {
            Latitude = double.Parse(lattitudeString, System.Globalization.CultureInfo.InvariantCulture);
            Longitude = double.Parse(longitudeString, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
