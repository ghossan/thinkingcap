using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PNLCalculator.UnitTests
{
  [TestClass]
  public class PNLCalculatorTest
  {
    [TestInitialize]
    public void TestInitialize()
    {
      m_mtmPnLData = 4615;
      m_mtmOpenQuantity = -1800;
    }

    [TestMethod]
    public void TestMethod1()
    {
      var calculator = new PNLCalculator();
      Dictionary<PriceUpdate, MTMPnLAndOpenQuantity>  data = calculator.ProcessFillsForPriceUpdate(@"..\..\..\Data\testfill.gz", 
                                                                                              @"..\..\..\Data\testprice.gz");

      if (data != null)
      {
        MTMPnLAndOpenQuantity mtmpnLData = data.Values.Last();
        Assert.AreEqual(mtmpnLData.MarkedToMarketPnl, m_mtmPnLData);
        Assert.AreEqual(mtmpnLData.OpenQuantity, m_mtmOpenQuantity);
      }
    }

    private static decimal m_mtmPnLData;
    private static long m_mtmOpenQuantity;
  }
}
