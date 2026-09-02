// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// Controls whether runtime-generated async code is used for an async method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class RuntimeAsyncMethodGenerationAttribute : Attribute
{
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeAsyncMethodGenerationAttribute"/> class.
    /// </summary>
    /// <param name="enabled">
    /// <c>true</c> to enable runtime async generation for the attributed method;
    /// <c>false</c> to disable it.
    /// </param>
    public RuntimeAsyncMethodGenerationAttribute(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Gets whether runtime async generation is enabled.
    /// </summary>
    public bool Enabled { get; }
    
}
