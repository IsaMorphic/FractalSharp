using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MandelBrot.Utilities;
namespace MandelBrotTests
{
    [TestClass]
    public class TinyDoubleTest
    {

        [TestMethod]
        public void FastDouble_Addition()
        {
            FastDouble test = FastDouble.Add(new FastDouble(200), new FastDouble(10));
        }

        [TestMethod]
        public void FastDouble_Multiplication()
        {
            Console.WriteLine(FastDouble.Multiply(new FastDouble(0.01234), new FastDouble(1.2345)).toDisplayString());
        }

        [TestMethod]
        public void Double_Division()
        {
            double test = 3.10234865021 / 7.0;
        }

    }
}
