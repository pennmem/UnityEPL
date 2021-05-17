using System;
using System.Threading;

#if TESTING
class InterfaceManager {
  public static ThreadLocal<System.Random> rnd = ThreadLocal<System.Random>(() => new System.Random());
}
#endif