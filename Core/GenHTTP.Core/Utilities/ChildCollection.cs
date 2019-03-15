using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GenHTTP.Utilities
{

    /// <summary>
    /// Contains the children of a Setting entry.
    /// </summary>
    public class ChildCollection
    {
        private Dictionary<string, List<Setting>> _Children;

        /// <summary>
        /// Create a new ChildCollection.
        /// </summary>
        internal ChildCollection()
        {
            _Children = new Dictionary<string, List<Setting>>();
        }

        /// <summary>
        /// Add a children to this element.
        /// </summary>
        /// <param name="setting">The setting to add</param>
        public void Add(Setting setting)
        {
            if (!_Children.ContainsKey(setting.Name))
                _Children.Add(setting.Name, new List<Setting>());
            _Children[setting.Name].Add(setting);
        }

        /// <summary>
        /// Remove all children with the given name.
        /// </summary>
        /// <param name="name">The name of the settings to remove</param>
        public void Remove(string name)
        {
            if (_Children.ContainsKey(name)) _Children.Remove(name);
        }

        /// <summary>
        /// Remove the first child matching the specified rule.
        /// </summary>
        /// <param name="name">The name of the element to find</param>
        /// <param name="attribName">The name of the attribute to compare with</param>
        /// <param name="attribValue">The value of the attribute to compare with</param>
        public void Remove(string name, string attribName, string attribValue)
        {
            if (_Children.ContainsKey(name))
            {
                for (int i = 0; i < _Children[name].Count; i++)
                {
                    if (_Children[name][i].Attributes[attribName] == attribValue)
                    {
                        _Children[name].RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve a child by its name.
        /// </summary>
        /// <param name="name">The name of the child to retrieve</param>
        /// <returns>The requested child</returns>
        /// <remarks>
        /// There will be returned only the first setting. If you want to
        /// retrieve all settings with this name, use the this[name, maxCount]
        /// indexer.
        /// </remarks>
        public Setting this[string name]
        {
            get
            {
                if (_Children.ContainsKey(name)) return _Children[name][0];
                Setting newSetting = new Setting(name);
                Add(newSetting);
                return newSetting;
            }
        }

        /// <summary>
        /// Retrieve all children with the given name.
        /// </summary>
        /// <param name="name">The name of the children</param>
        /// <param name="maxCount">The maximum count of settings to retrieve</param>
        /// <returns>The requested children</returns>
        public Collection<Setting> this[string name, int maxCount]
        {
            get
            {
                List<Setting> retVal = new List<Setting>();
                if (!_Children.ContainsKey(name)) return new Collection<Setting>(retVal);
                for (int i = 0; i < _Children[name].Count; i++)
                {
                    if (i + 1 > maxCount && maxCount != 0) break;
                    retVal.Add(_Children[name][i]);
                }
                return new Collection<Setting>(retVal);
            }
        }

        /// <summary>
        /// Retrieve a specific child.
        /// </summary>
        /// <param name="name">The name of the children to retrive</param>
        /// <param name="attribName">The name of the attribute to compare with</param>
        /// <param name="attribValue">The value of the attribute to compare with</param>
        /// <returns>The specified setting</returns>
        public Setting this[string name, string attribName, string attribValue]
        {
            get
            {
                if (_Children.ContainsKey(name))
                {
                    foreach (Setting child in _Children[name])
                    {
                        if (child.Attributes[attribName] == attribValue) return child;
                    }
                }
                Setting newSetting = new Setting(name);
                newSetting.Attributes[attribName] = attribValue;
                Add(newSetting);
                return newSetting;
            }
        }

        /// <summary>
        /// The number of children in this collection.
        /// </summary>
        public int Count
        {
            get
            {
                int retVal = 0;
                foreach (List<Setting> list in _Children.Values)
                {
                    foreach (Setting setting in list)
                    {
                        retVal++;
                    }
                }
                return retVal;
            }
        }

        /// <summary>
        /// Retrieve the enumerator to iterate over the children list.
        /// </summary>
        /// <returns>The enumerator for this collection</returns>
        public IEnumerator<Setting> GetEnumerator()
        {
            List<Setting> settings = new List<Setting>();
            foreach (List<Setting> list in _Children.Values)
            {
                foreach (Setting setting in list)
                {
                    settings.Add(setting);
                }
            }
            return settings.GetEnumerator();
        }

    }

}
