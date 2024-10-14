﻿using GenHTTP.Api.Content;

using GenHTTP.Modules.Reflection;

using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Discovery;

public class MethodCollectionExplorer : IApiExplorer
{

    public bool CanExplore(IHandler handler) => handler is MethodCollection;

    public void Explore(IHandler handler, List<string> path, OpenApiDocument document, ApiDiscoveryRegistry registry)
    {
        if (handler is MethodCollection collection)
        {
            foreach (var method in collection.Methods)
            {
                registry.Explore(method, path, document);
            }
        }
    }

}
