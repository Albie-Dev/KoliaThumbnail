using System;

namespace Kolia.Thumbnail.API.Exceptions
{
    /// <summary>
    /// Lớp cơ sở cho tất cả ngoại lệ của ứng dụng.
    /// </summary>
    public abstract class AppException : Exception
    {
        protected AppException(
            string message,
            string code,
            int statusCode)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        protected AppException(
            string message,
            Exception innerException,
            string code,
            int statusCode)
            : base(message, innerException)
        {
            Code = code;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Mã lỗi nghiệp vụ.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// HTTP Status Code.
        /// </summary>
        public int StatusCode { get; }
    }
}