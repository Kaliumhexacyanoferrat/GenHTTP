/*

Updated: 2009/10/30

2009/10/30  Andreas Nägeli        Initial version of this file.


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
using System.Text.RegularExpressions;

using GenHTTP.SessionManagement;
using GenHTTP.Abstraction.Elements;

namespace GenHTTP.Content {
  
  /// <summary>
  /// Allows an inheriting class to define parameters for
  /// the provider.
  /// </summary>
  public abstract class ParametrizedContentProvider : IContentProvider {
    /// <summary>
    /// The parameters used by this script.
    /// </summary>
    protected List<Parameter> _Parameters;
    /// <summary>
    /// The extracting rule of this script.
    /// </summary>
    protected Regex _ExtractingRule;
    /// <summary>
    /// The variable names for the extraction of parameters.
    /// </summary>
    protected List<string> _ExtractingNames;
    /// <summary>
    /// The rule which determines, whether the content provider is responsible
    /// for a request.
    /// </summary>
    protected Regex _MatchingRule;
    /// <summary>
    /// Specifies, whether the content provider should display an error message,
    /// if a required parameter is missing.
    /// </summary>
    protected bool _AbortIfMissingRequiredParameters = true;
    /// <summary>
    /// Defines the rights an user needs to run this script.
    /// </summary>
    protected List<string> _RequiredRights;
    /// <summary>
    /// Specifies, whether the content provider should display an error message,
    /// if the user does not own a required permission.
    /// </summary>
    protected bool _AbortIfMissingRights = true;

    private Server _Server;

    #region Constructors

    /// <summary>
    /// Create a new parametrized content provider.
    /// </summary>
    public ParametrizedContentProvider(Server server) {
      _Parameters = new List<Parameter>();
      _RequiredRights = new List<string>();
      _Server = server;
    }

    /// <summary>
    /// Call this method to initialize the content provider.
    /// </summary>
    protected void Init() {
      DefineParameters();
      DefineExtractingRule();
      DefineMatchingRule();
      DefineRequiredRights();
    }

    /// <summary>
    /// Implement this method to define the parameters
    /// your script needs.
    /// </summary>
    protected abstract void DefineParameters();

    /// <summary>
    /// Implement this method to define an extracting rule,
    /// which allows you to convert parameters within the
    /// URL into GET parameters.
    /// </summary>
    protected abstract void DefineExtractingRule();

    /// <summary>
    /// Implement this method to define the responsiblity
    /// of your content provider by setting the _MatchingRule
    /// attribute.
    /// </summary>
    protected abstract void DefineMatchingRule();

    /// <summary>
    /// Implement this method to define the rights an user
    /// needs to own if he wants to run this script.
    /// </summary>
    protected abstract void DefineRequiredRights();

    #endregion

    #region IContentProvider Members

    /// <summary>
    /// Specifies, whether the content provider requires a login.
    /// </summary>
    public abstract bool RequiresLogin { get; }

    /// <summary>
    /// Specifies, whether the content provider is responsible for a given request.
    /// </summary>
    /// <param name="request">The request to prove</param>
    /// <param name="info">Information about the user's session</param>
    /// <returns>true, if the content provider is responsible for this request</returns>
    public bool IsResponsible(HttpRequest request, AuthorizationInfo info) {
      return _MatchingRule.IsMatch(request.File);
    }

    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="response">The response to write to</param>
    /// <param name="info">Information about the user's session</param>
    public void HandleRequest(HttpRequest request, HttpResponse response, AuthorizationInfo info) {
      // extract parameters
      if (_ExtractingRule != null) {
        Match m = _ExtractingRule.Match(request.File);
        int i = 0;
        foreach (string name in _ExtractingNames) {
          request.GetFields.Add(name, m.Groups[++i].Value);
        }
      }
      // analyze parameters
      ParameterAnalysis analysis = new ParameterAnalysis();
      foreach (Parameter param in _Parameters) {
        if (param.Method == FormMethod.Get) {
          if (request.GetFields.ContainsKey(param.Name)) {
            param.Value = request.GetFields[param.Name];
            analysis.AddAvailable(param);
          }
          else analysis.AddMissing(param);
        }
        else {
          if (request.PostFields.ContainsKey(param.Name)) {
            param.Value = request.PostFields[param.Name];
            analysis.AddAvailable(param);
          }
          else analysis.AddMissing(param);
        }
      }
      // abort?
      if (_AbortIfMissingRequiredParameters && analysis.RequiredMissingCount > 0) {
        new DefaultWrongParametersProvider(_Server).HandleRequest(request, response, info);
        return;
      }
      // check, whether all needed rights are available
      bool hasRights = true;
      if (info != null) {
        foreach (string right in _RequiredRights) {
          if (!info.User.HasPermission(right)) {
            hasRights = false;
            break;
          }
        }
      }
      // abort?
      if (_AbortIfMissingRights && !hasRights) {
        new DefaultNotEnoughRightsProvider(_Server).HandleRequest(request, response, info);
        return;
      }
      // let the inheriting class generate content
      GenerateContent(request, response, info, analysis, hasRights);
    }

    /// <summary>
    /// Implement this method to generate content.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="response">The response to write to</param>
    /// <param name="info">Information about the user's session</param>
    /// <param name="analysis">A parameter analysis for this request</param>
    /// <param name="hasPermission">Specifies, whether the user has all required permissions for this script</param>
    protected abstract void GenerateContent(HttpRequest request, HttpResponse response, AuthorizationInfo info, ParameterAnalysis analysis, bool hasPermission);

    #endregion

  }

}
