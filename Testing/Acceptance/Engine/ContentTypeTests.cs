using System;

using GenHTTP.Api.Protocol;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ContentTypeTests
    {

        [TestMethod]
        public void MapContentTypeTests()
        {
            foreach (ContentType contentType in Enum.GetValues(typeof(ContentType)))
            {
                var mapped = new FlexibleContentType(contentType);

                Assert.AreEqual(mapped.KnownType, contentType);
            }
        }

    }

}
