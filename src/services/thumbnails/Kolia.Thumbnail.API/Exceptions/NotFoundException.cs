namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(
            string message,
            string code = "NOT_FOUND")
            : base(message, code, StatusCodes.Status404NotFound)
        {
        }
    }
}