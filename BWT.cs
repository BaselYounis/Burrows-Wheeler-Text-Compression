using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burrows_Wheeler_Compression
{
    class BWT
    {
        public static int n = 10000;
        public static int maxDigit = n.ToString().Length;
        public static char seperator = '|';
        public static char nullChar = '\0';//ascii 0
        public static void print(StringBuilder text)
        {
            for (global::System.Int32 i = 0; i < text.Length; i++)
            {
                if (text[i] == nullChar)
                {
                    Console.WriteLine("NULL CHAR");
                    continue;
                }
                else if (text[i] == ' ')
                {
                    Console.WriteLine("Whitespace");
                    continue;
                }
                else if (text[i] == '\n')
                {
                    Console.WriteLine("Indentation");
                    continue;
                }
                else if (text[i] == '\t')
                {
                    Console.WriteLine("SPACE");
                    continue;
                }
                else if (text[i] == '\r')
                {
                    Console.WriteLine("Carriage Return");
                    continue;
                }
                Console.WriteLine(text[i]);
            }
        }
        public static int getNthDigitFromRight(int number, int digitPlace)
        {
            string temp = number.ToString();
            if (digitPlace > temp.Length)
                return 0;
            char ret = temp[temp.Length - digitPlace];
            return ret - '0';
        }
        public static Suffix[] sortByRankViaCounting(Suffix[] arr, int valueIndex)
        {
            int max = int.MinValue;
            for (global::System.Int32 i = 0; i < arr.Length; i++)
            {
                if (valueIndex == 1)
                {
                    if (arr[i].currentRank > max)
                        max = arr[i].currentRank;
                }
                else if (valueIndex == 2)
                {
                    if (arr[i].adjRank > max)
                        max = arr[i].adjRank;
                }

            }
            int[] frequency = new int[max + 1];
            for (global::System.Int32 i = 0; i < frequency.Length; i++)
            {
                frequency[i] = 0;
            }
            for (int i = 0; i < arr.Length; i++)
            {
                if (valueIndex == 1)
                    frequency[arr[i].currentRank]++;
                else
                    frequency[arr[i].adjRank]++;
            }
            for (int i = 1; i <= max; i++)
            {
                frequency[i] += frequency[i - 1];
            }


            Suffix[] result = new Suffix[arr.Length];
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (valueIndex == 1)
                {
                    result[frequency[arr[i].currentRank] - 1] = arr[i];
                    frequency[arr[i].currentRank]--;
                }
                else
                {
                    result[frequency[arr[i].adjRank] - 1] = arr[i];
                    frequency[arr[i].adjRank]--;
                }
            }

            return result;

        }

        public static Suffix[] generateSuffixArray(StringBuilder text)
        {
            Suffix[] rotations = new Suffix[text.Length];
            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = new Suffix(i + 1);
                rotations[i].currentRank = rotations[i].valueAccess(text, 0);
                rotations[i].currentRankBeforeChange = rotations[i].currentRank;
                rotations[i].adjRank = (int)nullChar;
            }
            return rotations;
        }
        public static Suffix[] sortSuffixes(Suffix[] rotations, StringBuilder text)
        {
            Dictionary<int, int> lengthToCurrentRank = new Dictionary<int, int>();
            rotations = sortByRankViaCounting(rotations, 2);
            rotations = sortByRankViaCounting(rotations, 1);
            /*sortByRankViaRadix(rotations, text, 0, text.Length, 2);
            sortByRankViaRadix(rotations, text, 0, text.Length, 1);*/
            for (int i = 0; i < rotations.Length; i++)
            {
                lengthToCurrentRank.Add(rotations[i].length, rotations[i].currentRank);
            }
            for (int j = 1; j < text.Length / 2; j = j * 2)//log n * n 
            {
                int rank = 1;
                rotations[0].currentRank = rank;
                lengthToCurrentRank[rotations[0].length] = rotations[0].currentRank;
                for (int i = 1; i <= rotations.Length; i++)//o(n)
                {
                    if (i == rotations.Length)
                    {
                        rotations[i - 1].currentRankBeforeChange = rotations[i - 1].currentRank;
                        continue;
                    }
                    if (rotations[i].currentRank == rotations[i - 1].currentRankBeforeChange && rotations[i].adjRank == rotations[i - 1].adjRank)
                    {
                        rotations[i].currentRank = rank;

                    }
                    else
                    {
                        rank++;
                        rotations[i].currentRank = rank;

                    }
                    rotations[i - 1].currentRankBeforeChange = rotations[i - 1].currentRank;
                    lengthToCurrentRank[rotations[i].length] = rotations[i].currentRank;
                }
                for (int i = 0; i < rotations.Length; i++)
                {
                    int key = rotations[i].length - j;

                    if (key <= 0)
                    {
                        rotations[i].adjRank = (int)nullChar;//0
                        continue;
                    }
                    rotations[i].adjRank = lengthToCurrentRank[key];
                }
                rotations = sortByRankViaCounting(rotations, 2);//sort according to adjrank
                rotations = sortByRankViaCounting(rotations, 1);//sort according to rank
                /*sortByRankViaRadix(rotations, text, 0, text.Length, 2);
                sortByRankViaRadix(rotations, text, 0, text.Length, 1);*/

            }

            return rotations;
        }
        public static StringBuilder Transform(StringBuilder text)
        {
            text.Append(nullChar);//append the null character to make it the unique character...

            Suffix[] suffixes = generateSuffixArray(text);
            Stopwatch watch = new Stopwatch();
            //watch.Start();
            suffixes = sortSuffixes(suffixes, text);
            //watch.Stop();
            StringBuilder retVal = new StringBuilder("");


            for (int i = 0; i < suffixes.Length; i++)
            {
                retVal.Append(suffixes[i].lastLetter(text));
            }

            //long time=watch.ElapsedMilliseconds;

            // Console.WriteLine("Transformation Loop time : "+ time.ToString());

            return retVal;
        }
        public static StringBuilder fastTransform(StringBuilder text)
        {

            int partitions = text.Length / n;
            int remainder = text.Length % n;
            int remIdx = text.Length - remainder;
            if (text.Length < n)
                return Transform(text).Append(seperator);
            StringBuilder retVal = new StringBuilder("");
            for (int i = 0; i < partitions; i++)
            {
                StringBuilder sub = new StringBuilder("");
                for (global::System.Int32 j = 0; j < n; j++)
                {
                    sub.Append(text[i * n + j]);
                }
                sub = Transform(sub);
                sub.Append(seperator);
                retVal.Append(sub);
            }
            if (remainder > 0)
            {
                StringBuilder sub = new StringBuilder("");
                for (global::System.Int32 i = 0; i < remainder; i++)
                {
                    sub.Append(text[remIdx + i]);
                }
                sub = Transform(sub);
                sub.Append(seperator);
                retVal.Append(sub);

            }
            return retVal;

        }

        public static Letter[] generateLetterArray(char[] text, int size)
        {
            Dictionary<char, int> subscripts = new Dictionary<char, int>();
            for (int i = 0; i < 256; i++)
            {
                subscripts.Add((char)i, 0);//making a dictionary of all possible chars with number of times seen set to 0...
            }
            Letter[] letters = new Letter[size];
            for (int i = 0; i < size; i++)
            {
                letters[i] = new Letter(text[i], subscripts[text[i]]);
                subscripts[text[i]]++;
            }


            return letters;
        }
        public static void Inverse(char[] text, int actualSize)
        {
            Dictionary<string, int> beforeSorting = new Dictionary<string, int>();



            Letter[] letters = generateLetterArray(text, actualSize);//O(n)


            Letter[] lettersAfterSorting = Letter.sortLettersViaCounting(letters);//O(n)


            for (int i = 0; i < actualSize; i++)//O(n)
            {
                string str = letters[i].symbol.ToString() + letters[i].subscript.ToString();
                beforeSorting.Add(str, i);

            }



            int idx = 0;

            Letter currentKey = lettersAfterSorting[0];
            string keyString = currentKey.symbol.ToString() + currentKey.subscript.ToString();
            //go to second array using this key
            int index = beforeSorting[keyString];
            text[idx] = lettersAfterSorting[index].symbol;
            idx++;
            while (idx < actualSize)
            {
                currentKey = lettersAfterSorting[index];
                keyString = currentKey.symbol.ToString() + currentKey.subscript.ToString();
                index = beforeSorting[keyString];
                text[idx] = lettersAfterSorting[index].symbol;
                idx++;

            }




        }
        public static StringBuilder fastInverse(StringBuilder text)
        {
            char[] result = new char[text.Length];//text length is more than original length...
            int resultIdx = 0;
            char[] sub = new char[n + 1];
            int subIdx = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == seperator)
                {

                    Inverse(sub, subIdx);
                    for (global::System.Int32 j = 0; j < subIdx - 1; j++)
                    {

                        result[resultIdx] = sub[j];
                        resultIdx++;
                    }
                    subIdx = 0;
                    continue;
                }

                sub[subIdx] = text[i];
                subIdx++;
            }
            StringBuilder retVal = new StringBuilder(resultIdx);
            
            for (int i = 0; i < resultIdx; i++)
            {

                retVal.Append(result[i]);
            }
            
          
            return retVal;
        }
    }
    class Letter
    {
        public char symbol;
        public int value;
        public int subscript;
        public Letter(char symbol, int subscript)
        {
            this.symbol = symbol;
            this.value = Convert.ToInt32(symbol);
            this.subscript = subscript;
        }
        public static Letter[] sortLettersViaCounting(Letter[] arr)
        {
            int max = int.MinValue;
            for (global::System.Int32 i = 0; i < arr.Length; i++)
            {
                if (arr[i].value > max)
                    max = arr[i].value;

            }
            int[] frequency = new int[max + 1];
            for (global::System.Int32 i = 0; i < frequency.Length; i++)
            {
                frequency[i] = 0;
            }
            for (int i = 0; i < arr.Length; i++)
            {

                frequency[arr[i].value]++;

            }
            for (int i = 1; i <= max; i++)
            {
                frequency[i] += frequency[i - 1];
            }


            Letter[] result = new Letter[arr.Length];
            for (int i = arr.Length - 1; i >= 0; i--)
            {

                result[frequency[arr[i].value] - 1] = arr[i];
                frequency[arr[i].value]--;

            }

            return result;
        }

    }
    class Suffix
    {

        public int length;
        public int currentRank;
        public int adjRank;
        public int currentRankBeforeChange;

        public Suffix(int length)
        {
            if (length <= 0)
            {
                Console.WriteLine("WRONG SUFFIX LENGTH!");
            }
            this.length = length;

        }
        public char traditionalAccess(StringBuilder text, int index)
        {
            return text[index];
        }
        public int traditionalValueAccess(StringBuilder text, int index)
        {
            char letter = traditionalAccess(text, index);
            return Convert.ToInt32(letter);

        }
        public char access(StringBuilder text, int index)
        {
            int edge = text.Length - 1;

            if (index < this.length)
                return text[edge - this.length + index + 1];
            else
                return text[index - this.length];

        }
        public char lastLetter(StringBuilder text)
        {
            return this.access(text, text.Length - 1);
        }
        public int valueAccess(StringBuilder text, int index)
        {
            char letter = this.access(text, index);

            if (index + 1 > this.length)
            {
                return (int)BWT.nullChar;
            }
            return Convert.ToInt32(letter);
        }
        public void printSuffix(StringBuilder text)
        {
            string currentSuffix = "";
            for (int i = 0; i < this.length; i++)
            {
                currentSuffix += this.access(text, i);
            }
            Console.WriteLine(currentSuffix);
        }
        public void printRotation(StringBuilder text)
        {

            string currentSuffix = "";

            for (int i = 0; i < text.Length; i++)
            {
                currentSuffix += this.access(text, i);
            }
            Console.WriteLine(currentSuffix);
        }

    }
}
