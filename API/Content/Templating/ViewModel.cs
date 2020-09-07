using GenHTTP.Api.Protocol;

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

    public class ViewModel<T> : PageModel where T : class
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

    }

}
