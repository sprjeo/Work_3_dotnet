using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

class Tokenization : IEnumerable<string>, IDisposable
{
    private HashSet<char> _separators;
    private string _inputFile;
    private StreamReader _reader;
    

    public Tokenization(HashSet<char> separators, string inputFile)
    {
        _separators = separators ?? throw new ArgumentNullException(nameof(separators));
        _inputFile = inputFile ?? throw new ArgumentNullException(nameof(inputFile));
        _reader = new StreamReader(_inputFile);
    }
    ~Tokenization() {
        Dispose();
    }


    public IEnumerator<string> GetEnumerator()
    {
        string line;
        while ((line = _reader.ReadLine()) != null)
        {
            foreach (var token in Tokenize(line))
            {
                yield return token;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    private IEnumerable<string> Tokenize(string line)
    {
        var currentToken = string.Empty; 

        foreach (var symbol in line)
        {
            if (_separators.Contains(symbol))
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    yield return currentToken;
                    currentToken = string.Empty;
                }
            }
            else
            {
                currentToken += symbol;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
        {
            yield return currentToken;
        }
    }
    public void Dispose()
    {
        _reader?.Close();
        _reader = null;

        GC.SuppressFinalize(this);
    }
    
    
}

public class Program
{
    public static void Main()
    {
        HashSet<char> separators = new() { ' ', '.',  ',' };
        string inputFile = "input1.txt";

        Tokenization tokenizer = new(separators, inputFile);
        
            foreach (var token in tokenizer)
            {
                Console.WriteLine(token);
            }

    }

}
