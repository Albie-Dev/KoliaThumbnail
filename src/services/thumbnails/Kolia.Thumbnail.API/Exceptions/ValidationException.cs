namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class ValidationException : AppException
    {
        public ValidationException(
            string message,
            string code = "VALIDATION_ERROR")
            : base(message, code, StatusCodes.Status400BadRequest)
        {
        }
    }
}