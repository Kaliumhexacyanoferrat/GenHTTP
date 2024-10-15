﻿namespace GenHTTP.Modules.Reflection.Operations;

public enum OperationArgumentSource
{
    Path,
    Query,
    Body,
    Injected,
    Content
}

public sealed class OperationArgument
{

    #region Get-/Setters

    public string Name { get; }

    public Type Type { get; }

    public OperationArgumentSource Source { get; }

    #endregion

    #region Initialization

    public OperationArgument(string name, Type type, OperationArgumentSource source)
    {
        Name = name;
        Type = type;
        Source = source;
    }

    #endregion

}
