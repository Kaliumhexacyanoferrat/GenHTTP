using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public interface IBuilder<T>
    {

        T Build();

    }

}
