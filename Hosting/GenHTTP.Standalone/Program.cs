using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ServiceProcess;
using GenHTTP.Core;

namespace GenHTTP.Hosting.Standalone
{

    class Program
    {

        public static void Main(string[] args)
        {
            // ToDo: Reimplement

            /*try
            {
                // check, whether the help argument is given to the application
                foreach (string arg in args)
                {
                    if (arg == "-help" || arg == "/?")
                    {
                        Console.WriteLine();
                        Console.WriteLine("  GenHTTP Webserver");
                        Console.WriteLine();
                        Console.WriteLine("-port:<Port>\t\t1-65536");
                        Console.WriteLine();
                        Console.WriteLine("  Specifies the port the server is listening on.");
                        Console.WriteLine();
                        Console.WriteLine("-backlog:<Backlog>\t0-99999");
                        Console.WriteLine();
                        Console.WriteLine("  Specifies the backlog of the server socket.");
                        Console.WriteLine();
                        Console.WriteLine("-protocol:<Version>\t1.0 | 1.1");
                        Console.WriteLine();
                        Console.WriteLine("  Specifies the version of the HTTP protocol the server should use.");
                        Console.WriteLine();
                        Console.WriteLine("-root:<Path>\t\tDirectory");
                        Console.WriteLine();
                        Console.WriteLine("  Specifies the directory, in which the server will search for projects.");
                        Console.WriteLine();
                        Console.WriteLine("-gzip:<Value>\t\ton = enabled, off = disabled");
                        Console.WriteLine();
                        Console.WriteLine("  Enabled or disable GZip compression.");
                        Console.WriteLine();
                        Console.WriteLine("-gzip-limit:<Limit>\t\t1-4294967296");
                        Console.WriteLine();
                        Console.WriteLine("  Up to this limit the server will compress the content to send.");
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("If you do not specify parameters, the configuration file will be used.");
                        Console.WriteLine();
                        return;
                    }
                }
                // create new server
                var svr = new Server();
                // configure server by arguments
                Regex port = new Regex("-port:([0-9]{1,5})");
                Regex backlog = new Regex("-backlog:([0-9]{1,5})");
                Regex http = new Regex(@"-protocol:(1\.0|1)");
                Regex root = new Regex(@"-root:([a-zA-Z0-9" + Regex.Escape(".\\/_\":-") + "]+)");
                Regex gzip = new Regex(@"-gzip:(on|off)");
                Regex limit = new Regex("-gzip-limit:([0-9]{1,7})");
                foreach (string arg in args)
                {
                    if (port.IsMatch(arg)) svr.Configuration.Port = Convert.ToUInt16(port.Match(arg).Groups[1].Value);
                    if (backlog.IsMatch(arg)) svr.Configuration.Backlog = Convert.ToUInt16(backlog.Match(arg).Groups[1].Value);
                    if (http.IsMatch(arg)) svr.Configuration.Protocol = ((http.Match(arg).Groups[1].Value == "1.1") ? ProtocolType.Http_1_1 : ProtocolType.Http_1_0);
                    if (root.IsMatch(arg)) svr.Configuration.DocumentRoot = root.Match(arg).Groups[1].Value;
                    if (gzip.IsMatch(arg))
                    {
                        svr.Configuration.CompressionEnabled = (gzip.Match(arg).Groups[1].Value == "on");
                        svr.Configuration.CompressionAlgorithm = "gzip";
                    }
                    if (limit.IsMatch(arg)) svr.Configuration.CompressionLimit = Convert.ToUInt32(limit.Match(arg).Groups[1].Value);
                }
                svr.Listen();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + Environment.NewLine + e.StackTrace);
                Console.ReadLine();
            }*/
        }

    }

}
