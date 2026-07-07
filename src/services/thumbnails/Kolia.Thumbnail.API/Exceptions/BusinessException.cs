namespace Kolia.Thumbnail.API.Exceptions
{
    public sealed class BusinessException : AppException
    {
        public BusinessException(
            string message,
            string code = "BUSINESS_ERROR")
            : base(message, code, StatusCodes.Status400BadRequest)
        {
        }

        public BusinessException(
            string message,
            Exception innerException,
            string code = "BUSINESS_ERROR")
            : base(message, innerException, code, StatusCodes.Status400BadRequest)
        {
        }
    }
}