using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Provides methods which must be implemented by container 
    /// with headline elements.
    /// </summary>
    public interface IHeadlineContainer
    {

        /// <summary>
        /// Add a new headline.
        /// </summary>
        /// <param name="value">The value of the headline</param>
        /// <returns>The created object</returns>
        Headline AddHeadline(string value);

        /// <summary>
        /// Add a new headline.
        /// </summary>
        /// <param name="value">The value of the headline</param>
        /// <param name="size">The size of the headline (from 1 to 6)</param>
        /// <returns>The created object</returns>
        Headline AddHeadline(string value, byte size);

    }

}
