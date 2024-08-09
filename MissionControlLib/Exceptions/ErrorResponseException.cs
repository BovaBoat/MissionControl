namespace MissionControlLib.Exceptions
{
    public class ErrorResponseException : Exception
    {
        public ErrorResponseException(string message)
        : base(message)
        {

        }
    }
}
