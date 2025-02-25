using GenHTTP.Api.Protocol;
using System.Globalization;

namespace GenHTTP.Modules.I18n.Provider;

public sealed record AsyncOrSyncSetter(CultureSetterDelegate? SyncSetter = null, CultureSetterAsyncDelegate? AsyncSetter = null);

/// <summary>
/// Delegate to extract the cultures for a given request.
/// </summary>
/// <param name="request">The request to extract cultures for.</param>
/// <returns>An enumerable of CultureInfo objects representing the extracted cultures.</returns>
public delegate IAsyncEnumerable<CultureInfo> CultureSelectorCombinedAsyncDelegate(IRequest request);

/// <summary>
/// Delegate to extract the cultures for a given request.
/// </summary>
/// <param name="request">The request to extract cultures for.</param>
/// <returns>An enumerable of CultureInfo objects representing the extracted cultures.</returns>
public delegate ValueTask<IEnumerable<CultureInfo>> CultureSelectorAsyncDelegate(IRequest request);

/// <summary>
/// Delegate to set the culture for a given request.
/// </summary>
/// <param name="request">The request to set the culture for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be set.</param>
public delegate ValueTask CultureSetterAsyncDelegate(IRequest request, CultureInfo cultureInfo);

/// <summary>
/// Delegate to filter the cultures for a given request.
/// </summary>
/// <param name="request">The request to filter cultures for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be filtered.</param>
/// <returns>True if the culture is valid for the request, otherwise false.</returns>
public delegate ValueTask<bool> CultureFilterAsyncDelegate(IRequest request, CultureInfo cultureInfo);

/// <summary>
/// Delegate to extract the cultures for a given request.
/// </summary>
/// <param name="request">The request to extract cultures for.</param>
/// <returns>An enumerable of CultureInfo objects representing the extracted cultures.</returns>
public delegate IEnumerable<CultureInfo> CultureSelectorDelegate(IRequest request);

/// <summary>
/// Delegate to set the culture for a given request.
/// </summary>
/// <param name="request">The request to set the culture for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be set.</param>
public delegate void CultureSetterDelegate(IRequest request, CultureInfo cultureInfo);

/// <summary>
/// Delegate to filter the cultures for a given request.
/// </summary>
/// <param name="request">The request to filter cultures for.</param>
/// <param name="cultureInfo">The CultureInfo object representing the culture to be filtered.</param>
/// <returns>True if the culture is valid for the request, otherwise false.</returns>
public delegate bool CultureFilterDelegate(IRequest request, CultureInfo cultureInfo);
