using System;
using System.Net;

namespace SatispayGBusiness
{
    public class SatispayException : Exception
    {
        public HttpStatusCode Code { get; set; }

        public SatispayException() : base()
        {
        }
        public SatispayException(string message, HttpStatusCode code) : base(message)
        {
            Code = code;
        }
    }
    public class ActivationTokenNotFoundException : SatispayException
    {
        public ActivationTokenNotFoundException() : base("Activation token not found", HttpStatusCode.NotFound)
        { }
    }
    public class ActivationTokenAlreadyPairedException : SatispayException
    {
        public ActivationTokenAlreadyPairedException() : base("Activation token already paired", HttpStatusCode.Forbidden)
        { }
    }
    public class InvalidRsaKeyException : SatispayException
    {
        public InvalidRsaKeyException() : base("Invalid RSA key", HttpStatusCode.BadRequest)
        { }
    }
}
