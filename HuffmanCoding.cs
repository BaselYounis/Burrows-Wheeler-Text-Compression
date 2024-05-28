using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_Wheeler_Compression
{
    public class HuffmanTreeNode : IComparable<HuffmanTreeNode>
    {
        // Fields
        private char symbol;
        private int frequency;
        private HuffmanTreeNode? left;
        private HuffmanTreeNode? right;

        // Properties
        public char Symbol
        {
            get => symbol;
            set => symbol = value;
        }
        public int Frequency
        {
            get => frequency;
            set => frequency = value;
        }
        public HuffmanTreeNode? Left
        {
            get => left;
            set => left = value;
        }
        public HuffmanTreeNode? Right
        {
            get => right;
            set => right = value;
        }

        // Constructors
        public HuffmanTreeNode(char symbol, int frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
            Left = null;
            Right = null;
        }
        public HuffmanTreeNode()
        {
            Left = null;
            Right = null;
        }

        // Methods
        public int CompareTo(HuffmanTreeNode? other) //O(1).
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            else
            {
                return Frequency - other.Frequency;
            }
        }
    }
    internal class HuffmanCoding
    {
        public static Dictionary<char, int> frequencies = new Dictionary<char, int>();
        static PriorityQueue<HuffmanTreeNode> nodesQueue = new PriorityQueue<HuffmanTreeNode>();
        static Dictionary<char, string> binaryRep = new Dictionary<char, string>();
        public static HuffmanTreeNode root = new HuffmanTreeNode();

        private static HuffmanTreeNode BuildTree()  //~O(Log N + C Log C)
        {
            CreatePriorityQueue();      //~O(C Log C)
            while (nodesQueue.Size > 1) //O(Log N)
            {
                HuffmanTreeNode left = nodesQueue.Dequeue();
                HuffmanTreeNode right = nodesQueue.Dequeue();
                HuffmanTreeNode parent = new HuffmanTreeNode('\0', left.Frequency + right.Frequency)
                {
                    Left = left,
                    Right = right
                };
                nodesQueue.Enqueue(parent);
            }

            return nodesQueue.Dequeue();
        }
        public static void CalculateFrequencies(string text)    //~O(N+C Log C)
        {
            foreach (char character in text)    //~O(N)
            {
                int count;
                if (frequencies.TryGetValue(character, out count))
                {
                    frequencies[character] = count + 1;
                }
                else
                {
                    frequencies[character] = 1;
                }
            }
            var sortedDictionary = frequencies.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);  //~O(C Log C)
            frequencies = sortedDictionary;
        }
        private static void CreatePriorityQueue()   //~O(C Log C)
        {
            foreach (var i in frequencies)
            {
                var new_node = new HuffmanTreeNode(i.Key, i.Value);
                nodesQueue.Enqueue(new_node);
            }
        }
        private static void GetBinaryRepresentation(HuffmanTreeNode root, string code = "")//~O(N +C Log C)
        {
            if (root == null)
                return;

            if (root.Left == null && root.Right == null)
            {
                if (code == "")
                    binaryRep[root.Symbol] = "1";
                else
                    binaryRep[root.Symbol] = code;
                return;
            }
            GetBinaryRepresentation(root.Right, code + "1");
            GetBinaryRepresentation(root.Left, code + "0");

        }
        private static string Compress(string text)     //~O(N)
        {
            StringBuilder compressedData = new StringBuilder();
            foreach (var symbol in text)
            {
                compressedData.Append(binaryRep[symbol]);
            }
            return compressedData.ToString();
        }
        public static string Encode(string text)    //~O(N +C Log C)
        {
            CalculateFrequencies(text);     //~O(N + C Log C)
            root = BuildTree();     //~O(Log N + C Log C)
            if (root == null)
                return "";
            if (frequencies.Count != 1)
                GetBinaryRepresentation(root, "");  //~O(N +C Log C)
            else if (frequencies.Count == 1)
                GetBinaryRepresentation(root, "*");

            return Compress(text);  //~O(N)
        }
        public static string Decode(string text)  //~O(Log N + C Log C)
        {
            //Decode from file
            if (root.Right == null && root.Left == null)
                root = BuildTree();     //~O(Log N + C Log C)

            HuffmanTreeNode curr = root;
            StringBuilder s = new StringBuilder();
            List<string> binary = new List<string>();
            StringBuilder decoded = new StringBuilder();

            foreach (var item in text)      //~O(N)
            {
                if (frequencies.Count == 1) { }
                else if (item == '1')
                {
                    curr = curr.Right;
                }
                else if (item == '0')
                {
                    curr = curr.Left;
                }

                s.Append(item);

                if (curr.Right == null && curr.Left == null)
                {
                    decoded.Append(curr.Symbol);
                    binary.Add(s.ToString());    //~O(1)
                    s.Clear();                   //~O(1)
                    curr = root;
                }
            }
            return decoded.ToString();
        }
        public static void writeToFile(string filePath)     //~O(C)
        {
            //Calculate Frequncies : 1:abdsc , 2: zxyf....
            Dictionary<int, string> temp = new Dictionary<int, string>();
            foreach (var item in frequencies)       //~O(C)
            {
                if (temp.ContainsKey(item.Value))
                {
                    temp[item.Value] += item.Key;
                }
                else
                {
                    temp.Add(item.Value, item.Key.ToString());
                }
            }

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                int x = 0;
                foreach (var item in temp)      //~O(C)
                {
                    if (x == 0)
                    {
                        x++;
                        writer.Write(item.Key + BurrowsWheelerCompression.keyValueSeparator + item.Value);
                    }
                    else
                        writer.Write(BurrowsWheelerCompression.pairsSeparator + item.Key + BurrowsWheelerCompression.keyValueSeparator + item.Value);
                }
            }
        }
    }
}
