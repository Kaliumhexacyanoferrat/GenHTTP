using System.Runtime.Serialization;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers.Yaml;
using GenHTTP.Modules.Websockets;

// read and write complex objects as YAML instead of JSON
var serialization = new YamlFormat();

// only support GUIDs as primitive types
var formatters = Formatting.Empty()
                           .Add(new GuidFormatter())
                           .Build();

var websocket = Websocket.Reactive()
                         .Handler(new ChatHandler())
                         .Serialization(serialization)
                         .Formatters(formatters);
