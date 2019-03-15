/*

Updated: 2009/10/15

2009/10/15  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.DublinCore {
  
  /// <summary>
  /// Allows you to describe you document, using the
  /// DublinCore scheme (<see href="http://dublincore.org/documents/dcmi-terms/">DCMI Metadata Terms</see>).
  /// </summary>
  /// <remarks>
  /// This class does not support all available attributes
  /// and variations of the scheme. If you want to add
  /// some of the missing attributes, you need to extend this class
  /// or add the meta information manually.
  /// </remarks>
  public class DublinCore {
    private DublinElement _Title;
    private DublinElement _Subject;
    private DublinElement _Description;
    private string _Creator;
    private string _Publisher;
    private string _Coverage;
    private DateTime _Date;
    private DublinCoreDocumentType _DocumentType = DublinCoreDocumentType.Unspecified;
    private DublinReference _Identifier;
    private DublinReference _Source;
    private DublinReference _Relation;
    private DublinReference _Rights;
    private LanguageInfo _Language;
    private List<string> _Contributors;

    #region Constructors

    /// <summary>
    /// Create a new object containing meta information.
    /// </summary>
    public DublinCore() {
      _Contributors = new List<string>();
      _Title = new DublinElement("DC.title");
      _Subject = new DublinElement("DC.subject");
      _Description = new DublinElement("DC.description");
      _Identifier = new DublinReference("DC.identifier");
      _Source = new DublinReference("DC.source");
      _Relation = new DublinReference("DC.relation");
      _Rights = new DublinReference("DC.rights");
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// A name given to the resource.
    /// </summary>
    public DublinElement Title {
      get { return _Title; }
    }

    /// <summary>
    /// The topic of the resource.
    /// </summary>
    /// <remarks>
    /// Typically, the subject will be represented using keywords, key phrases, or 
    /// classification codes. Recommended best practice is to use a controlled 
    /// vocabulary. To describe the spatial or temporal topic of the 
    /// resource, use the Coverage element.
    /// </remarks>
    public DublinElement Subject {
      get { return _Subject; }
    }

    /// <summary>
    /// An account of the resource.
    /// </summary>
    /// <remarks>
    /// Description may include but is not limited to: an abstract, a table
    /// of contents, a graphical representation, or a free-text account of the resource.
    /// </remarks>
    public DublinElement Description {
      get { return _Description; }
    }

    /// <summary>
    /// An entity primarily responsible for making the resource.
    /// </summary>
    /// <remarks>
    /// Examples of a Creator include a person, an organization, or a service.
    /// Typically, the name of a Creator should be used to indicate the entity.
    /// </remarks>
    public string Creator {
      get { return _Creator; }
      set { _Creator = value; }
    }

    /// <summary>
    /// An entity responsible for making the resource available.
    /// </summary>
    /// <remarks>
    /// Examples of a Publisher include a person, an organization, or a service.
    /// Typically, the name of a Publisher should be used to indicate the entity.
    /// </remarks>
    public string Publisher {
      get { return _Publisher; }
      set { _Publisher = value; }
    }

    /// <summary>
    /// The spatial or temporal topic of the resource, the spatial applicability
    /// of the resource, or the jurisdiction under which the resource is relevant.
    /// </summary>
    /// <remarks>
    /// Spatial topic and spatial applicability may be a named place or a location 
    /// specified by its geographic coordinates. Temporal topic may be a named period, 
    /// date, or date range. A jurisdiction may be a named administrative entity or 
    /// a geographic place to which the resource applies. Recommended best practice 
    /// is to use a controlled vocabulary such as the Thesaurus of Geographic Names [TGN].
    /// Where appropriate, named places or time periods can be used in preference 
    /// to numeric identifiers such as sets of coordinates or date ranges.
    /// </remarks>
    public string Coverage {
      get { return _Coverage; }
      set { _Coverage = value; }
    }

    /// <summary>
    /// A point or period of time associated with an event in the lifecycle of the resource.
    /// </summary>
    public DateTime Date {
      get { return _Date; }
      set { _Date = value; }
    }

    /// <summary>
    /// The nature or genre of the resource.
    /// </summary>
    public DublinCoreDocumentType DocumentType {
      get { return _DocumentType; }
      set { _DocumentType = value; }
    }

    /// <summary>
    /// An unambiguous reference to the resource within a given context.
    /// </summary>
    public DublinReference Identifier {
      get { return _Identifier; }
    }

    /// <summary>
    /// A related resource from which the described resource is derived.
    /// </summary>
    public DublinReference Source {
      get { return _Source; }
    }

    /// <summary>
    /// A related resource.
    /// </summary>
    public DublinReference Relation {
      get { return _Relation; }
    }

    /// <summary>
    /// Information about rights held in and over the resource.
    /// </summary>
    public DublinReference Rights {
      get { return _Rights; }
    }

    /// <summary>
    /// A language of the resource.
    /// </summary>
    public LanguageInfo Language {
      get { return _Language; }
      set { _Language = value; }
    }

    #endregion

    #region Contributor handling

    /// <summary>
    /// Add a contributing entity.
    /// </summary>
    /// <param name="name">The name of the entity</param>
    public void AddContributor(string name) {
      if (!_Contributors.Contains(name)) _Contributors.Add(name);
    }

    /// <summary>
    /// Remove a contributing entity.
    /// </summary>
    /// <param name="name">The name of the entity</param>
    public void RemoveContributor(string name) {
      if (_Contributors.Contains(name)) _Contributors.Remove(name);
    }

    /// <summary>
    /// The number of contributing entities.
    /// </summary>
    public int ContributorCount {
      get { return _Contributors.Count; }
    }

    /// <summary>
    /// Retrieve an enumerator to enumerate over the contributors.
    /// </summary>
    /// <returns>The requested enumerator</returns>
    public IEnumerator<string> GetEnumerator() {
      return _Contributors.GetEnumerator();
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize the information in this class to a document.
    /// </summary>
    /// <param name="doc">The document to write to</param>
    internal void Serialize(Document doc) {
      // serialize elements
      _Title.Serialize(doc);
      _Subject.Serialize(doc);
      _Description.Serialize(doc);
      // add string elements
      if (_Creator != null) doc.Header.AdditionalMetaInformation.Add("DC.creator", _Creator);
      if (_Publisher != null) doc.Header.AdditionalMetaInformation.Add("DC.publisher", _Publisher);
      if (_Coverage != null) doc.Header.AdditionalMetaInformation.Add("DC.coverage", _Coverage);
      // add datetime
      if (_Date != null) doc.Header.AdditionalMetaInformation.Add(new DocumentMetaInformation("DC.date", "DCTERMS.W3CDTF", _Date.Year + "-" + LeadingZero(_Date.Month) + "-" + LeadingZero(_Date.Day)));
      // write type
      if (_DocumentType != DublinCoreDocumentType.Unspecified) doc.Header.AdditionalMetaInformation.Add(new DocumentMetaInformation("DC.type", "DCTERMS.DCMIType", _DocumentType.ToString()));
      // write references
      _Identifier.Serialize(doc);
      _Source.Serialize(doc);
      _Relation.Serialize(doc);
      _Rights.Serialize(doc);
      // write language information
      if (_Language != null) {
        doc.Header.AdditionalMetaInformation.Add(new DocumentMetaInformation("DC.language", "DCTERMS.RFC3066", _Language.LanguageString));
      }
      // write all contributors
      foreach (string contrib in _Contributors) {
        doc.Header.AdditionalMetaInformation.Add(new DocumentMetaInformation("DC.contributor", contrib));
      }
      // set profile
      doc.Header.Profile = "http://dublincore.org/documents/dcq-html";
      // link schemes
      DocumentLink dc = new DocumentLink();
      dc.Rel = LinkType.Other;
      dc.UserDefinedRel = "schema.DC";
      dc.Href = "http://purl.org/dc/elements/1.1/";
      DocumentLink dcTerms = new DocumentLink();
      dcTerms.Rel = LinkType.Other;
      dcTerms.UserDefinedRel = "schema.DCTERMS";
      dcTerms.Href = "http://purl.org/dc/terms/";
      doc.Header.AddLink(dc);
      doc.Header.AddLink(dcTerms);
    }

    private string LeadingZero(int number) {
      if (number < 10) return "0" + number;
      return number.ToString();
    }

    #endregion

  }

}
