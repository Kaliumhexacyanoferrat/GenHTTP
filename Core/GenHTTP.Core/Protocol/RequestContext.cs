using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Core.Protocol
{

    internal class RequestContext : IDisposable
    {

        #region Get-/Setters

        internal RequestBuffer Buffer { get; }

        internal Scanner Scanner { get; }

        internal RequestBuilder Request { get; }

        #endregion

        #region Initialization

        public RequestContext()
        {
            Buffer = new RequestBuffer();
            Scanner = new Scanner(Buffer);
            Request = new RequestBuilder();
        }

        public RequestContext(RequestBuffer buffer)
        {
            Buffer = buffer;
            Scanner = new Scanner(buffer);
            Request = new RequestBuilder();
        }
            
        #endregion

        #region Functionality

        public async Task<RequestContext> GetNext()
        {
            var next = new RequestContext(await Buffer.GetNext());
            
            Dispose();

            return next;
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Buffer.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
