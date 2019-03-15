using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Compilation
{

    /// <summary>
    /// Stores template bases for a project.
    /// </summary>
    public class TemplateCollection
    {
        private Dictionary<string, ITemplateBase> _Templates;

        #region Constructors

        internal TemplateCollection()
        {
            _Templates = new Dictionary<string, ITemplateBase>();
        }

        #endregion

        #region Collection handling

        /// <summary>
        /// Add a template base to this collection.
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <param name="template">The template to add</param>
        public void Add(string name, ITemplateBase template)
        {
            if (_Templates.ContainsKey(name)) _Templates[name] = template;
            else _Templates.Add(name, template);
        }

        /// <summary>
        /// Remove a template base from this collection.
        /// </summary>
        /// <param name="name">The name of the template to remove</param>
        public void Remove(string name)
        {
            if (_Templates.ContainsKey(name)) _Templates.Remove(name);
        }

        /// <summary>
        /// The number of templates in this collection.
        /// </summary>
        public int Count
        {
            get { return _Templates.Count; }
        }

        /// <summary>
        /// Retrieve an enumerator to iterate over all templates.
        /// </summary>
        /// <returns>The enumerator for this collection</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _Templates.Keys.GetEnumerator();
        }

        /// <summary>
        /// Retrieve a template base.
        /// </summary>
        /// <param name="name">The name of the template to retrieve</param>
        /// <returns>The requested template base</returns>
        public ITemplateBase this[string name]
        {
            get
            {
                if (_Templates.ContainsKey(name)) return _Templates[name];
                return null;
            }
        }

        #endregion

    }

}
