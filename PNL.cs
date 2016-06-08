using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ssha_local {
    internal class PNL {

        Price[] prices;
        Fill[] fills;
        Dictionary<string, Entry> ledger = new Dictionary<string, Entry>();

        internal void Start() {
            fills = readFills(@"E:\workspaces\temp\SG-0612\fills.txt");
            prices = readPrice(@"E:\workspaces\temp\SG-0612\prices.txt");

            //Keep track of net position and PNL for each symbol
            

            int totalPrices = prices.Length;
            Price currentPrice;
            Price previousPrice;

            //Requirement - You will output one PNL message for each Price Update message
            for (int i = 0; i < totalPrices; i++) {
                currentPrice = prices[i];

                //i==0 introduces a subtle bug. you should pick the first price for a given symbol.
                // Previous price should be index agnostic
                //previousPrice = (i == 0)? new Price(): prices[i - 1];
                previousPrice = getPreviousPrice(currentPrice.Epoch, currentPrice.Symbol);
                sumMTM(currentPrice, previousPrice);
            }

        }


        /// <summary>
        /// Retrieves the previous last known Price for a given symbol. Prices are sorted by epoch during retrieval. This is index agnostic.
        /// and terribly inefficient too.
        /// </summary>
        /// <param name="currentEpoch"></param>
        /// <returns></returns>
        Price getPreviousPrice(Int64 currentEpoch, string Symbol) {
            return (from p in prices
                       where p.Epoch < currentEpoch && string.Compare(p.Symbol,Symbol)==0
                    select p).LastOrDefault();
        }


        /// <summary>
        /// Spits out the MTM
        /// </summary>
        /// <param name="currentPrice"></param>
        /// <param name="previousPrice"></param>
        void sumMTM(Price currentPrice,Price previousPrice) {

            //1. Get all fills for the given symbol and epochs
            var partFills = fills.Where(a => (a.Symbol == currentPrice.Symbol 
            && a.Epoch > previousPrice.Epoch 
            && a.Epoch <= currentPrice.Epoch)).Take(2);//taking just two here//WARNING


            //keep track of rows that are retrieved for a given price.
            var currentDateTime = new DateTime(1970, 1, 1, 0, 0, 0,0, DateTimeKind.Utc).AddMilliseconds(currentPrice.Epoch).ToString("MM/dd/yyyy HH:mm:ss:fff");
            Console.Write("{0} - {1} - {2} | ", currentPrice.Symbol, currentDateTime, partFills.Count());
            //var tempPrice = partFills.Sum(a => a.Price); //Remember that we have added signs while retrieving price & positions from file
            var tempPos = partFills.Sum(a => a.Position);
            var tempCostofTransaction = partFills.Sum(a =>  a.Position * Math.Abs(a.Price));
            var tempMarketValue = partFills.Sum(a => a.Position * currentPrice.ClosingPrice);

#if DEBUG
            //temporary variables to capture the metrics for a given price.
            Console.WriteLine("Net: {0} position, {1} price, {2} tran cost, {3} market value", tempPos, currentPrice.ClosingPrice, tempCostofTransaction, tempMarketValue);
#endif

            //2. what is the MTM for current price?
            // using http://ibkb.interactivebrokers.com/node/56
            // 2a. Calculate Transaction MTM
            var transactionMTM = partFills.Sum(a => (currentPrice.ClosingPrice - a.Price) * a.Position);


            //Store the position and PNL for future calculations
            Entry tempLedgerEntry;
            if (ledger.ContainsKey(currentPrice.Symbol)) {
                tempLedgerEntry = ledger[currentPrice.Symbol];
                tempLedgerEntry.Position += tempPos;
                tempLedgerEntry.Bookprice += tempCostofTransaction;
                ledger.Remove(currentPrice.Symbol);//should I really remove this?
            }else {
                //This is the first time we are seeing this symbol. add it to our ledger
                tempLedgerEntry = new Entry();
                tempLedgerEntry.Symbol = currentPrice.Symbol;
                tempLedgerEntry.Position = tempPos;
                tempLedgerEntry.Bookprice = tempCostofTransaction;
                
            }
            ledger.Add(currentPrice.Symbol, tempLedgerEntry);
#if DEBUG
            Console.WriteLine("My effective Book - {0} {1} ${2} ", tempLedgerEntry.Symbol, tempLedgerEntry.Position, tempLedgerEntry.Bookprice);
            Console.WriteLine();
#endif

            // 2b. Calculate Prior Period MTM
            var priorPeriodMTM = partFills.Sum(a=> (currentPrice.ClosingPrice - previousPrice.ClosingPrice) * tempLedgerEntry.Position);


            // 2c. Total MTM - write to console.
            Console.WriteLine("PNL {0} {1} {2} {3}",currentPrice.Epoch, currentPrice.Symbol, tempLedgerEntry.Position, (priorPeriodMTM-transactionMTM));

        }

        internal struct Entry {
            internal string Symbol { get; set; }
            internal Int64 Position { get; set; }

            /// <summary>
            /// Total cost of my transaction
            /// </summary>
            internal decimal Bookprice { get; set; }
        }

        internal struct MTM {
            internal Int64 Position { get; set; }
            internal decimal Price { get; set; }
        }



        Fill[] readFills(string path) {

            //Do we care if the data is sorted in any particular order? 
            //Make sure that the assumptions are listed in code
            var fills = (from l in File.ReadAllLines(path)
                          let s = l.Split(new char[] { ' ' })
                          let isSell= string.Compare(s.ElementAt(5),"S")==0 // are we selling?
                          let price= decimal.Parse(s.ElementAt(3))
                          let position= Int32.Parse(s.ElementAt(4))
                         select new Fill {
                              Epoch = Int64.Parse(s.ElementAt(1)),
                              Symbol = s.ElementAt(2),
                              Price = isSell?price: price*-1, //if we are selling, then subtract money from books
                              Position = isSell?position*-1:position,//if we are selling, then we have negative positions
                              PositionFlag = s.ElementAt(5)
                          }).OrderBy(a=>a.Epoch).Take(5000).ToArray(); //taking on first 5000

            Console.WriteLine("Fills {0}", fills.Count());

            return fills;
        }

        Price[] readPrice(string path) {
            var prices = (from l in File.ReadAllLines(path)
                          let s = l.Split(new char[] { ' ' })
                          let sym= s.ElementAt(2)
                          where sym=="MSFT"
                          select new Price {
                              Flag = s.ElementAt(0),
                              Epoch = Int64.Parse(s.ElementAt(1)),
                              Symbol = s.ElementAt(2),
                              ClosingPrice = decimal.Parse(s.ElementAt(3))
                          }).OrderBy(a=>a.Epoch).Take(5).ToArray(); //Taking only first 100 to debug

            Console.WriteLine("Prices {0}", prices.Count());

            return prices;
        }

        internal struct Fill {
            internal string Flag { get { return "F"; } }
            internal Int64 Epoch { get; set; }

            internal string Symbol { get; set; }

            internal decimal Price { get; set; }

            internal int Position { get; set; }

            internal string PositionFlag { get; set; }
        }


        internal struct Price {

            internal string Flag { get; set; }

            internal Int64 Epoch { get; set; }

            internal string Symbol { get; set; }

            internal decimal ClosingPrice { get; set; }
        }

    }
}
