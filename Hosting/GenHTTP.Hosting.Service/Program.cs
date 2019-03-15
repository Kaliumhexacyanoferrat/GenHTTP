using System;
using System.ServiceProcess;

namespace GenHTTP.Hosting.Service
{

    public class Program
    {

        static void Main(string[] args)
        {
            // run the GenHTTP webserver as a service
            ServiceBase.Run(new GenHTTPService());
        }

    }

}
