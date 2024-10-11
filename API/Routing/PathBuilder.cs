using System;
using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Routing;

/// <summary>
/// Allows to build a <c cref="WebPath" /> instance.
/// </summary>
public sealed class PathBuilder : IBuilder<WebPath>
{
    private readonly List<WebPathPart> _Segments;

    private bool _TrailingSlash;

    #region Get-/Setters

    /// <summary>
    /// True, if no segments have (yet) been added to
    /// this path.
    /// </summary>
    public bool IsEmpty => _Segments.Count == 0;

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new, empty path builder.
    /// </summary>
    /// <param name="trailingSlash">Whether the resulting path should end with a slash</param>
    public PathBuilder(bool trailingSlash)
    {
            _Segments = new();
            _TrailingSlash = trailingSlash;
        }

    /// <summary>
    /// Creates a new path builder with the given segments.
    /// </summary>
    /// <param name="parts">The segments of the path</param>
    /// <param name="trailingSlash">Whether the resulting path should end with a slash</param>
    public PathBuilder(IEnumerable<WebPathPart> parts, bool trailingSlash)
    {
            _Segments = new(parts);
            _TrailingSlash = trailingSlash;
        }

    /// <summary>
    /// Creates a new path builder from the given absolute
    /// or relative path.
    /// </summary>
    /// <param name="path">The path to be parsed</param>
    public PathBuilder(string path)
    {
            var splitted = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var parts = new List<WebPathPart>(splitted.Length);

            foreach (var entry in splitted)
            {
                parts.Add(new WebPathPart(entry));
            }

            _Segments = parts;
            _TrailingSlash = path.EndsWith('/');
        }

    #endregion

    #region Functionality

    /// <summary>
    /// Adds the given segment to the beginning of the resulting path.
    /// </summary>
    /// <param name="segment">The segment to be prepended</param>
    public PathBuilder Preprend(string segment)
    {
            _Segments.Insert(0, new WebPathPart(segment));
            return this;
        }
        
    /// <summary>
    /// Adds the given part to the beginning of the resulting path.
    /// </summary>
    /// <param name="path">The part to be prepended</param>
    public PathBuilder Preprend(WebPathPart part)
    {
            _Segments.Insert(0, part);
            return this;
        }

    /// <summary>
    /// Adds the given path to the beginning of the resulting path.
    /// </summary>
    /// <param name="path">The path to be prepended</param>
    public PathBuilder Preprend(WebPath path)
    {
            _Segments.InsertRange(0, path.Parts);
            return this;
        }

    /// <summary>
    /// Adds the given segment to the end of the resulting path.
    /// </summary>
    /// <param name="segment">The segment to be appended</param>
    public PathBuilder Append(string segment)
    {
            _Segments.Add(new WebPathPart(segment));
            return this;
        }

    /// <summary>
    /// Adds the given segment to the end of the resulting path.
    /// </summary>
    /// <param name="segment">The segment to be appended</param>
    public PathBuilder Append(WebPathPart segment)
    {
            _Segments.Add(segment);
            return this;
        }

    /// <summary>
    /// Adds the given path to the end of the resulting path.
    /// </summary>
    /// <param name="path">The path to be appended</param>
    public PathBuilder Append(WebPath path)
    {
            _Segments.AddRange(path.Parts);
            return this;
        }

    /// <summary>
    /// Specifies, whether the resulting path ends with a slash or not.
    /// </summary>
    /// <param name="existent">True, if the path should end with a trailing slash</param>
    public PathBuilder TrailingSlash(bool existent)
    {
            _TrailingSlash = existent;
            return this;
        }

    public WebPath Build() => new(_Segments, _TrailingSlash);

    #endregion 

}
