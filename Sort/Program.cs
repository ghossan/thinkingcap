using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeSort
{
  class Program
  {
    static void Main(string[] args)
    {

      int[] numbers = { 3, 8, 7, 5 , 1, 9, 5};
      QuickSortRecursive(numbers, 0, numbers.GetLength(0) - 1);
      for (int k = 0; k < numbers.GetLength(0); k++)
      {
        Console.WriteLine("{0} ", numbers[k]);
      }
    }


    private static int partition(int[] a, int lo, int hi)
    {
      int i = lo;
      int j = hi+1;
      int v = a[lo];
      while (true)
      {

        // find item on lo to swap
        while (a[++i] <= v)
          if (i == hi) break;

        // find item on hi to swap
        while (a[--j] > v)
          if (j == lo) break;      // redundant since a[lo] acts as sentinel

        // check if pointers cross
        if (i >= j) break;

        exch(a, i, j);
      }

      // put partitioning item v at a[j]
      exch(a, lo, j);

      // now, a[lo .. j-1] <= a[j] <= a[j+1 .. hi]
      return j;
    }


    // exchange a[i] and a[j]
    private static void exch(int[] a, int i, int j)
    {
      int swap = a[i];
      a[i] = a[j];
      a[j] = swap;
    }

    public static void QuickSortRecursive(int[] data, int left, int right)
    {
        if (right <= left) return;
        int q = partition(data, left, right);
        QuickSortRecursive(data, left, q - 1);
        QuickSortRecursive(data, q + 1, right);
      
    }

    private static int Partition(ref int[] data, int left, int right)
    {
      int pivot = data[right];
      int temp;
      int i = left;

      for (int j = left; j < right; ++j)
      {
        if (data[j] <= pivot)
        {
          temp = data[j];
          data[j] = data[i];
          data[i] = temp;
          i++;
        }
      }

      data[right] = data[i];
      data[i] = pivot;

      return i;
    }
    public static void MergeSort(int[] input, int left, int right)
    {
      if (left < right)
      {
        int middle = (left + right) / 2;

        MergeSort(input, left, middle);
        MergeSort(input, middle + 1, right);

        //Merge
        int[] leftArray = new int[middle - left + 1];
        int[] rightArray = new int[right - middle];

        Array.Copy(input, left, leftArray, 0, middle - left + 1);
        Array.Copy(input, middle + 1, rightArray, 0, right - middle);

        int i = 0;
        int j = 0;
        for (int k = left; k < right + 1; k++)
        {
          if (i == leftArray.Length)
          {
            input[k] = rightArray[j];
            j++;
          }
          else if (j == rightArray.Length)
          {
            input[k] = leftArray[i];
            i++;
          }
          else if (leftArray[i] <= rightArray[j])
          {
            input[k] = leftArray[i];
            i++;
          }
          else
          {
            input[k] = rightArray[j];
            j++;
          }
        }
      }
    }

  }
}
