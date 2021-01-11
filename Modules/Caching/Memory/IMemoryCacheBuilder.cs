using GenHTTP.Api.Content.Caching;
using GenHTTP.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Caching.Memory
{
    
    public interface IMemoryCacheBuilder<T> : IBuilder<ICache<T>>
    {


    }

}
