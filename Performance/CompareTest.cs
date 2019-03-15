using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Performance {

  internal enum TestEnum {
    One,
    Two,
    Three
  }

  public class CompareTest {

    public void Run() {
      TestEnum e = TestEnum.Three;
      Stopwatch w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000000; i++) {
        if (e == TestEnum.Three) {
          int x = 1;
          x++;
        }
      }
      w.Stop();
      Console.WriteLine(w.ElapsedTicks);
    }

  }

}
