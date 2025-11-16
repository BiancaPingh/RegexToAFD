using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    public static string ReadingRegexFromFile(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            //read char by char from file - to manage parenthesis correctly
            
            var patterns = new List<char>();
            bool isOpenParenthesis = false;
            using (StreamReader sr = new StreamReader(filePath))
            {
                int currentChar;
                while ((currentChar = sr.Read()) != -1)
                {
                    patterns.Add((char)currentChar);
                    if ((char)currentChar == '(')
                    {
                        isOpenParenthesis = true;
                    }
                    else if ((char)currentChar == ')')
                    {
                        isOpenParenthesis = false;
                        patterns.RemoveRange(patterns.Count - 2, 1); // remove the last added '.' before ')'
                    }
                    else if (isOpenParenthesis == true && (char.IsLetterOrDigit((char)currentChar)))
                    {
                        
                        patterns.Add('.');
                    }
                }
            }
            string[] strings = new string[patterns.Count];
            Console.WriteLine("Read regex patterns:");
            Console.WriteLine(string.Format(string.Join("", patterns)));
            return string.Format(string.Join("", patterns));
        }
        else
        {
            Console.WriteLine("File not found.");
            return string.Empty;
        }
    }

    public static string RewritingRegexPatterns(string patterns)
    {
        var rewrittenPatterns = new List<string>();
        Stack<char> bookStack = new Stack<char>();
        

        foreach (var pattern in patterns)
        {
            //daca e litera sau cifra se adauga direct in lista
            if (char.IsLetterOrDigit(pattern))
            {
                rewrittenPatterns.Add(pattern.ToString());
                
            }
            else
            {
                //first - see if the priority is correct for adding items to the stack
                if (pattern == '(')
                {
                    bookStack.Push(pattern);
                    
                }
                else if (pattern == ')')
                {
                    while (bookStack.Count > 0 && bookStack.Peek() != '(')
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    if (bookStack.Count > 0 && bookStack.Peek() == '(')
                    {
                        bookStack.Pop();
                    }
                }
                else if (pattern == '+' || pattern == '*') // both have the same priority, the biggest one
                {
                    bookStack.Push(pattern);
                }
                else if (pattern == '.')
                {

                    while (bookStack.Count > 0 && (bookStack.Peek() == '+' || bookStack.Peek() == '*'))
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    bookStack.Push(pattern);

                }
                else if (pattern == '|')
                {

                    while (bookStack.Count > 0 && (bookStack.Peek() == '+' || bookStack.Peek() == '*' || bookStack.Peek() == '.'))
                    {
                        rewrittenPatterns.Add(bookStack.Pop().ToString());
                    }
                    bookStack.Push(pattern);

                }
            }
        }
        while (bookStack.Count > 0)
        {
            rewrittenPatterns.Add(bookStack.Pop().ToString());
        }
        return string.Format(string.Join("", rewrittenPatterns));
    }

    static void Main(string[] args)
    {
       string initialString = ReadingRegexFromFile("C:\\Users\\pingh\\RegexToAFD\\RegexToAFD\\regex_patterns.txt");
       string rewrittenString = RewritingRegexPatterns(initialString);
         Console.WriteLine("Rewritten regex patterns:");
         Console.WriteLine(rewrittenString);
    }
}
