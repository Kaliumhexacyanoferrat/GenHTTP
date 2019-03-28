using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Routing
{

    public static class Routing
    {

        public static string GetSegment(string path)
        {
            var normalized = (path.StartsWith("/")) ? path.Substring(1) : path;

            var index = normalized.IndexOf('/');

            if (index > -1)
            {
                return normalized.Substring(0, index);
            }

            return normalized;
        }
        
        public static string GetRelation(int depth)
        {
            if (depth == 0)
            {
                return "./";
            }

            return new StringBuilder().Insert(0, "../", depth).ToString();
        }

    }


}
