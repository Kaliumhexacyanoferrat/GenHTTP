using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Content
{

    public sealed class ContentInfoBuilder : IBuilder<ContentInfo>, IContentInfoBuilder<ContentInfoBuilder>
    {
        private string? _Title, _Description;

        #region Functionality

        /// <summary>
        /// Sets the description of the element.
        /// </summary>
        /// <param name="description">The description of the element</param>
        public ContentInfoBuilder Description(string description)
        {
            _Description = description;
            return this;
        }

        /// <summary>
        /// Sets the title of the element.
        /// </summary>
        /// <param name="description">The title of the element</param>
        public ContentInfoBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public ContentInfo Build() => new(_Title, _Description);

        #endregion

    }

}
