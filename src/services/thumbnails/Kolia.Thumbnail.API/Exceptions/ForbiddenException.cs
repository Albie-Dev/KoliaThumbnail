namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class ForbiddenException : AppException
    {
        public ForbiddenException(
            string message = "Forbidden.",
            string code = "FORBIDDEN")
            : base(message, code, StatusCodes.Status403Forbidden)
        {
        }
    }
}