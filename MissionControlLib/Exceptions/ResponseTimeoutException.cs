namespace MissionControl.Domain.Exceptions
{
    public class ResponseTimeoutException : Exception
    {
        public ResponseTimeoutException(string message)
        : base(message)
        {

        }
    }
}
