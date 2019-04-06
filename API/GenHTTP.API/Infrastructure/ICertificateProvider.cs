using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public interface ICertificateProvider
    {

        X509Certificate2 Provide(string? host);

    }

}
