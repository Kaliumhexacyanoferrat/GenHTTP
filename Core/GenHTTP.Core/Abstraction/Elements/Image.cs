using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// Embeds a picture into a document.
    /// </summary>
    public class Image : StyledElement
    {
        private string _Source;
        private string _Alternative;
        private string _LongDesc;
        private string _UseMap;
        private bool _IsMap;

        #region Constructors

        /// <summary>
        /// Create a new image element.
        /// </summary>
        /// <param name="source">The source of the image (an URL)</param>
        /// <param name="alternativeDescription">An alternative description of the image (required)</param>
        public Image(string source, string alternativeDescription)
        {
            Source = source;
            Alternative = alternativeDescription;
        }

        /// <summary>
        /// Create a new image element.
        /// </summary>
        /// <param name="source">The source of the image (an URL)</param>
        /// <param name="alternativeDescription">An alternative description of the image (required)</param>
        /// <param name="longDescription">The URL of a document with more information about this image</param>
        public Image(string source, string alternativeDescription, string longDescription)
          : this(source, alternativeDescription)
        {
            _LongDesc = longDescription;
        }

        /// <summary>
        /// Create a new image element.
        /// </summary>
        /// <param name="source">The source of the image (an URL)</param>
        /// <param name="alternativeDescription">An alternative description of the image (required)</param>
        /// <param name="longDescription">The URL of a document with more information about this image</param>
        /// <param name="mapSource">The map to use for this image</param>
        public Image(string source, string alternativeDescription, string longDescription, string mapSource)
          : this(source, alternativeDescription)
        {
            _LongDesc = longDescription;
            _UseMap = mapSource;
        }

        /// <summary>
        /// Create a new image element.
        /// </summary>
        /// <param name="source">The source of the image (an URL)</param>
        /// <param name="alternativeDescription">An alternative description of the image (required)</param>
        /// <param name="isMap">Specify, whether this image is a map</param>
        public Image(string source, string alternativeDescription, bool isMap)
          : this(source, alternativeDescription)
        {
            _IsMap = isMap;
        }

        /// <summary>
        /// Create a new image element.
        /// </summary>
        /// <param name="source">The source of the image (an URL)</param>
        /// <param name="alternativeDescription">An alternative description of the image (required)</param>
        /// <param name="longDescription">The URL of a document with more information about this image</param>
        /// <param name="isMap">Specify, whether this image is a map</param>
        public Image(string source, string alternativeDescription, string longDescription, bool isMap)
          : this(source, alternativeDescription)
        {
            _LongDesc = longDescription;
            _IsMap = isMap;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The URL of the image.
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set
            {
                if (value == null || value.Length == 0) throw new ArgumentException("Image source cannot be null or empty");
                _Source = value;
            }
        }

        /// <summary>
        /// An alternative description of the image.
        /// </summary>
        public string Alternative
        {
            get { return _Alternative; }
            set
            {
                if (value == null || value.Length == 0) throw new ArgumentException("Alternative description cannot be null or empty");
                _Alternative = value;
            }
        }

        /// <summary>
        /// The URL of a document containing additional information
        /// about this image.
        /// </summary>
        public string LongDesc
        {
            get { return _LongDesc; }
            set { _LongDesc = value; }
        }

        /// <summary>
        /// The map to use for this image.
        /// </summary>
        public string UseMap
        {
            get { return _UseMap; }
            set { _UseMap = value; }
        }

        /// <summary>
        /// Specify, whether this image should be threated as a map.
        /// </summary>
        /// <remarks>
        /// If you embed this image into a link, the browser will
        /// sent coordinates to the server with the coresponding request.
        /// </remarks>
        public bool IsMap
        {
            get { return _IsMap; }
            set { _IsMap = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("<img src=\"" + _Source + "\" alt=\"" + _Alternative + "\"" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
            if (_LongDesc != null && _LongDesc.Length > 0) b.Append(" longdesc=\"" + _LongDesc + "\"");
            if (_UseMap != null && _UseMap.Length > 0) b.Append(" usemap=\"" + _UseMap + "\"");
            if (_IsMap) b.Append(" ismap=\"ismap\"");
            if (IsXHtml) b.Append(" />"); else b.Append(">");
        }

        #endregion

    }

}
