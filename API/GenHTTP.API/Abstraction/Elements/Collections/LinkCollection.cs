using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add links to a container.
    /// </summary>
    public class LinkCollection : ILinkContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new link collection.
        /// </summary>
        /// <param name="d">The delegate which allows us to add elements to the container</param>
        public LinkCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ILinkContainer Members

        /// <summary>
        /// Add an empty link.
        /// </summary>
        /// <returns>The created object</returns>
        public Link AddLink()
        {
            Link lnk = new Link();
            _Delegate(lnk);
            return lnk;
        }

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <returns>The created object</returns>
        public Link AddLink(string url)
        {
            Link lnk = new Link(url);
            _Delegate(lnk);
            return lnk;
        }

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="linkText">The link text</param>
        /// <returns>The created object</returns>
        public Link AddLink(string url, string linkText)
        {
            Link lnk = new Link(url, linkText);
            _Delegate(lnk);
            return lnk;
        }

        /// <summary>
        /// Add a new link.
        /// </summary>
        /// <param name="url">The URL to link</param>
        /// <param name="element">The link element</param>
        /// <returns>The created object</returns>
        public Link AddLink(string url, Element element)
        {
            Link lnk = new Link(url, element);
            _Delegate(lnk);
            return lnk;
        }

        #endregion
    }

}
