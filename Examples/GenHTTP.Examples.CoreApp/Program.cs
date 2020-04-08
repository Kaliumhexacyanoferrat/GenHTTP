﻿using GenHTTP.Core;

namespace GenHTTP.Examples.CoreApp
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            var project = Project.Build();

            return Host.Create()
                       .Handler(project)
#if DEBUG
                       .Console()
                       .Debug()
#endif
                       .Run();
        }

    }

}
