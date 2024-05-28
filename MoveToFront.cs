using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_Wheeler_Compression
{
    internal class MoveToFront
    {
        public static string EncodeWithMTF(string input)    //~O(N)
        {
            StringBuilder encoded = new StringBuilder();
            List<char> alphabet = Enumerable.Range(0, 256).Select(i => (char)i).ToList();
            foreach (char c in input)
            {
                int index = alphabet.IndexOf(c);
                char x = (char)index;
                encoded.Append(x);

                char temp = alphabet[index];
                alphabet.RemoveAt(index);
                alphabet.Insert(0, temp);
            }
            return encoded.ToString();  //~O(N)
        }
        static List<int> CreateSymbols()    //~O(1)
        {
            List<int> symbols = Enumerable.Range(0, 256).ToList();
            return symbols;
        }
        public static string DecodeWithMTF(string text)     //~O(N)
        {

            List<int> asciiValues = text.Select(c => (int)c).ToList();

            List<int> symbol = CreateSymbols();
            List<int> outputDecoding = new List<int>(asciiValues.Count);

            foreach (int asciiValue in asciiValues)     //~O(N)
            {
                int targetChar = symbol[asciiValue];
                outputDecoding.Add(targetChar);
                symbol.Remove(targetChar);
                symbol.Insert(0, targetChar);
            }


            string decodedText = new string(outputDecoding.Select(i => (char)i).ToArray()); //~O(N)
            return decodedText;
        }
    }
}
