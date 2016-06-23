using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayProblems
{
  class Program
  {
    static void Main(string[] args)
    {

      string validParen = "{(Hello)}";
      IsValidString(validParen);
      int[] arr4 = {3, 4, 5, 6, 1, 2};
      pivotedBinarySearch(arr4, arr4.Length, 4);
      
      int[] ar3 = { 2,2,2 };
      findDuplicates(ar3);

      int[] ar1 = {1, 12, 15, 26, 38};
      int[] ar2 = {2, 13, 17, 30, 45};
      Console.WriteLine(findMedian(ar1, ar2));
      

      int[] arr1 = {10, 20, 9, 40};
      int x = 400;
      Console.WriteLine("{0}", isProduct(arr1, x));

      int[] arr2 = { 10, 2, 3, 5, 7, 8, 9, 1 };
      Calculate2MissingNumbers(arr2, 10);
      
    }

    static bool IsValidString(string s)
    {
      
      Stack<char> stack = new Stack<char>();
      char[] chars = s.ToCharArray();
      char top = (char) 0;
      for (int i = 0; i < chars.Length; i++)
      {
        char ch = s[i];
      
        switch (ch)
        {
          case '(':
          case '[':
          case '{':
            stack.Push(ch);
            break;

          case ')':
            if(stack.Count == 0) return false;
            top = stack.Pop();
            if (top != '(') return false;
             break;
          case ']':
             if (stack.Count == 0) return false;
             top = stack.Pop();
             if (top != '[') return false;
             break;
          case '}':
             if (stack.Count == 0) return false;
             top = stack.Pop();
             if (top != '{') return false;
             break;
         
        }

      }

      return true;

    }

    static void Print2Smallest(int[] arr)
    {
      int n = arr.Length;

      if (n < 2) return;

      int first = int.MaxValue , second = int.MaxValue;

      for (int i = 0; i < n; i++)
      {
        if (arr[i] < first)
        {
          second = first;
          first = arr[i];
        }
        else if (arr[i] > first && arr[i] < second)
        {
          second = arr[i];
        }

      }


    }

    static int pivotedBinarySearch(int[] arr, int n, int key)
  {
      int pivot = findPivot(arr, 0, n - 1);
      if (pivot == -1)
        return binarySearch(arr, 0, n - 1, key); ; //return binary search result as array is not pivoted
      
      if (arr[pivot] == key)
        return pivot;

      if (arr[0] <= key)
      {
        return binarySearch(arr, 0, pivot - 1, key);
      }
      return binarySearch(arr, pivot + 1, n - 1, key);
  }

    static int binarySearch(int[] arr, int low, int high, int key)
    {
      if (high < low) return -1;

      int mid = (low + high)/2;

      if (key == arr[mid])
        return mid;
      if (key > arr[mid])
        return binarySearch(arr, mid + 1, high, key);
      return binarySearch(arr, low, mid - 1, key);
    }

    static int findPivot(int[] arr, int low, int high)
    {
      if (high < low) return -1;
      if (high == low) return 0;
      int mid = (low + high)/2;
      if (mid < high && arr[mid] > arr[mid + 1])
      {
        return mid;
      }
      if (mid > low && arr[mid] < arr[mid - 1])
      {
        return mid - 1;
      }
      if (arr[low] >= arr[mid])
        return findPivot(arr, low, mid - 1);
      return findPivot(arr, mid + 1, high);
    }

    static int findDuplicates(int[] A)
    {
      if (A == null || A.Length == 0)
        return 0;
      int count = 0, size = A.Length;
      for (int j = 1; j < size; j++)
      {
        if (A[count] != A[j])
        {
          A[++count] = A[j];
        }
      }
      return count + 1;

    }
    

    static int findMedian(int[] a, int[] b)
    {
      int m = a.Length;
      int n = b.Length;

      if ((m + n) % 2 != 0)
      {
        return findKth(a, b, (m + n) / 2, 0, m - 1, 0, n - 1);

      }
      else
      {
        return (findKth(a, b, (m + n) / 2, 0, m - 1, 0, n - 1)
              + findKth(a, b, (m + n) / 2 - 1, 0, m - 1, 0, n - 1)) / 2;
      }
    }

    static int findKth(int[] a, int[] b, int k, int aStart, int aEnd, int bStart, int bEnd)
    {
      int aLen = aEnd - aStart + 1;
      int bLen = bEnd - bStart + 1;

      // Handle special cases
      if (aLen == 0)
        return b[bStart + k];
      if (bLen == 0)
        return a[aStart + k];
      if (k == 0)
        return a[aStart] < b[bStart] ? a[aStart] : b[bStart];

      int aMid = aLen * k / (aLen + bLen); // a’s middle count
      int bMid = k - aMid - 1; // b’s middle count
      if (a[aMid] > b[bMid])
      {
        k = k - (bMid - bStart + 1);
        aEnd = aMid;
        bStart = bMid + 1;
      }
      else
      {
        k = k - (aMid - aStart + 1);
        bEnd = bMid;
        aStart = aMid + 1;
      }
      return findKth(a, b, k, aStart, aEnd, bStart, bEnd);


    }

    static bool isProduct(int[] arr, int product)
    {
      var data = new Dictionary<int, int>();
      for (int i = 0; i < arr.Length; i++)
      {
        if (arr[i] == 0){
          if (product == 0){
            return true;
          }
            continue;
        }

        if (product%arr[i] == 0)
        {
          if (data.ContainsKey(product/arr[i]))
          {
            return true;
          }
          data.Add(arr[i], arr[i]);
        }
      }

      return false;
    }


    static void Calculate2MissingNumbers(int[] arr, int range)
    {
     
      int SUMN = (range*(range + 1))/2;

      int SUMARRAY = arr.Sum();

      int sumofMissingNumbes = Math.Abs(SUMN - SUMARRAY);

      int PRODUCTARRAY = 1;
      for (int i = 0; i < arr.Length; i++)
      {
        PRODUCTARRAY *= arr[i];
      }

      int PRODUCTNNumbers = 1;
      for (int i = 1; i <= range; i++)
      {
        PRODUCTNNumbers *= i;
      }

      int product = PRODUCTNNumbers / PRODUCTARRAY;

      int diffSqrt = (int)Math.Sqrt((sumofMissingNumbes*sumofMissingNumbes - 4*product));
      int a = (sumofMissingNumbes + diffSqrt) / 2;
      int b = sumofMissingNumbes - a;

      Console.WriteLine("{0}, {1}", a, b);
    }


    /* Function to get median of a sorted array */
      static int median(int[] arr, int n)
      {
          if (n%2 == 0)
              return (arr[n/2] + arr[n/2-1])/2;
          else
              return arr[n/2];
      }

  }
}
