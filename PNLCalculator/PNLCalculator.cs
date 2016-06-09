using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("PNLCalculator.UnitTests")]
namespace PNLCalculator
{
  class PNLCalculator{

    /// <summary>
    /// Starting point of data processing
    /// </summary>
    /// <param name="fillFilePath"></param>
    /// <param name="priceFillPath"></param>
    public void Process(string fillFilePath, string priceFillPath){
      ProcessFillsForPriceUpdate(fillFilePath, priceFillPath);
      Console.WriteLine("Press enter to terminate");
      Console.ReadLine();
    }

    /// <summary>
     ///  Opens Price and Fill data files and does a forward scan on both the files. Calculates MTM PnL 
     /// while the file is being scanned, hence done in one pass
    /// </summary>
    /// <param name="fillFilePath"></param>
    /// <param name="priceFillPath"></param>
    /// <returns></returns>
    public Dictionary<PriceUpdate, MTMPnLAndOpenQuantity> ProcessFillsForPriceUpdate(string fillFilePath, string priceFillPath){
      var mtmLedger = new Dictionary<PriceUpdate, MTMPnLAndOpenQuantity>(1000);
      var fillTransactionLedger = new Dictionary<string, FillPnLAndOpenQuantity>(1000);
      Console.WriteLine("Start reading price and fill data files..");
      using (var priceStream = new FileStream(priceFillPath, FileMode.Open, FileAccess.Read, FileShare.Read)){
        using (var priceStreamReader = new StreamReader(new GZipStream(priceStream, CompressionMode.Decompress))){

          using (var fillStream = new FileStream(fillFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)){
            using (var fillStreamReader = new StreamReader(new GZipStream(fillStream, CompressionMode.Decompress))){

              string priceDataLine = null;
              while ((priceDataLine = priceStreamReader.ReadLine()) != null){
                try{
                var currentPriceUpdate = new PriceUpdate(priceDataLine);
                string fillDataLine = null;

                while ((fillDataLine = fillStreamReader.ReadLine()) != null){
                  try{
                    bool exitLoop = false;
                    var currentFill = new Fill(fillDataLine);

                    FillPnLAndOpenQuantity currentVal;
                    if (!fillTransactionLedger.TryGetValue(currentFill.Symbol, out currentVal)){
                      currentVal = new FillPnLAndOpenQuantity(0, 0);
                    }

                    if (currentPriceUpdate.Symbol == currentFill.Symbol && currentFill.Epoch > currentPriceUpdate.Epoch){
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
      return mtmLedger;
    }

    /// <summary>
    /// Calculates the running Realized Cost and Open Quantity for every Fill transaction for each security
    /// Side	          Realized Cost	                                OpenQuantity
    /// Buy	  Realized Cost -= FillPrice * FillQuantity	  OpenQuantity += FillQuantity
    /// Sell	Realized Cost += FillPrice * FillQuantity	  OpenQuantity -= FillQuantity
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="currentFill"></param>
    /// <returns></returns>
    private FillPnLAndOpenQuantity CalculateFillPnLAndOpenQuantity(FillPnLAndOpenQuantity previousValue, Fill currentFill){
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

    /// <summary>
    /// Calculates the Mark to Market PnL for each Price update
    /// Mark To Market PnL = 	Realized Cost + OpenQuantity * CurrentPrice
    /// </summary>
    /// <param name="currentPnL"></param>
    /// <param name="priceUpdate"></param>
    /// <returns></returns>
    private MTMPnLAndOpenQuantity CalculateMTMPnLAndOpenQuantity(FillPnLAndOpenQuantity currentPnL, PriceUpdate priceUpdate){
      decimal markedToMarketPandL = currentPnL.RealizedCost + currentPnL.OpenQuantity*priceUpdate.CurrentPrice;
      return new MTMPnLAndOpenQuantity(priceUpdate.Epoch, priceUpdate.Symbol, markedToMarketPandL, currentPnL.OpenQuantity);
    }

  }
}
