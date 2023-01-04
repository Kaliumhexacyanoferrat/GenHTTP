﻿using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Providers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Protobuf.Providers
{
    public sealed class ProtobufFormat : ISerializationFormat
    {
        public ValueTask<object?> DeserializeAsync(Stream stream, [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] Type type)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
        {
            var result = request.Respond()
              .Content(new ProtobufContent(response))
              .Type(ContentType.ApplicationProtobuf);

            return new ValueTask<IResponseBuilder>(result);
        }
    }
}
