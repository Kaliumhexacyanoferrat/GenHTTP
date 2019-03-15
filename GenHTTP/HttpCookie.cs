using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {
  
  /// <summary>
  /// Represents a HTTP cookie.
  /// </summary>
  [Serializable]
  public class HttpCookie : MarshalByRefObject {
    private string _Name;
    private string _Value;
    private DateTime? _Expires;
    private ulong? _MaxAge;

    /// <summary>
    /// Create a new HTTP cookie.
    /// </summary>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The content of the cookie</param>
    public HttpCookie(string name, string value) {
      _Name = name;
      _Value = value;
    }

    /// <summary>
    /// Create a new HTTP cookie.
    /// </summary>
    /// <param name="name">The name of the cookie</param>
    /// <param name="value">The content of the cookie</param>
    /// <param name="expires">The expire date of the cookie</param>
    /// <param name="maxAge">The maximum age of the cookie</param>
    public HttpCookie(string name, string value, DateTime expires, ulong maxAge) : this(name, value) {
      _Name = name;
      _Value = value;
      _Expires = expires;
      _MaxAge = maxAge;
    }

    #region get-/setters

    /// <summary>
    /// The name of the cookie.
    /// </summary>
    public string Name {
      get { return _Name; }
    }

    /// <summary>
    /// The content of the cookie.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// The expire date of the cookie.
    /// </summary>
    public DateTime? Expires {
      get { return _Expires; }
      set { _Expires = value; }
    }

    /// <summary>
    /// The maximum age of the cookie.
    /// </summary>
    public ulong? MaxAge {
      get { return _MaxAge; }
      set { _MaxAge = value; }
    }

    #endregion

    /// <summary>
    /// Serialize this object to a HTTP response header line.
    /// </summary>
    /// <returns>The HTTP representation of this cookie</returns>
    internal string ToHttp() {
      string cookie = "Set-Cookie: " + _Name + "=" + _Value;
      if (_Expires.HasValue) {
        DateTime t = _Expires.Value;
        cookie += "; expires=" + GetDayOfWeek(t) + ", " + GetTwoDigits(t.Day) + "-" + GetMonth(t) + "-" + t.Year + " " + GetTwoDigits(t.Hour) + ":" + GetTwoDigits(t.Minute) + ":" + GetTwoDigits(t.Second) + " GMT";
      }
      if (_MaxAge.HasValue) {
        cookie += "; Max-Age=" + _MaxAge.Value;
      }
      cookie += "; Path=/";
      return cookie;
    }

    private string GetDayOfWeek(DateTime t) {
      switch (t.DayOfWeek) {
        case DayOfWeek.Monday: return "Mon";
        case DayOfWeek.Tuesday: return "Tue";
        case DayOfWeek.Wednesday: return "Wed";
        case DayOfWeek.Thursday: return "Thu";
        case DayOfWeek.Friday: return "Fri";
        case DayOfWeek.Saturday: return "Sat";
        case DayOfWeek.Sunday: return "Sun";
      }
      return "Mon";
    }

    private string GetTwoDigits(int t) {
      if (t < 10) return "0" + t;
      return t.ToString();
    }

    private string GetMonth(DateTime t) {
      if (t.Month ==  1) return "Jan";
      if (t.Month ==  2) return "Feb";
      if (t.Month ==  3) return "Mar";
      if (t.Month ==  4) return "Apr";
      if (t.Month ==  5) return "May";
      if (t.Month ==  6) return "Jun";
      if (t.Month ==  7) return "Jul";
      if (t.Month ==  8) return "Aug";
      if (t.Month ==  9) return "Sep";
      if (t.Month == 10) return "Okt";
      if (t.Month == 11) return "Nov";
      if (t.Month == 12) return "Dec";
      return "Jan";
    }

  }

}
