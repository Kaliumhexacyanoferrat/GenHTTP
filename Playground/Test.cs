

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;

var i = Inline.Create().Get((IRequest r) =>
{
   r.Respond().Content(new byte[] { 1, 2, 3 });
});
