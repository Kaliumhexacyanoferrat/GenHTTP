using System.Collections.Generic;

using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core
{

    public static class WebPathExtensions
    {

        public static string RelativeTo(this WebPath path, WebPath target)
        {
            var common = CommonParts(path, target);

            var hops = (path.Parts.Count - common + (path.TrailingSlash ? 1 : 0)) - 1;

            var relativeParts = new List<string>();

            if (hops > 0)
            {
                for (int i = 0; i < hops; i++)
                {
                    relativeParts.Add("..");
                }
            }
            else
            {
                relativeParts.Add(".");
            }

            var added = false;

            for (int i = common; i < target.Parts.Count; i++)
            {
                relativeParts.Add(target.Parts[i]);
                added = true;
            }

            var trailing = target.TrailingSlash || !added;

            return new WebPath(relativeParts, trailing).ToString().Substring(1);
        }

        private static int CommonParts(WebPath one, WebPath two)
        {
            int common;

            for (common = 0; common < one.Parts.Count; common++)
            {
                if (common >= two.Parts.Count)
                {
                    return common;
                }

                if (two.Parts[common] != one.Parts[common])
                {
                    return common;
                }
            }

            return common;
        }

    }

}
