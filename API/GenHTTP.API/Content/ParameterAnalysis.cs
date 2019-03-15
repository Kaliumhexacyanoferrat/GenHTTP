using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Allows a script to determine, which parameters are missing.
    /// </summary>
    public class ParameterAnalysis
    {
        private List<Parameter> _Missing;
        private List<Parameter> _Available;

        #region Constructors

        internal ParameterAnalysis()
        {
            _Missing = new List<Parameter>();
            _Available = new List<Parameter>();
        }

        #endregion

        #region add methods

        internal void AddMissing(Parameter p) { _Missing.Add(p); }

        internal void AddAvailable(Parameter p) { _Available.Add(p); }

        #endregion

        #region getters

        /// <summary>
        /// Retrieve all missing parameters.
        /// </summary>
        public Collection<Parameter> Missing
        {
            get { return new Collection<Parameter>(_Missing); }
        }

        /// <summary>
        /// The number of missing parameters.
        /// </summary>
        public int MissingCount
        {
            get { return _Missing.Count; }
        }

        /// <summary>
        /// Retrieve all missing and required parameters.
        /// </summary>
        public Collection<Parameter> RequiredMissing
        {
            get
            {
                return new Collection<Parameter>(new List<Parameter>(_Missing.Where((Parameter p) => p.Required)));
            }
        }

        /// <summary>
        /// The number of missing, required parameters.
        /// </summary>
        public int RequiredMissingCount
        {
            get { return RequiredMissing.Count; }
        }

        /// <summary>
        /// Retrieve all available parameters which were expected
        /// by the script and are therefor not additional.
        /// </summary>
        public Collection<Parameter> Available
        {
            get { return new Collection<Parameter>(_Available); }
        }

        /// <summary>
        /// Checks, whether a given parameter is missing.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <returns>true, if the parameter is missing</returns>
        public bool IsMissing(string name)
        {
            foreach (Parameter par in _Missing) if (par.Name == name) return true;
            return false;
        }

        /// <summary>
        /// Retrive a parameter by its name.
        /// </summary>
        /// <param name="name">The name of the parameter to retrieve</param>
        /// <returns>The requested parameter</returns>
        public Parameter GetParameter(string name)
        {
            foreach (Parameter par in _Available) if (par.Name == name) return par;
            foreach (Parameter par in _Missing) if (par.Name == name) return par;
            return null;
        }

        /// <summary>
        /// Retrieve the value of a parameter.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="name">The name of the parameter</param>
        /// <param name="def">The default value to return if the conversion fails</param>
        /// <returns>The requested value</returns>
        public T GetParameterValue<T>(string name, T def)
        {
            Parameter par = GetParameter(name);
            if (par == null) return def;
            return par.ConvertTo<T>(def);
        }

        #endregion

    }

}
