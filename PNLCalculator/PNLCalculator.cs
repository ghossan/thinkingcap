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

    public void Process(string fillFilePath, string priceFillPath){
    //  List<Fill> fills = ReadFills(fillFilePath);
     // List<Price> prices = ReadPrice(priceFillPath);
    //  ProcessFillsForPriceUpdate(fills, prices);

      ProcessFillsForPriceUpdate(fillFilePath, priceFillPath);
      Console.WriteLine("Press enter to terminate");
      Console.ReadLine();
    }

    private void ProcessFillsForPriceUpdate(string fillFilePath, string priceFillPath)
    {
      var mtmLedger = new Dictionary<Price, MTMPnLAndOpenQuantity>(1000);
      var fillTransactionLedger = new Dictionary<string, FillPnLAndOpenQuantity>(1000);
      Console.WriteLine("Start reading price and fill data files..");
      using (var priceStream = new FileStream(priceFillPath, FileMode.Open, FileAccess.Read, FileShare.Read)){
        using (var priceStreamReader = new StreamReader(new GZipStream(priceStream, CompressionMode.Decompress))){

          using (var fillStream = new FileStream(fillFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)){
            using (var fillStreamReader = new StreamReader(new GZipStream(fillStream, CompressionMode.Decompress))){

              string priceDataLine = null;
              while ((priceDataLine = priceStreamReader.ReadLine()) != null){
                try{
                var currentPriceUpdate = new Price(priceDataLine);
                string fillDataLine = null;
                while ((fillDataLine = fillStreamReader.ReadLine()) != null){
                  try{
                    bool exitLoop = false;
                    var currentFill = new Fill(fillDataLine);
                    FillPnLAndOpenQuantity currentVal;
                    if (!fillTransactionLedger.TryGetValue(currentFill.Symbol, out currentVal))
                    {
                      currentVal = new FillPnLAndOpenQuantity(0, 0);
                    }
                    if (currentPriceUpdate.Symbol == currentFill.Symbol && currentFill.Epoch > currentPriceUpdate.Epoch)
                    {
                      MTMPnLAndOpenQuantity mtmPnLData = CalculateMTMPnLAndOpenQuantity(currentVal, currentPriceUpdate);
                      mtmLedger.Add(currentPriceUpdate, mtmPnLData);
                      Console.WriteLine(mtmPnLData.ToString());
                      exitLoop = true;
                    }
                    currentVal = CalculateFillPnLAndOpenQuantity(currentVal, currentFill);
                    fillTransactionLedger[currentFill.Symbol] = currentVal;
                    if (exitLoop){
                      break;
                    }
                  }
                  catch (Exception ex){
                    Console.WriteLine("Exception reading fill file at line: {0}. Exception: {1}", fillDataLine, ex);
                  }
                }
                }
                catch (Exception ex){
                  Console.WriteLine("Exception reading price file at line: {0}. Exception: {1}", priceDataLine, ex);
                }
              }
            }
          }
        }
      }
      Console.WriteLine("End reading price and fill data files.");
      Console.WriteLine("Number of MTM updates: {0}", mtmLedger.Count);
    }

    private void ProcessFillsForPriceUpdate(List<Fill> fills, List<Price> prices){
      var mtmLedger = new Dictionary<Price, MTMPnLAndOpenQuantity>(prices.Count);
      var fillTransactionLedger = new Dictionary<string, FillPnLAndOpenQuantity>(prices.Count);
      int previousFillIndex = -1;

      for (int priceIndex = 0; priceIndex < prices.Count; priceIndex++){
        Price currentPriceUpdate = prices[priceIndex];

        for (int fillIndex = previousFillIndex + 1; fillIndex < fills.Count; fillIndex++){
          bool exitLoop = false;
          Fill currentFill = fills[fillIndex];
          FillPnLAndOpenQuantity currentVal;
          if (!fillTransactionLedger.TryGetValue(currentFill.Symbol, out currentVal)){
            currentVal = new FillPnLAndOpenQuantity(0, 0);
          }
          if (currentPriceUpdate.Symbol == currentFill.Symbol && currentFill.Epoch > currentPriceUpdate.Epoch){
            MTMPnLAndOpenQuantity mtmPnLData = CalculateMTMPnLAndOpenQuantity(currentVal, currentPriceUpdate);
            mtmLedger.Add(currentPriceUpdate, mtmPnLData);
            previousFillIndex = fillIndex;
            Console.WriteLine(mtmPnLData.ToString());
            exitLoop = true;
          }
          currentVal = CalculateFillPnLAndOpenQuantity(currentVal, currentFill);
          fillTransactionLedger[currentFill.Symbol] = currentVal;
          if (exitLoop){
            break;
          }
        }
      }
      Console.WriteLine("Number of MTM updates in the collection: {0}", mtmLedger.Count);
    }

    private FillPnLAndOpenQuantity CalculateFillPnLAndOpenQuantity(FillPnLAndOpenQuantity previousValue, Fill currentFill)
    {
      decimal runningCost = 0;
      long openQuantity = 0;

      decimal currentRunningCost = currentFill.FillPrice*currentFill.FillSize;
      if (currentFill.Side == "B"){
        runningCost = previousValue.RealizedCost + (currentRunningCost  * -1);
        openQuantity = previousValue.OpenQuantity + currentFill.FillSize;
      }
     else if(currentFill.Side == "S"){
       runningCost = previousValue.RealizedCost + currentRunningCost;
       openQuantity = previousValue.OpenQuantity - currentFill.FillSize;
     }
      return new FillPnLAndOpenQuantity(runningCost, openQuantity);
    }

    private MTMPnLAndOpenQuantity CalculateMTMPnLAndOpenQuantity(FillPnLAndOpenQuantity currentPnL, Price priceUpdate){
      decimal markedToMarketPandL = currentPnL.RealizedCost + currentPnL.OpenQuantity*priceUpdate.CurrentPrice;
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
