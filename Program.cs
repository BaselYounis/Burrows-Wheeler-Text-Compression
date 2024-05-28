using Burrows_Wheeler_Compression;
using System.Diagnostics;
using System.Text;

class BurrowsWheelerCompression
{
    public static string pairsSeparator = "";
    public static string keyValueSeparator = "";
    public static string treeSeparator = "";
    public static string lastValueSeparator = "þ";

    static int byteSize = 7;
    private static string compress(string data, ref string lastValue)     //~O(N)
    {
        string[] binaryValues = new string[data.Length / byteSize];
        int it = 0;
        StringBuilder x = new StringBuilder();
        for (int i = 0; i < data.Length; i++)   //~O(N)
        {
            x.Append(data[i]);
            if ((i + 1) % byteSize == 0)
            {
                binaryValues[it++] = x.ToString();
                x.Clear();
            }
            if (i + 1 == data.Length && x.Length < byteSize)
            {
                lastValue = x.ToString();    //~O(byteSize) ~= O(1)
            }
        }

        int decimalValue;
        StringBuilder asciiChar = new StringBuilder();
        //Convert Binary To ASCII
        foreach (string binaryValue in binaryValues)    //~O(N) N/7
        {
            decimalValue = Convert.ToInt32(binaryValue, 2);     //~O(1)     N = 7
            asciiChar.Append((char)decimalValue);
        }
        return asciiChar.ToString();
    }
    private static string deCompress(string text, string lastValue)    //~O(N)
    {
        StringBuilder data = new StringBuilder();
        for (int i = 0; i < text.Length; i++)    //~O(N)
        {
            int asciiValue = (int)text[i];
            string binaryValue = Convert.ToString(asciiValue, 2);   //~O(Log N) N = asciiValue
            if (i == text.Length)
                data.Append(binaryValue);
            else
                data.Append(binaryValue.PadLeft(byteSize, '0'));
        }

        data.Append(lastValue);
        return data.ToString();
    }
    static void readTestFile(string filePath, StringBuilder str)
    {
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                line = reader.ReadToEnd();
                str.Append(line);
            }
        }
        else
        {
            Console.WriteLine("File not found!");
        }
    }
    static void writeCompressed(string filePath, string data, string lastValue)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(data);
            writer.Write(lastValueSeparator);
            if (lastValue.Length == 0)
                writer.Write('\0');
            else
                writer.Write(lastValue);
            writer.Write(treeSeparator);
        }
    }
    static void readCompressed(string filePath, ref string compressed, ref string lastValue) //~O(N)
    {
        string data;
        using (StreamReader reader = new StreamReader(filePath))
        {
            data = reader.ReadToEnd();
        }
        StringBuilder text = new StringBuilder();
        StringBuilder lvalue = new StringBuilder();
        int x = 0;

        for (int i = 0; i < data.Length; i++)   //~O(N)
        {
            if (x == 0 && data[i].ToString() == lastValueSeparator)
            {
                x++;
                continue;
            }
            else if (x == 1 && data[i].ToString() == treeSeparator)
            {
                x++;
                break;
            }
            if (x == 0 && data[i].ToString() != lastValueSeparator)
            {
                text.Append(data[i]);
            }
            else if (x == 1 && data[i].ToString() != treeSeparator)
            {
                lvalue.Append(data[i]);
            }
        }
        compressed = text.ToString();
        lastValue = lvalue.ToString();
        string tree = data.Substring((text.Length + lvalue.Length) + 2);
        calculateFrequencies(tree);     //~O(N)
    }
    private static void calculateFrequencies(string tree)   //~O(N) N = Distinct Values 
    {
        //Calculate frequencies from File
        Dictionary<char, int> frequencies = new Dictionary<char, int>();
        string[] pairs = tree.Split(new[] { pairsSeparator }, StringSplitOptions.None); //~O(N) Assume Worst pairsLength = 256 & each Pair is 1:a , 2:b , 3:x ...
        foreach (string pair in pairs)  //~O(N)
        {
            string[] kvp = pair.Split(new[] { keyValueSeparator }, StringSplitOptions.None);    //~O(1)
            char[] chars = kvp[1].ToCharArray();    //~O(1)
            foreach (var item in chars)     //~O(1)
            {
                frequencies.Add(item, int.Parse(kvp[0]));
            }
        }
        var sortedDictionary = frequencies.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value); //~O(N Log N)
        HuffmanCoding.frequencies = sortedDictionary;
    }
    static void writeDecompressed(string filePath, StringBuilder data)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(data);
        }
    }
    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        StringBuilder str = new StringBuilder();
        string testFile = "dickens.txt";
        string testPath = @"..\..\..\..\Test Files\" + testFile;
        string compressedFile = @"..\..\..\..\Output\Compressed.bin";
        string deCompressedFile = @"..\..\..\..\Output\Decompressed.txt";
        double compressionTime = 0, decompressionTime;

        readTestFile(testPath, str);

        stopwatch.Start();
        //Burrows - Wheeler Transformation
        StringBuilder transformed = BWT.fastTransform(str);
        Console.WriteLine("Transformed with BWT : \n");

        //Move To Front Encoding
        string EncodedWithMTF = MoveToFront.EncodeWithMTF(transformed.ToString());
        Console.WriteLine("Encoded With MTF : \n");

        //Huffman Encoding
        string encodedWithHuffman = HuffmanCoding.Encode(EncodedWithMTF);
        Console.WriteLine("Encoded With Huffman : \n");

        //Compression
        string lastvalue = "";
        string compressed = compress(encodedWithHuffman, ref lastvalue);
        Console.WriteLine("Compressed : \n");
        stopwatch.Stop();

        compressionTime = stopwatch.Elapsed.TotalSeconds;

        //Write to File
        writeCompressed(compressedFile, compressed, lastvalue);
        HuffmanCoding.writeToFile(compressedFile);
        Console.WriteLine("Wrote To File : \n");

        //Read Compressed File
        string comp = "",lvalue = "";
        readCompressed(compressedFile, ref comp, ref lvalue);
        Console.WriteLine("Read Compressed : \n");

        stopwatch.Restart();
        //Decompression
        string decompressed = deCompress(comp, lvalue);
        Console.WriteLine("Decompressed : \n");

        //Huffman Decoding
        string DecodedWithHuffman = HuffmanCoding.Decode(decompressed);
        Console.WriteLine("Decoded With Huffman : \n");

        //Move To Front Decoding
        string DecodedWithMTF = MoveToFront.DecodeWithMTF(DecodedWithHuffman);
        Console.WriteLine("Decoded With MTF: \n");


        //Burrows - Wheeler Inverse Transformation
        StringBuilder inverted = BWT.fastInverse(new StringBuilder(DecodedWithMTF));
        Console.WriteLine("Inverted With BWT : \n");
        stopwatch.Stop();

        decompressionTime = stopwatch.Elapsed.TotalSeconds;

        //Write text After Decompression
        writeDecompressed(deCompressedFile, inverted);



        double compressionRatio = str.Length / compressionTime;


        Console.WriteLine("\nEvaluation : ");
        Console.WriteLine("Test File : " + testFile);
        Console.WriteLine("Compression Time   = " + compressionTime + " sec");
        Console.WriteLine("Decompression Time = " + decompressionTime + " sec");
        Console.WriteLine("Total Execution Time = " + (compressionTime + decompressionTime) + " sec");
        Console.WriteLine("Compression Ratio  ~= " + Math.Ceiling(compressionRatio) + " char/sec");


    }
}