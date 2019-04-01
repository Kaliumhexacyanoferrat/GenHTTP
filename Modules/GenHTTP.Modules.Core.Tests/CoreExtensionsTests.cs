using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Tests
{

    public class CoreExtensionsTests
    {

        [Fact]
        public void TestNoExtension()
        {
            Assert.Null("Dockerfile".GuessContentType());
        }

        [Fact]
        public void TestKnownExtension()
        {
            Assert.Equal(ContentType.TextCss, "test.css".GuessContentType());
        }

        [Fact]
        public void TestKnownExtensionUppercased()
        {
            Assert.Equal(ContentType.TextCss, "test.CSS".GuessContentType());
        }

        [Fact]
        public void TestOther()
        {
            Assert.Null("test.xyz".GuessContentType());
        }

    }

}
