using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNLCalculator{
  class Program{

    static void Main(string[] args){
      if (ParseInputParams(args)){
        var calculator = new PNLCalculator();
        calculator.Process(args[0], args[1]);
      }
    }

    private static bool ParseInputParams(string[] args){
      // Test if input arguments were supplied:
      if (args.Length < 2){
        Console.WriteLine("path to fills and prices files required");
        return false;
      }
      string fillFilePath = args[0];
      if (!CheckIfPathExtensionIsGZipped(fillFilePath)){
        Console.WriteLine("provide gzip comparessed fill file");
        return false;
      }
      string priceFilePath = args[1];
      if (!CheckIfPathExtensionIsGZipped(priceFilePath)){
        Console.WriteLine("provide gzip comparessed price file");
        return false;
      }
      return true;
    }

    private static bool CheckIfPathExtensionIsGZipped(string path){
      if (Path.GetExtension(path) != ".gz"){
        return false;
      }
      return true;
    }
  }


  


 

 
}
