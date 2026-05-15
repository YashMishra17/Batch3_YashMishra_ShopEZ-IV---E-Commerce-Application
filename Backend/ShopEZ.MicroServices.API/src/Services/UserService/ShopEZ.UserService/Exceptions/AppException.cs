namespace ShopEZ.UserService.Exceptions
{
    /// <summary>
    /// Domain exception that carries an HTTP status code.
    /// Identical contract to the monolith's AppException so error
    /// response shapes are preserved end-to-end.
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}