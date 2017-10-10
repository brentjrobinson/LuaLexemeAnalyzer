using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUALexemeAnalyzer
{
    class SyntaxAnalyzer
    {
        static Stack<Token> stateStack = new Stack<Token>();
        static Stack<int> sStack = new Stack<int>();
        static Stack<int> iStack = new Stack<int>();


        public static void ParseSyntax(List<List<Token>> t)
        {
            Console.WriteLine("Token Pass:");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = OpRun(t);
            Console.WriteLine("Operator Pass:");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = ArthExpressRun(t);
            Console.WriteLine("Arithmetic Expression Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = BoolExpressRun(t);
            Console.WriteLine("Boolean Expression Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = PrintAssignStateRun(t);
            Console.WriteLine("Print & Assignment Statement Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = GeneralStateRun(t);
            Console.WriteLine("General Statement Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();


            t = GenericStateRun(t);
            Console.WriteLine("Generic Statement Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();

            t = ProgramRun(t);
            Console.WriteLine("Program Statement Pass: ");
            ReadTokens(t);

            Console.WriteLine("Hit Enter Key For Next Pass");
            Console.ReadLine();
            Console.WriteLine();







        }

        private static void ReadTokens(List<List<Token>> t)
        {
            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                    Console.Write(t[i][s].ToString());
                Console.WriteLine();
            }

        }

        private static List<List<Token>> OpRun(List<List<Token>> t)
        {

            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "Symbol")
                    {
                        if (t[i][s].Val() == "+" || t[i][s].Val() == "-" || t[i][s].Val() == "/" || t[i][s].Val() == "*" || t[i][s].Val() == "%")
                            t[i][s] = new Token(TokenType.arithmetic_op, t[i][s].Val());
                        else if (t[i][s].Val() != "(" && t[i][s].Val() != ")" && t[i][s].Val() != "=")
                            t[i][s] = new Token(TokenType.relative_op, t[i][s].Val());
                        else if (t[i][s].Val() == "=")
                        {
                            t[i][s] = new Token(TokenType.assignment_operator, t[i][s].Val());
                        }

                    }
                }



            }


            return t;

        }

        private static List<List<Token>> ArthExpressRun(List<List<Token>> t)
        {


            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "Ident")  //Handles Variables
                    {
                        t[i][s] = new Token(TokenType.arithmetic_expression, t[i][s].Val());
                    }

                    if (t[i][s].Type() == "IntConst" || t[i][s].Type() == "CharConst" || t[i][s].Type() == "StrConst")
                        t[i][s] = new Token(TokenType.arithmetic_expression, t[i][s].Val());
                }
            }


            return t;

        }

        private static List<List<Token>> BoolExpressRun(List<List<Token>> t)
        {


            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "relative_op")
                    {
                        if (t[i][s - 1].Type() == "arithmetic_expression" && t[i][s + 1].Type() == "arithmetic_expression")
                        {
                            t[i][s] = new Token(TokenType.boolean_expression, (t[i][s - 1].Val() + " " + t[i][s].Val() + " " + t[i][s + 1].Val()));
                            t[i].RemoveAt(s + 1);
                            t[i].RemoveAt(s - 1);
                            s -= 1;
                            
                        }
                    }


                    if (t[i][s].Type() == "arithmetic_op" && t[i][s - 1].Type() == "arithmetic_expression" && t[i][s + 1].Type() == "arithmetic_expression")
                    {
                        t[i][s] = new Token(TokenType.arithmetic_expression, (t[i][s - 1].Val() + " " + t[i][s].Val() + " " + t[i][s + 1].Val()));
                        t[i].RemoveAt(s + 1);
                        t[i].RemoveAt(s - 1);
                        s -= 1;


                    }

                }
            }


            return t;

        }

        private static List<List<Token>> PrintAssignStateRun(List<List<Token>> t)
        {

            var temp = 0;
            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "assignment_operator" && !Int32.TryParse(t[i][s - 1].Val(), out temp))  //Handles Non Int Variables
                    {
                        if (t[i][s - 1].Type() == "arithmetic_expression" && t[i][s + 1].Type() == "arithmetic_expression")
                        {
                            t[i][s] = new Token(TokenType.assignment_statement, (t[i][s - 1].Val() + " " + t[i][s].Val() + " " + t[i][s + 1].Val()));
                            t[i].RemoveAt(s + 1);
                            t[i].RemoveAt(s - 1);
                            s-=1;
                           
                        }
                    }


                    if (t[i][s].Val() == "print")
                    {
                        if (t[i][s + 1].Val() == "(" && t[i][s + 2].Type() == "arithmetic_expression" && t[i][s + 3].Val() == ")")
                        {
                            t[i][s] = new Token(TokenType.print_statement, (t[i][s].Val() + t[i][s + 1].Val() + t[i][s + 2].Val() + t[i][s + 3].Val()));

                            t[i].RemoveAt(s + 3);
                            t[i].RemoveAt(s + 2);
                            t[i].RemoveAt(s + 1);
                            
                        }

                    }


                   





                }
            }

            return t;

        }
        private static List<List<Token>> GeneralStateRun(List<List<Token>> t)
        {

            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "Keyword" && (t[i][s].Val() == "REPEAT" || t[i][s].Val() == "WHILE" || t[i][s].Val() == "IF"))
                    {
                        stateStack.Push(t[i][s]);
                        sStack.Push(s);
                        iStack.Push(i);
                    }

                    else if (t[i][s].Type() == "Keyword" && (t[i][s].Val() == "UNTIL" && t[i][s].Type() == "boolean_expression") && stateStack.Count !=0)
                    {
                        s += 1;

                        var j = iStack.Peek();
                        var u = sStack.Peek();
                        string val = "";
                        while (!(j == i && s == u))
                            {
                            if (t[j].Count != 0)
                                val += t[j][u].Val() + " ";

                            u++;
                            if (t[j].Count <= u)
                            {
                                j++;
                                u = 0;
                            }
                        }

                        j = iStack.Pop();
                        u = sStack.Pop();
                        stateStack.Pop();

                        t[j][u] = new Token(TokenType.repeat_statement, val);

                        var topi = i;
                        var tops = s;

                        while (!(j == topi && u == tops))
                        {
                            if (t[topi].Count != 0)
                                t[topi].RemoveAt(tops);

                            tops--;
                            if (tops < 0)
                            {
                                topi -= 1;
                                tops = t[topi].Count - 1;
                            }
                        }


                    }
                    else if (t[i][s].Type() == "Keyword" && (t[i][s].Val() == "END")  && stateStack.Count != 0)
                    {
                        var j = iStack.Peek();
                        var u = sStack.Peek();
                        string val = "";
                        while (!(j == i && s == u))
                        {
                            if (t[j].Count != 0)
                                val += t[j][u].Val() + " ";

                            u++;
                            if (t[j].Count <= u)
                            {
                                j++;
                                u = 0;
                            }
                        }

                        j = iStack.Pop();
                        u = sStack.Pop();
                        stateStack.Pop();


                        if (t[j][u].Val() == "WHILE")
                            t[j][u] = new Token(TokenType.while_statement, val);

                        if (t[j][u].Val() == "IF")
                            t[j][u] = new Token(TokenType.if_statement, val);

    
                        var topi = i;
                        var tops = s;

                        while (!(j == topi && u == tops))
                        {
                            if (t[topi].Count != 0)
                                t[topi].RemoveAt(tops);

                            tops--;
                            if (tops < 0)
                            {
                                topi -= 1;
                                tops = t[topi].Count-1;
                            }
                        }



                    }

                    
                }
            }

            return t;
        }


        private static List<List<Token>> GenericStateRun(List<List<Token>> t)
        {
            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "repeat_statement" || t[i][s].Type() == "while_statement" || t[i][s].Type() == "if_statement" || t[i][s].Type() == "print_statement" || t[i][s].Type() == "assignment_statement")
                    {
                            t[i][s] = new Token(TokenType.statement, t[i][s].Val());

                    }
                }
            }

            return t;

        }

        private static List<List<Token>> ProgramRun(List<List<Token>> t)
        {
            for (int i = 0; i < t.Count; i++)
            {
                for (int s = 0; s < t[i].Count; s++)
                {
                    if (t[i][s].Type() == "Keyword" && t[i][s].Val() == "FUNCTION")
                    {
                        iStack.Push(i);
                        sStack.Push(s);
                        stateStack.Push(t[i][s]);
                    }

                    else if (t[i][s].Val() == "END")
                    {
                        var j = iStack.Peek();
                        var u = sStack.Peek();
                        string val = "";
                        while (!(j == i && s == u))
                            {
                            if(t[j].Count != 0)
                            val += t[j][u].Val() + " ";

                            u++;
                            if (t[j].Count <= u)
                            {
                                j++;
                                u = 0;
                            }
                        }

                        j = iStack.Pop();
                        u = sStack.Pop();


                            t[j][u] = new Token(TokenType.program, val);

                        var topi = i;
                        var tops = s;

                        while (!(j == topi && u == tops))
                        {
                            if(t[topi].Count !=0)
                            t[topi].RemoveAt(tops);

                            tops--;
                            if (tops < 0)
                            {
                                topi -= 1;
                                tops = t[topi].Count - 1;
                            }
                        }



                    }

                }
            }

            return t;

        }

    }
}
