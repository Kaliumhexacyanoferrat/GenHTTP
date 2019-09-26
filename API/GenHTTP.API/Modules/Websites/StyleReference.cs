﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Modules.Websites
{

    public class StyleReference
    {

        #region Get-/Setters

        public string Path { get; }
        
        #endregion

        #region Functionality

        public StyleReference(string path)
        {
            Path = path;
        }

        #endregion

    }

}
