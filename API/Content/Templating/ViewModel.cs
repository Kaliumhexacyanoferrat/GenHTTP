using GenHTTP.Api.Protocol;
using System.Collections;
using System.Threading.Tasks;

namespace GenHTTP.Api.Content.Templating
{

    public class ViewModel : ViewModel<object>
    {

        #region Initialization

        public ViewModel(IRequest request, IHandler handler, object data) : base(request, handler, data)
        {

        }

        public ViewModel(IRequest request, IHandler handler) : base(request, handler, null)
        {

        }

        #endregion

    }

    public class ViewModel<T> : AbstractModel where T : class
    {

        #region Get-/Setters

        public T? Data { get; }

        #endregion

        #region Initialization

        public ViewModel(IRequest request, IHandler handler, T? data) : base(request, handler)
        {
            Data = data;
        }

        #endregion

        #region Functionality

        public override ValueTask<ulong> CalculateChecksumAsync()
        {
            if (Data is IEnumerable enumerable)
            {
                unchecked
                {
                    ulong hash = 19;

                    foreach (var item in enumerable)
                    {
                        hash = hash * 31 + (ulong)item.GetHashCode();
                    }

                    return new(hash);
                }
            }

            return new((ulong)(Data?.GetHashCode() ?? 17));
        }

        #endregion

    }

}
