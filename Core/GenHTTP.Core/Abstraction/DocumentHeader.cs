/*

Updated: 2009/10/12

2009/10/12  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;

using GenHTTP.Abstraction.DublinCore;

namespace GenHTTP.Abstraction {

  /// <summary>
  /// The document header contains meta information about
  /// a <see cref="Document" />, such as the author, the description
  /// or the title of the document.
  /// </summary>
  /// <remarks>
  /// The document header abstraction does not provide all
  /// features of the W3C standards:
  /// 
  /// - You can't internationalize the document header
  /// - You can't internationalize the <see cref="Title" /> field
  /// - You can't give an ID to the document header
  /// - You can't give an ID to the title field
  /// - You can't give an ID to the base field
  /// </remarks>
  public class DocumentHeader {
    private Document _Document;
    private string _Profile;
    private string _Title;
    private string _Base;
    private string _Description;
    private string _Author;
    private string _Favicon;
    private bool _GeneratorInfo;
    private bool _ForceEncodingHttpEquiv;
    private DocumentScriptCollection _Scripts;
    private DocumentMetaInformationCollection _MetaInformation;
    private Dictionary<string, DocumentKeywordCollection> _Keywords;
    private HashSet<DocumentLink> _Links;
    private DublinCore.DublinCore _DublinCore;

    #region Constructors

    /// <summary>
    /// Create a new document header.
    /// </summary>
    /// <param name="document"></param>
    internal DocumentHeader(Document document) {
      _Document = document;
      _MetaInformation = new DocumentMetaInformationCollection();
      _Links = new HashSet<DocumentLink>();
      _Keywords = new Dictionary<string, DocumentKeywordCollection>();
      _Keywords.Add("", new DocumentKeywordCollection());
      _Scripts = new DocumentScriptCollection();
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Set or get the title of this document.
    /// </summary>
    public string Title {
      get { return _Title; }
      set { _Title = value; }
    }

    /// <summary>
    /// The description of this document.
    /// </summary>
    public string Description {
      get { return _Description; }
      set { _Description = value; }
    }

    /// <summary>
    /// The author of this document.
    /// </summary>
    public string Author {
      get { return _Author; }
      set { _Author = value; }
    }

    /// <summary>
    /// The profile used for this document.
    /// </summary>
    public string Profile {
      get { return _Profile; }
      set { _Profile = value; }
    }

    /// <summary>
    /// Document base URI.
    /// </summary>
    public string Base {
      get { return _Base; }
      set { _Base = value; }
    }

    /// <summary>
    /// Additional information about this document.
    /// </summary>
    public DocumentMetaInformationCollection AdditionalMetaInformation {
      get { return _MetaInformation; }
    }

    /// <summary>
    /// Retrieve the keywords of this document.
    /// </summary>
    public DocumentKeywordCollection Keywords {
      get { return _Keywords[""]; }
    }

    /// <summary>
    /// Define keywords for different languages.
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    public DocumentKeywordCollection this[LanguageInfo language] {
      get {
        if (language == null) throw new ArgumentNullException();
        string name = language.LanguageString;
        if (_Keywords.ContainsKey(name)) return _Keywords[name];
        DocumentKeywordCollection collection = new DocumentKeywordCollection();
        _Keywords.Add(name, collection);
        return collection;
      }
    }

    /// <summary>
    /// Specify, whether the 'generator' tag should be
    /// added to the meta information. 
    /// </summary>
    public bool GeneratorInfo {
      get { return _GeneratorInfo; }
      set { _GeneratorInfo = value; }
    }

    /// <summary>
    /// Get or set the meta information following the
    /// Dublin Core Metadata Initiative.
    /// </summary>
    /// <remarks>
    /// This property is set to null by default.
    /// </remarks>
    public DublinCore.DublinCore DublinCore {
      get { return _DublinCore; }
      set { _DublinCore = value; }
    }

    /// <summary>
    /// Set the URL to a icon which should be used
    /// for the favourites of the user.
    /// </summary>
    /// <remarks>
    /// The document generator will select the content
    /// type of the icon automatically.
    /// </remarks>
    public string Favicon {
      get { return _Favicon; }
      set { _Favicon = value; }
    }

    /// <summary>
    /// Define, whether the content-type HTTP equivalent
    /// should be sent to the client.
    /// </summary>
    public bool ForceEncodingHttpEquiv {
      get { return _ForceEncodingHttpEquiv; }
      set { _ForceEncodingHttpEquiv = value; }
    }

    /// <summary>
    /// The script data of this document.
    /// </summary>
    public DocumentScriptCollection Scripts {
      get { return _Scripts; }
    }

    #endregion

    #region Link collection

    /// <summary>
    /// Add a new link to this document.
    /// </summary>
    /// <param name="link">The link to add</param>
    public void AddLink(DocumentLink link) {
      _Links.Add(link);
    }

    /// <summary>
    /// Remove a link from the document.
    /// </summary>
    /// <param name="link">The link to remove</param>
    public void RemoveLink(DocumentLink link) {
      _Links.Remove(link);
    }

    /// <summary>
    /// Retrieve the number of links.
    /// </summary>
    public int LinkCount {
      get { return _Links.Count; }
    }

    /// <summary>
    /// Retrieve an enumerator to iterate over all links of this document.
    /// </summary>
    /// <returns>The requested enumerator</returns>
    public IEnumerator<DocumentLink> GetEnumerator() {
      return _Links.GetEnumerator();
    }

    #endregion

    #region CSS

    /// <summary>
    /// Add an external stylesheet to this document.
    /// </summary>
    /// <param name="url">The URL of the stylesheet</param>
    public void AddStylesheet(string url) {
      _Links.Add(new DocumentLink(LinkType.Stylesheet, ContentType.TextCss, url));
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize the document header.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    internal void Serialize(StringBuilder b) {
      // some constants
      string nl = "\r\n";
      string tagEnd = (_Document.IsXHtml) ? " />" : ">";
      // write head tag
      b.Append("<head");
      // print encoding info before all other meta information (if wanted)
      if (_ForceEncodingHttpEquiv) _MetaInformation.Add(new DocumentMetaInformation("content-type", DocumentMetaInformationType.HttpEquivalent, "text/html; charset=" + _Document.Encoding.Name));
      // add dublin-core info if required
      if (_DublinCore != null) {
        _DublinCore.Serialize(_Document);
      }
      // profile
      if (_Profile != null && _Profile.Length > 0) b.Append(" profile=\"" + _Profile + "\"");
      b.Append(">" + nl);
      // write title
      if (_Title == null) throw new DocumentGeneratorException("The 'head' tag needs to include the 'title' tag");
      b.Append("  <title>" + DocumentEncoding.ConvertString(_Title) + "</title>" + nl);
      // write base
      if (_Base != null && _Base.Length > 0) b.Append("  <base href=\"" + _Base + "\"" + tagEnd + nl);
      // add favicon
      if (_Favicon != null && _Favicon.Length > 0) {
        DocumentLink fav = new DocumentLink();
        fav.Rel = LinkType.Other;
        fav.UserDefinedRel = "icon";
        fav.Type = HttpResponseHeader.GetContentTypeByExtension(_Favicon.Substring(_Favicon.Length-3));
        fav.Href = _Favicon;
        _Links.Add(fav);
      }
      // write links
      foreach (DocumentLink link in _Links) link.Serialize(b, _Document);
      // write keywords
      foreach (string language in _Keywords.Keys) {
        DocumentKeywordCollection keywords = _Keywords[language];
        if (keywords.Count > 0) {
          DocumentMetaInformation meta = keywords.ToMetaInformation();
          if (language != "") meta.Internationalization.Language = new LanguageInfo(language);
          _MetaInformation.Add(meta);
        }
      }
      // some additional meta information
      if (_Description != null && _Description.Length > 0) _MetaInformation.Add("description", _Description);
      if (_Author != null && _Author.Length > 0) _MetaInformation.Add("author", _Author);
      if (_GeneratorInfo) _MetaInformation.Add("generator", "GenHTTP Document Library");
      // serialize meta information
      _MetaInformation.Serialize(b, _Document);
      // serialize scripts
      _Scripts.Serialize(b, _Document);
      // close head section
      b.Append("</head>" + nl + nl);
    }

    #endregion

  }

}
