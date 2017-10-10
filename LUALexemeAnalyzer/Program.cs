// By Nicholas Pound, Brent Robinson, and Jermey Taylor

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUALexemeAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Enter File Path to LUA File");
            string s = Console.ReadLine();
            var luaTokens =  Analyzer.ParseLua(s);
            SyntaxAnalyzer.ParseSyntax(luaTokens);

            Console.ReadLine();
        }
    }
}
