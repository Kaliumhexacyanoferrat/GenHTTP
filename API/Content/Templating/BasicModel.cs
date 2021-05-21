using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Simple page model with no further information than the mandatory request
    /// and handler values.
    /// </summary>
    public class BasicModel : AbstractModel
    {

        #region Initialization

        public BasicModel(IRequest request, IHandler handler) : base(request, handler)
        {

        }

        #endregion

        #region Functionality

        public override ValueTask<ulong> CalculateChecksumAsync() => new(17);

        #endregion

    }

}
