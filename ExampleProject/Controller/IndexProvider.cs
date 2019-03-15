using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ExampleProject.View;

using GenHTTP;
using GenHTTP.Content;
using GenHTTP.Abstraction;
using GenHTTP.Abstraction.Style;
using GenHTTP.Abstraction.Elements;
using GenHTTP.SessionManagement;

namespace ExampleProject.Controller {
  
  /// <summary>
  /// Provides the index page to the clients.
  /// </summary>
  public class IndexProvider : ParametrizedContentProvider {
    private Project _Project;

    #region Constructors

    /// <summary>
    /// Create a new index provider.
    /// </summary>
    /// <param name="project">The related project</param>
    public IndexProvider(Project project) : base(project.Server) {
      _Project = project;
      Init();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// You can define parameters (required and optional) here.
    /// </summary>
    protected override void DefineParameters() {
      // This call would add the required parameter ID to the content provider
      // _Parameters.Add(new Parameter("ID", FormMethod.Get, true, "[0-9]{1,4}"));
    }

    /// <summary>
    /// If you decide to use rewrites (like Index-1.cs instead of Index.cs?ID=1)
    /// you can write an extraction rule
    /// </summary>
    protected override void DefineExtractingRule() {
      // _ExtractingRule = new Regex(@"/Index\-([0-9]{1,4})\.cs$");
      // _ExtractingNames.Add("ID");
    }

    /// <summary>
    /// Defines, which URL is assigned to the pages of this content provider.
    /// </summary>
    protected override void DefineMatchingRule() {
      _MatchingRule = new Regex(@"^/" + _Project.Name + @"/(?:Index\.cs|)$");
    }

    /// <summary>
    /// You can add the name of permissions here, which the user needs to
    /// display this page.
    /// </summary>
    protected override void DefineRequiredRights() {
      // _RequiredRights.Add("AddPost");
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Specify, whether the user needs to login to show this page.
    /// </summary>
    public override bool RequiresLogin {
      get { return false; }
    }

    #endregion

    #region Content generation

    /// <summary>
    /// In this method the content of the page will be generated.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="response">The response to write to</param>
    /// <param name="info">Information about the user's session</param>
    /// <param name="analysis">Parameter analysis (which are missing? etc.)</param>
    /// <param name="hasPermission">If this variable is set to true, the user owns all required permissions to display this page</param>
    protected override void GenerateContent(HttpRequest request, HttpResponse response, AuthorizationInfo info, ParameterAnalysis analysis, bool hasPermission) {
      // load a template to fill it
      ExampleTemplate template = _Project.GetTemplateInstance("main") as ExampleTemplate;

      // set some meta data
      template.Title = "Some great page";
      template.Headline = "Example project";

      // add some text to the body
      template.Value = new NeutralElement();
      template.Value.AddText("A simple website to demonstrate the GenHTTP framework.");

      // send the response to the client
      response.Send(template);
    }

    #endregion

  }

}
