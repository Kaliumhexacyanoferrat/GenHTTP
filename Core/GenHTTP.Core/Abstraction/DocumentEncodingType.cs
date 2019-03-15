/*

Updated: 2009/10/12

2009/10/12  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction {

  /// <summary>
  /// The encoding of the web page.
  /// </summary>
  public enum DocumentEncodingType {
    /// <summary>
    /// Unicode transformation format
    /// </summary>
    utf_8,
    /// <summary>
    /// Unicode transformation format
    /// </summary>
    utf_16,
    /// <summary>
    /// Western Europe
    /// </summary>
    iso_8859_1,
    /// <summary>
    /// Western and Central Europe
    /// </summary>
    iso_8859_2,
    /// <summary>
    /// Western Europe and South European
    /// </summary>
    iso_8859_3,
    /// <summary>
    /// Western Europe and Baltic Countries
    /// </summary>
    iso_8859_4,
    /// <summary>
    /// Cyrillic alphabet
    /// </summary>
    iso_8859_5,
    /// <summary>
    /// Arabic
    /// </summary>
    iso_8859_6,
    /// <summary>
    /// Greek
    /// </summary>
    iso_8859_7,
    /// <summary>
    /// Hebrew
    /// </summary>
    iso_8859_8,
    /// <summary>
    /// Western Europe with amended Turkish character set
    /// </summary>
    iso_8859_9,
    /// <summary>
    /// Western Europe with rationalised character set for Nordic languages, including complete Icelandic set
    /// </summary>
    iso_8859_10,
    /// <summary>
    /// Thai
    /// </summary>
    iso_8859_11,
    /// <summary>
    /// Baltic languages plus Polish
    /// </summary>
    iso_8859_13,
    /// <summary>
    /// Celtic languages
    /// </summary>
    iso_8859_14,
    /// <summary>
    /// Added the Euro sign and other rationalisations to ISO 8859-1
    /// </summary>
    iso_8859_15,
    /// <summary>
    /// Central European languages
    /// </summary>
    iso_8859_16,
    /// <summary>
    /// Basic English
    /// </summary>
    us_ascii,
    /// <summary>
    /// For Central European languages that use Latin script
    /// </summary>
    windows_1250,
    /// <summary>
    /// For Cyrillic alphabets
    /// </summary>
    windows_1251,
    /// <summary>
    /// For Western languages
    /// </summary>
    windows_1252,
    /// <summary>
    /// For Greek
    /// </summary>
    windows_1253,
    /// <summary>
    /// For Turkish
    /// </summary>
    windows_1254,
    /// <summary>
    /// For Hebrew
    /// </summary>
    windows_1255,
    /// <summary>
    /// For Arabic
    /// </summary>
    windows_1256,
    /// <summary>
    /// For Baltic languages
    /// </summary>
    windows_1257,
    /// <summary>
    /// For Vietnamese
    /// </summary>
    windows_1258,
    /// <summary>
    /// Russian
    /// </summary>
    koi8_r,
    /// <summary>
    /// Ukrainian
    /// </summary>
    koi8_u,
    /// <summary>
    /// Thai
    /// </summary>
    tis_620,
    /// <summary>
    /// MacRoman
    /// </summary>
    macintosh,
    /// <summary>
    /// Japanese, Unix
    /// </summary>
    euc_jp,
    /// <summary>
    /// Japanese, Win/Mac
    /// </summary>
    shift_jis,
    /// <summary>
    /// Korean
    /// </summary>
    euc_kr,
    /// <summary>
    /// Chinese, simplified
    /// </summary>
    gb2312,
    /// <summary>
    /// Chinese, simplified
    /// </summary>
    gb18030,
    /// <summary>
    /// Chinese, traditional
    /// </summary>
    big5
  }

}
