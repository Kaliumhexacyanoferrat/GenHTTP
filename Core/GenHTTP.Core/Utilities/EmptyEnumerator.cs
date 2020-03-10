using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Core.Utilities
{

    internal class EmptyEnumerator : IEnumerator
    {

        internal static readonly EmptyEnumerator Instance = new EmptyEnumerator();

        #region Get-/Setters

        public object Current => throw new InvalidOperationException();

        #endregion

        #region Initialization

        private EmptyEnumerator() { }

        #endregion

        #region Functionality

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {

        }

        #endregion

    }

    public class EmptyEnumerator<T> : IEnumerator<T>
    {

        internal static readonly EmptyEnumerator<T> Instance = new EmptyEnumerator<T>();

        #region Get-/Setters

        public T Current => throw new InvalidOperationException();

        object IEnumerator.Current => throw new InvalidOperationException();

        #endregion

        #region Initialization

        private EmptyEnumerator() { }

        #endregion

        #region Functionality

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {

        }

        #endregion

    }

}
