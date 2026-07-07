namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class ConflictException : AppException
    {
        public ConflictException(
            string message,
            string code = "CONFLICT")
            : base(message, code, StatusCodes.Status409Conflict)
        {
        }
    }
}