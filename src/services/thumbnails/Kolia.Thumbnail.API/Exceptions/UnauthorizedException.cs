namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class UnauthorizedException : AppException
    {
        public UnauthorizedException(
            string message = "Unauthorized.",
            string code = "UNAUTHORIZED")
            : base(message, code, StatusCodes.Status401Unauthorized)
        {
        }
    }
}