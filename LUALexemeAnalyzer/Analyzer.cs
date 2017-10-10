// By Nicholas Pound, Brent Robinson, and Jermey Taylor


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace LUALexemeAnalyzer
{
    public static class Analyzer
    {
        public static char[] IllegalCharacters = new char[] { '.' };

        public static List<List<Token>> ParseLua(string filepath)
        {

            using (StreamReader sr = new StreamReader(filepath))
            {
                var LUAContent = new List<List<Token>>();
                while(!sr.EndOfStream)
                {
                    LUAContent.Add(Advance(sr.ReadLine()));
                }
                return LUAContent;
            }
        }



        public static List<Token> Advance(string lua)
        {
            if (lua == string.Empty)
                return new List<Token>();

            if (lua.Contains("--")) //removes commends
            {
                int del = lua.IndexOf("--");
                lua = lua.Remove(del);
            }
            lua = EatWhitespace(lua);


            List<Token> parsed = new List<Token>();

            for (int i = 0; i < lua.Length; i++)
            {
               

                char nextChar = lua[i];
                if (Syntax.IsSymbol(nextChar))
                {
                    //This token is going to be a symbol. There are
                    //three special look-ahead cases for '<=', '>=', 
                    //and '!='.
                    if ((new[] { '<', '>', '!' }.Contains(nextChar))
                        && lua[i + 1] == '=')
                    {

                        parsed.Add( new Token(TokenType.Symbol, nextChar + "="));
                        i = NextChar(i);//Eat the '='
                        nextChar = lua[i];
                    }
                    else
                    {
                        parsed.Add(new Token(TokenType.Symbol, nextChar.ToString()));
                    }
                }
                else if (Syntax.IsNumber(nextChar))
                {
                    //This token is going to be an integer constant.
                    string intConst = nextChar.ToString();

                    if (!(lua.Length <= NextChar(i)))
                        while (Syntax.IsNumber(lua[NextChar(i)]))
                        {
                            i = NextChar(i);//Eat the next int
                            nextChar = lua[i];
                            intConst += nextChar.ToString();
                            if (i + 1 >= lua.Length)
                                break;
                        }


                    int result;
                    if (!int.TryParse(intConst, out result))
                    {
                        throw new CompilationException(
                        "Int const must be in range [0,2147483648), " +
                        "but got: " + intConst, lua);
                    }

                    parsed.Add(new Token(
                        TokenType.IntConst, intConst));
                }

                else if (Syntax.IsStringConstantStart(nextChar))
                {
                    //This token is going to be a string constant.
                    string strConst = "";

                    i = NextChar(i);//Eat the "
                    nextChar = lua[i];

                    while (nextChar != '"')
                    {
                        strConst += nextChar;
                        i = NextChar(i);
                        nextChar = lua[i];
                    }



                    parsed.Add(new Token(
                        TokenType.StrConst, strConst));
                }

                else if (Syntax.IsCharConstantStart(nextChar))
                {
                    i = NextChar(i);
                    nextChar = lua[i];

                    char marker = nextChar;
                    if (marker == '\\')
                    {

                        string code = "";
                        if (!(lua.Length <= NextChar(i)))
                            while (Syntax.IsNumber(lua[NextChar(i)]))
                            {
                                i = NextChar(i);//Eat the next int
                                nextChar = lua[i];
                                code += nextChar.ToString();
                                if (i + 1 >= lua.Length)
                                    break;
                            }


                        if (code.Length != 3)
                        {
                            throw new CompilationException(
                            "Expected: \\nnn where n are decimal digits",
                            lua);
                        }
                        int value = int.Parse(code);
                        if (value >= 256)
                        {
                            throw new CompilationException(
                            "Character ordinal is out of range [0,255]",
                            lua);
                        }
                        parsed.Add(new Token(
                            TokenType.IntConst, value.ToString()));
                    }
                    else
                    {
                        parsed.Add(new Token(
                            TokenType.CharConst, ((int)marker).ToString()));
                    }
                    i = NextChar(i);
                    nextChar = lua[i];//Swallow the end of the character ordinal

                    if(nextChar!='\'')
                        {
                            throw new CompilationException(
                            "Character is not a char",
                            lua);
                        }

                }
                else
                {
                    string id = "";

                    #region And
                    if (nextChar == 'a' && lua.Length > i + 2)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "and")
                        {
                            i += 2;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "AND"));
                        }
                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region BREAK

                    else if (nextChar == 'b' && lua.Length > i + 4)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "break")
                        {
                            i += 4;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "BREAK"));
                        }
                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region DO
                    else if (nextChar == 'd' && lua.Length > i + 1)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() == "do")
                        {
                            i += 1;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "DO"));
                        }
                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region ELSE
                    else if (nextChar == 'e' && lua.Length > i + 2)
                    {
                        #region 6
                        if (nextChar == 'e' && lua.Length > i + 5)
                        {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() + lua[i + 5].ToString() == "elseif")
                            {
                                i += 5;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "ELSEIF"));
                            }
                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() == "else")
                            {
                                i += 3;
                                nextChar = lua[i];
                                parsed.Add(new Token(
                            TokenType.Keyword, "ELSE"));
                            }
                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "end")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "END"));
                            }
                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, lua));
                                i--;
                                nextChar = lua[i];
                            }
                        }
                        #endregion
                        #region 4
                        else if (nextChar == 'e' && lua.Length > i + 3)
                        {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() == "else")
                            {
                                i += 3;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "ELSE"));
                            }
                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "end")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "END"));
                            }

                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, id));
                                i--;
                                nextChar = lua[i];
                            }
                        }
                        #endregion
                        #region 3
                        else
                        {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "end")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "END"));
                            }
                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, id));
                                i--;
                                nextChar = lua[i];
                            }
                        }
                    }
                    #endregion
                    #endregion
                    #region FOR
                    else if (nextChar == 'f' && lua.Length > i + 2)
                    {
                        #region 8
                        if (nextChar == 'f' && lua.Length > i + 7)
                        {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() + lua[i + 5].ToString() + lua[i + 6].ToString() + lua[i + 7].ToString() == "function")
                            {
                                i += 7;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FUNCTION"));
                            }

                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "false")
                            {
                                i += 4;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FALSE"));
                            }

                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "for")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FOR"));
                            }

                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, id));
                                i--;
                                nextChar = lua[i];
                            }
                        }

                        #endregion
                        #region 5
                        else if (nextChar == 'f' && lua.Length > i + 4)
                        {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "false")
                            {
                                i += 4;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FALSE"));
                            }
                            else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "for")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FOR"));
                            }

                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, id));
                                i--;
                                nextChar = lua[i];
                            }

                        }
                        #endregion
                        #region 3
                        else {
                            if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "for")
                            {
                                i += 2;
                                nextChar = lua[i];
                                parsed.Add( new Token(
                            TokenType.Keyword, "FOR"));
                            }

                            else
                            {
                                while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                                {
                                    id += nextChar;
                                    i = NextChar(i);
                                    nextChar = lua[i];
                                }
                                parsed.Add( new Token(
                        TokenType.Ident, id));
                                i--;
                                nextChar = lua[i];
                            }
                        }
                        #endregion
                    }

                    #endregion
                    #region IF
                    else if (nextChar == 'i' && lua.Length > i + 1)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() == "if")
                        {
                            i += 1;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "IF"));
                        }

                        else if (nextChar.ToString() + lua[i + 1].ToString() == "in")
                        {
                            i += 1;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "IN"));
                        }

                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region LOCAL
                    else if (nextChar == 'l' && lua.Length > i + 4)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "local")
                        {
                            i += 4;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "LOCAL"));
                        }
                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region NIL
                    else if (nextChar == 'n' && lua.Length > i + 2)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "nil")
                        {
                            i += 2;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "NIL"));
                        }

                        else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() == "not")
                        {
                            i += 2;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "NOT"));
                        }

                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region OR
                    else if (nextChar == 'o' && lua.Length > i + 1)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() == "or")
                        {
                            i += 1;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "OR"));
                        }
                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }

                    #endregion
                    #region REPEAT
                    else if (nextChar == 'r' && lua.Length > i + 5)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() + lua[i + 5].ToString() == "repeat")
                        {
                            i += 5;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "REPEAT"));
                        }

                        else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() + lua[i + 5].ToString() == "return")
                        {
                            i += 5;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "RETURN"));
                        }



                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region THEN
                    else if (nextChar == 't' && lua.Length > i + 3)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() == "then")
                        {
                            i += 3;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "THEN"));
                        }

                        else if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() == "true")
                        {
                            i += 3;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "TRUE"));
                        }

                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion
                    #region UNTIL
                    else if (nextChar == 'u' && lua.Length > i + 4)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "until")
                        {
                            i += 4;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "UNTIL"));
                        }

                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }

                    }
                    #endregion
                    #region WHILE
                    else if (nextChar == 'w' && lua.Length > i + 4)
                    {
                        if (nextChar.ToString() + lua[i + 1].ToString() + lua[i + 2].ToString() + lua[i + 3].ToString() + lua[i + 4].ToString() == "while")
                        {
                            i += 4;
                            nextChar = lua[i];
                            parsed.Add( new Token(
                        TokenType.Keyword, "WHILE"));
                        }

                        else
                        {
                            while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                            {
                                id += nextChar;
                                i = NextChar(i);
                                nextChar = lua[i];
                            }
                            parsed.Add( new Token(
                    TokenType.Ident, id));
                            i--;
                            nextChar = lua[i];
                        }
                    }
                    #endregion


                    else
                    {
                        while (!Syntax.IsSymbol(nextChar) && (i + 1) < lua.Length)
                        {
                            id += nextChar;
                            i = NextChar(i);
                            nextChar = lua[i];
                        }
                        parsed.Add(new Token(
                TokenType.Ident, id));
                        i--;
                        nextChar = lua[i];
                    }
                }
            }
            
            return parsed;
        }

        private static int NextChar(int i)
        {
            i += 1;
            return i;
        }

        private static string EatWhitespace(string lua)
        {
            while (lua.Contains(" "))
                lua = lua.Replace(" ", "");
            return lua;

        }


    }

    internal class Syntax
    {
        internal static bool IsCharConstantStart(char nextChar)
        {
            if (nextChar == '\'')
                return true;
            return false;
        }

        internal static bool IsNumber(char nextChar)
        {
            int gb;
            if (Int32.TryParse(nextChar.ToString(), out gb))
                return true;
            return false;
        }


        internal static bool IsStringConstantStart(char nextChar)
        {
            if (nextChar == '"')
                return true;
            return false;
        }

        internal static bool IsSymbol(char nextChar)
        {
            if (nextChar == '<' || nextChar == '>' || nextChar == '!' || nextChar == '+' || nextChar == '=' || nextChar == '-' || nextChar == '*' || nextChar == '/' || nextChar == '%' || nextChar == '^' || nextChar == '#' || nextChar == '~' || nextChar == '(' || nextChar == ')')
                return true;
            return false;      
        }
    }

    public class Token
    {
        private object symbol;
        private string v;

        public Token(object symbol, string v)
        {
            this.symbol = symbol;
            this.v = v;
        }

        public string Type()
        { return symbol.ToString(); }

        public string Val()
        { return v.ToString(); }

        public override string ToString()
        {
            return "<" + symbol + ", " + v + ">";
        }
    }

    enum TokenType
    {
Symbol,
Keyword,
Ident,
        IntConst,
        StrConst,
        CharConst,
            arithmetic_op,
            arithmetic_expression,
            relative_op,
            boolean_expression,
        print_statement,
        assignment_operator,
        assignment_statement,
        block,
        repeat_statement,
        while_statement,
        if_statement,
        statement,
        program





    }
}
