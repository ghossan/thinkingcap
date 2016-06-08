using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNLCalculator
{
  class PNLCalculator{

    public void Process(){
      List<Fill> fills = ReadFills(@"D:\Test Applications\P&LCalculator\PNLCalculator\PNLCalculator\Data\fills.gz");
      List<Price> prices = ReadPrice(@"D:\Test Applications\P&LCalculator\PNLCalculator\PNLCalculator\Data\prices.gz");
      ProcessFillsForPriceUpdate(fills, prices);
      Console.WriteLine("Press enter to terminate");
      Console.ReadLine();
    }


    private void ProcessFillsForPriceUpdate(List<Fill> fills, List<Price> prices)
    {

      var MTMLedger = new Dictionary<Price, MTMPnLAndOpenQuantity>(prices.Count);
      var FillTransactionLedger = new Dictionary<string, FillPnLAndOpenQuantity>(prices.Count);
      int previousFillIndex = 0;
      for (int priceIndex = 0; priceIndex < prices.Count; priceIndex++)
      {
        Price currentPriceUpdate = prices[priceIndex];
        for (int fillIndex = previousFillIndex; fillIndex < fills.Count; fillIndex++)
        {
          Fill currentFill = fills[fillIndex];
          FillPnLAndOpenQuantity currentVal;
          if (!FillTransactionLedger.TryGetValue(currentFill.Symbol, out currentVal))
          {
            currentVal = new FillPnLAndOpenQuantity(0, 0);
          }
          if (currentPriceUpdate.Symbol == currentFill.Symbol && currentFill.Epoch > currentPriceUpdate.Epoch)
          {
            MTMPnLAndOpenQuantity mtmPnLData = CalculateMTMPnLAndOpenQuantity(currentVal, currentPriceUpdate);
            MTMLedger.Add(currentPriceUpdate, mtmPnLData);
            previousFillIndex = fillIndex;
            Console.WriteLine(mtmPnLData.ToString());
            break;
          }
          else
          {
            currentVal = CalculateFillPnLAndOpenQuantity(currentVal, currentFill);
            FillTransactionLedger[currentFill.Symbol] = currentVal;
          }
        }
      }

#if DEBUG
      Console.WriteLine("Number of MTM updates in the collection: {0}", MTMLedger.Count);
#endif

    }

    private FillPnLAndOpenQuantity CalculateFillPnLAndOpenQuantity(FillPnLAndOpenQuantity previousValue, Fill currentFill)
    {
      decimal realizedPnL = 0;
      long openQuantity = 0;
      if (currentFill.Side == "B"){
        realizedPnL = previousValue.RealizedPnL + (currentFill.FillPrice * currentFill.FillSize * -1);
        openQuantity = previousValue.OpenQuantity + currentFill.FillSize;
      }
     else if(currentFill.Side == "S"){
       realizedPnL = previousValue.RealizedPnL + (currentFill.FillPrice * currentFill.FillSize * 1);
       openQuantity = previousValue.OpenQuantity - currentFill.FillSize;
     }
      return new FillPnLAndOpenQuantity(realizedPnL, openQuantity);
    }

    private MTMPnLAndOpenQuantity CalculateMTMPnLAndOpenQuantity(FillPnLAndOpenQuantity currentPnL, Price priceUpdate){
      decimal markedToMarketPandL = currentPnL.RealizedPnL + currentPnL.OpenQuantity*priceUpdate.CurrentPrice;
      return new MTMPnLAndOpenQuantity(priceUpdate.Epoch, priceUpdate.Symbol, markedToMarketPandL, currentPnL.OpenQuantity);
    }

    private List<Fill> ReadFills(string filePath){
      var dataList = new List<Fill>(1000);
      using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)){
        using (var streamReader = new StreamReader(new GZipStream(fStream, CompressionMode.Decompress))){
          string line = null;
          while ((line = streamReader.ReadLine()) != null){
            try{
              dataList.Add(new Fill(line));
            }
            catch (Exception ex){
              Console.WriteLine("Exception reading fill file at line: {0}. Exception: {1}", line, ex);
            }
          }
        }
      }
      return dataList;
    }

    private List<Price> ReadPrice(string filePath){
      var dataList = new List<Price>(1000);
      using (var fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)){
        using (var streamReader = new StreamReader(new GZipStream(fStream, CompressionMode.Decompress))){
          string line = null;
          while ((line = streamReader.ReadLine()) != null){
            try{
              dataList.Add(new Price(line));
            }
            catch (Exception ex){
              Console.WriteLine("Exception reading price file at line: {0}. Exception: {1}", line, ex);
            }
          }
        }
      }
      return dataList;
    }


  }
}
