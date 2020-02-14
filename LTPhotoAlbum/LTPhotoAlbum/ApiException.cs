using System;
using System.Net;

namespace LTPhotoAlbum
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(HttpStatusCode statusCode, string message) : base($"{message} - {statusCode}")
        {
            StatusCode = statusCode;
        }
    }
}