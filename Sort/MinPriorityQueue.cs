using System;


namespace MergeSort
{
  class MinPriorityQueue
  {

    private IComparable[] m_queue;
    private int m_queueCount = 0;
    public MinPriorityQueue(int size)
    {
      m_queue = new IComparable[size];
    }

    public void Enqueue(IComparable obj){
      if (++m_queueCount >= m_queue.Length){
        ResizeQueue();
      }
      	// initialize
			int childIndex = m_queueCount;
			int parentIndex = childIndex >> 1;
      while (parentIndex > 0){
        if (m_queue[parentIndex].CompareTo(obj) > 0){
          m_queue[childIndex] = m_queue[parentIndex];
        }
        else{
          break;
        }
        // move up heap
        childIndex = parentIndex;
        parentIndex = childIndex >> 1;
      }
      m_queue[childIndex] = obj;
    }

    public IComparable Dequeue()
    {
      int rootIndex = 0;
      // get front of queue

      IComparable retObj = m_queue[rootIndex];

      if (m_queueCount > 1)
      {
        int parent = rootIndex;
        int leftChild = parent >> 1;
        int rightChild = leftChild + 1;

        while (leftChild <= m_queueCount && rightChild <= m_queueCount)
        {
          int higherPriorityIndex = m_queue[leftChild].CompareTo(m_queue[rightChild]) > 0
            ? rightChild
            : leftChild;

          if (m_queue[higherPriorityIndex].CompareTo(m_queue[m_queueCount]) > 0)
          {
            // we're going to swap in last instead of children
            break;
          }
          m_queue[parent] = m_queue[higherPriorityIndex];
          // set indexes
          parent = higherPriorityIndex;
          leftChild = parent << 1;
          rightChild = leftChild + 1;

        }
        m_queue[parent] = m_queue[m_queueCount];
      }

      m_queue[m_queueCount] = null;
      m_queueCount--;
      return retObj;
    }

    private void ResizeQueue()
    {
      // allocate new queue, using growth factor
      var newQueue = new IComparable[(int)(m_queue.Length * 2)];

      // copy element from old queue
      Array.Copy(m_queue, newQueue, m_queue.Length);

      // release old queue and keep new one
      m_queue = newQueue;
    }
  }
}
