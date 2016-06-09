using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNLCalculator{
  
  struct Fill{
    private long m_Epoch;
    private string m_Symbol;
    private decimal m_FillPrice;
    private long m_FillSize;
    private string m_Side;

    public long Epoch { get { return m_Epoch; } }

    public string Symbol { get { return m_Symbol; } }

    public decimal FillPrice { get { return m_FillPrice; } }

    public long FillSize { get { return m_FillSize; } }

    public string Side { get { return m_Side; } }

    public Fill(string data)
    {
      string[] parts = data.Split(new char[] { ' ' });
      m_Epoch = long.Parse(parts.ElementAt(1));
      m_Symbol = parts.ElementAt(2);
      m_FillPrice = decimal.Parse(parts.ElementAt(3));
      m_FillSize = long.Parse(parts.ElementAt(4));
      m_Side = parts.ElementAt(5);
    }
  }

  struct PriceUpdate{
    private long m_Epoch;
    private string m_Symbol;
    private decimal m_CurrentPrice;
    public long Epoch { get { return m_Epoch; } }
    public string Symbol { get { return m_Symbol; } }
    public decimal CurrentPrice { get { return m_CurrentPrice; } }

    public PriceUpdate(string data){
      string[] parts = data.Split(new char[] { ' ' });
      m_Epoch = long.Parse(parts.ElementAt(1));
      m_Symbol = parts.ElementAt(2);
      m_CurrentPrice = decimal.Parse(parts.ElementAt(3));
    }
  }


  struct MTMPnLAndOpenQuantity{
    private long m_epochTimeStamp;
    private string m_symbol;
    private decimal m_markedToMarketPNL;
    private long m_openQuantity;

    public decimal MarkedToMarketPnl
    {
      get { return m_markedToMarketPNL; }
    }

    public long OpenQuantity
    {
      get { return m_openQuantity; }
    }

    public MTMPnLAndOpenQuantity(long epochTimeStamp, string symbol, decimal markedToMarketPnl, long openQuantity){
      m_epochTimeStamp = epochTimeStamp;
      m_symbol = symbol;
      m_markedToMarketPNL = markedToMarketPnl;
      m_openQuantity = openQuantity;
    }

    public override string ToString(){
      return string.Format("PNL {0} {1} {2} {3}",
        m_epochTimeStamp,
        m_symbol,
        OpenQuantity,
        MarkedToMarketPnl);
    }
  }


  struct FillPnLAndOpenQuantity{
    private decimal m_realizedCost;
    private long m_openQuantity;

    public FillPnLAndOpenQuantity(decimal realizedCost, long openQuantity){
      m_realizedCost = realizedCost;
      m_openQuantity = openQuantity;
    }

    public decimal RealizedCost{
      get { return m_realizedCost; }
    }

    public long OpenQuantity{
      get { return m_openQuantity; }
    }
  }

}
