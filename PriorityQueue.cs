using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_Wheeler_Compression
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> items;
        public PriorityQueue()
        {
            items = new List<T>();
        }
        public int Size => items.Count;
        private void Swaping(int i, int j)
        {
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
        public void Enqueue(T item)
        {
            items.Add(item);
            int currentIndex = items.Count - 1;
            int parentIndex = (currentIndex - 1) / 2;

            while (currentIndex > 0 && items[currentIndex].CompareTo(items[parentIndex]) < 0)
            {
                Swaping(currentIndex, parentIndex);
                currentIndex = parentIndex;
                parentIndex = (currentIndex - 1) / 2;
            }
        }
        public T Dequeue()
        {
            if (items.Count != 0)
            {
                T top = items[0];
                items[0] = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);

                int index = 0;
                while (true)
                {
                    int leftChildIndex = 2 * index + 1;
                    int rightChildIndex = 2 * index + 2;

                    int smallestIndex = index;

                    if (leftChildIndex < items.Count && items[leftChildIndex].CompareTo(items[smallestIndex]) < 0)
                        smallestIndex = leftChildIndex;
                    if (rightChildIndex < items.Count && items[rightChildIndex].CompareTo(items[smallestIndex]) < 0)
                        smallestIndex = rightChildIndex;

                    if (smallestIndex == index)
                        break;

                    Swaping(index, smallestIndex);
                    index = smallestIndex;
                }
                return top;
            }
            else
            {
                throw new InvalidOperationException("Priority queue empty.");
            }
        }
    }
}
