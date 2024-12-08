using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n;

/// <summary>
/// Delegate to select the cultures for a given request.
/// </summary>
/// <param name="request">The request to select cultures for.</param>
/// <returns>An enumerable of CultureInfo objects representing the selected cultures.</returns>
public delegate IEnumerable<CultureInfo> CultureSelector_Delegate(IRequest request);

/// <summary>
/// Delegate to set the culture for a given request.
/// </summary>
/// <param name="request">The request to set the culture for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be set.</param>
public delegate void CultureSetter_Delegate(IRequest request, CultureInfo cultureInfo);

/// <summary>
/// Delegate to filter the cultures for a given request.
/// </summary>
/// <param name="request">The request to filter cultures for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be filtered.</param>
/// <returns>True if the culture is valid for the request, otherwise false.</returns>
public delegate bool CultureFilter_Delegate(IRequest request, CultureInfo cultureInfo);
