using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using GenHTTP.Abstraction;

namespace Performance {

  public class LanguageTest {

    public void Run() {
      LanguageInfo info = new LanguageInfo(Language.German, Country.Switzerland);
      Stopwatch w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000; i++) {
        string s = info.LanguageString;
      }
      w.Stop();
      Console.WriteLine(w.ElapsedTicks);
    }

  }

}

