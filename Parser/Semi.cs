using CStoker;
/////////////////////////////////////////////////////////////////////////
// Semi.cs   -  Builds semiExpressions                                 //
// Author:         Pramukh Shenoy                                      //
// Original Code:  Jim Fawcett, CST 4-187, Syracuse University         //
//              (315) 443-3948, jfawcett@twcny.rr.com                  //
/////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * Semi provides, via class CSemiExp, facilities to extract semiExpressions.
 * A semiExpression is a sequence of tokens that is just the right amount
 * of information to parse for code analysis.  SemiExpressions are token
 * sequences that end in "{" or "}" or ";"
 * 
 * CSemiExp works with a private CToker object attached to a specified file.
 * It provides a get() function that extracts semiExpressions from the file
 * while filtering out comments and merging quotes into single tokens.
 * 
 * Public Interface
 * ================
 * CSemiExp semi = new CSemiEx;();    // constructs CSemiExp object
 * if(semi.open(fileName)) ...        // attaches semi to specified file
 * semi.close();                      // closes file stream
 * if(semi.Equals(se)) ...            // do these semiExps have same tokens?
 * int hc = semi.GetHashCode()        // returns hashcode
 * if(getSemi()) ...                  // extracts and stores next semiExp
 * int len = semi.count;              // length property
 * semi.verbose = true;               // verbose property - shows tokens
 * string tok = semi[2];              // access a semi token
 * string tok = semi[1];              // extract token
 * semi.flush();                      // removes all tokens
 * semi.initialize();                 // adds ";" to empty semi-expression
 * semi.insert(2,tok);                // inserts token as third element
 * semi.Add(tok);                     // appends token
 * semi.Add(tokArray);                // appends array of tokens
 * semi.display();                    // sends tokens to Console
 * string show = semi.displayStr();   // returns tokens as single string
 * semi.returnNewLines = false;       // property defines newline handling
 *                                    //   default is true
 */
//
/*
 * Build Process
 * =============
 * Required Files:
 *   Semi.cs Toker.cs
 * 
 * Compiler Command:
 *   csc /target:exe /define:TEST_SEMI Semi.cs Toker.cs
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace CSsemi
{
    ///////////////////////////////////////////////////////////////////////
    // class CSemiExp - filters token stream and collects semiExpressions

    public class CSemiExp
    {
        CToker toker = null;
        List<string> semiExp = null;
        string currTok = "";
        string prevTok = "";

        //----< line count property >----------------------------------------

        public int lineCount
        {
            get { return toker.lineCount; }
        }
        //----< constructor >------------------------------------------------

        public CSemiExp()
        {
            toker = new CToker();
            semiExp = new List<string>();
            discardComments = true;  // not implemented yet
            returnNewLines = true;
            displayNewLines = false;
        }

        //----< test for equality >------------------------------------------

        override public bool Equals(Object semi)
        {
            CSemiExp temp = (CSemiExp)semi;
            if (temp.count != this.count)
                return false;
            for (int i = 0; i < temp.count && i < this.count; ++i)
                if (this[i] != temp[i])
                    return false;
            return true;
        }

        //---< pos of first str in semi-expression if found, -1 otherwise >--

        public int FindFirst(string str)
        {
            for (int i = 0; i < count; ++i)
                if (this[i] == str)
                    return i;
            return -1;
        }
        //---< pos of last str in semi-expression if found, -1 otherwise >--- 

        public int FindLast(string str)
        {
            for (int i = this.count - 1; i >= 0; --i)
                if (this[i] == str)
                    return i;
            return -1;
        }

        public int FindAll(string str)
        {
            int totalcount = 0;
            for (int i = 0; i < count; ++i)
                if (this[i] == str)
                    totalcount++;
            return totalcount;
        }

        public List<string> FindVariablesList()
        {
            List<string> variableslist = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (semiExp[i] != "\n" && semiExp[i] != " " && semiExp[i] != "public"
                    && semiExp[i] != "private" && semiExp[i] != "protected" && semiExp[i] != "static" && semiExp[i] != ";"
                    && semiExp[i] != "return")
                {
                    variableslist.Add(semiExp[i]);
                }
            }
            return variableslist;
        }
        //----< deprecated: here to avoid breakage with old code >----------- 

        public int Contains(string str)
        {
            return FindLast(str);
        }
        //----< have to override GetHashCode() >-----------------------------

        override public System.Int32 GetHashCode()
        {
            return base.GetHashCode();
        }
        //----< opens member tokenizer with specified file >-----------------

        public bool open(string fileName)
        {
            return toker.openFile(fileName);
        }
        //----< close file stream >------------------------------------------

        public void close()
        {
            toker.close();
        }
        //----< is this the last token in the current semiExpression? >------

        bool isTerminator(string tok)
        {
            switch (tok)
            {
                case ";": return true;
                case "{": return true;
                case "}": return true;
                case "\n":
                    if (this.FindFirst("#") != -1)  // expensive - may wish to cache in get
                        return true;
                    return false;
                default: return false;
            }
        }
        //----< get next token, saving previous token >----------------------

        string get()
        {
            prevTok = currTok;
            currTok = toker.getTok();
            if (verbose)
                Console.Write("{0} ", currTok);
            return currTok;
        }
        //----< is this character a punctuator> >----------------------------

        bool IsPunc(char ch)
        {
            return (Char.IsPunctuation(ch) || Char.IsSymbol(ch));
        }
        //
        //----< are these characters an operator? >--------------------------
        //
        // Performance issue - C# would not let me make opers static, so
        // it is being constructed on every call.  This is not desireable,
        // but neither is using a static data member that is initialized
        // remotely.  I will think more about this later.

        bool IsOperatorPair(char first, char second)
        {
            string[] opers = new string[]
      { 
        "/*", "*/", "//", "!=", "==", ">=", "<=", "&&", "||", "--", "++",
        "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=", "<<", ">>",
        "\\n", "\\t", "\\r", "\\f"
      };

            StringBuilder test = new StringBuilder();
            test.Append(first).Append(second);
            foreach (string oper in opers)
                if (oper.Equals(test.ToString()))
                    return true;
            return false;
        }
        //----< collect semiExpression from filtered token stream >----------

        public bool getSemi()
        {
            semiExp.RemoveRange(0, semiExp.Count);  // empty container
            do
            {
                get();
                if (currTok == "")
                    return false;  // end of file
                if (returnNewLines || currTok != "\n")
                    semiExp.Add(currTok);
            } while (!isTerminator(currTok) || count == 0);

            // if for then append next two semiExps, e.g., for(int i=0; i<se.count; ++i) {

            if (semiExp.Contains("for"))
            {
                CSemiExp se = clone();
                getSemi();
                se.Add(semiExp.ToArray());
                getSemi();
                se.Add(semiExp.ToArray());
                semiExp.Clear();
                for (int i = 0; i < se.count; ++i)
                    semiExp.Add(se[i]);
            }
            return (semiExp.Count > 0);
        }
        //----< get length property >----------------------------------------

        public int count
        {
            get { return semiExp.Count; }
        }
        //----< indexer for semiExpression >---------------------------------

        public string this[int i]
        {
            get { return semiExp[i]; }
            set { semiExp[i] = value; }
        }
        //----< insert token - fails if out of range and returns false>------

        public bool insert(int loc, string tok)
        {
            if (0 <= loc && loc < semiExp.Count)
            {
                semiExp.Insert(loc, tok);
                return true;
            }
            return false;
        }
        //----< append token to end of semiExp >-----------------------------

        public CSemiExp Add(string token)
        {
            semiExp.Add(token);
            return this;
        }
        //----< load semiExp from array of strings >-------------------------

        public void Add(string[] source)
        {
            foreach (string tok in source)
                semiExp.Add(tok);
        }
        //--< initialize semiExp with single ";" token - used for testing >--

        public bool initialize()
        {
            if (semiExp.Count > 0)
                return false;
            semiExp.Add(";");
            return true;
        }
        //----< remove all contents of semiExp >-----------------------------

        public void flush()
        {
            semiExp.RemoveRange(0, semiExp.Count);
        }
        //----< is this token a comment? >-----------------------------------

        public bool isComment(string tok)
        {
            if (tok.Length > 1)
                if (tok[0] == '/')
                    if (tok[1] == '/' || tok[1] == '*')
                        return true;
            return false;
        }
        //----< display semiExpression on Console >--------------------------

        public void display()
        {
            Console.Write("\n");
            Console.Write(displayStr());
        }
        //----< return display string >--------------------------------------

        public string displayStr()
        {
            StringBuilder disp = new StringBuilder("");
            foreach (string tok in semiExp)
            {
                disp.Append(tok);
                if (tok.IndexOf('\n') != tok.Length - 1)
                    disp.Append(" ");
            }
            return disp.ToString();
        }
        //----< announce tokens when verbose is true >-----------------------

        public bool verbose
        {
            get;
            set;
        }
        //----< determines whether new lines are returned with semi >--------

        public bool returnNewLines
        {
            get;
            set;
        }
        //----< determines whether new lines are displayed >-----------------

        public bool displayNewLines
        {
            get;
            set;
        }
        //----< determines whether comments are discarded >------------------

        public bool discardComments
        {
            get;
            set;
        }
        //
        //----< make a copy of semiEpression >-------------------------------

        public CSemiExp clone()
        {
            CSemiExp copy = new CSemiExp();
            for (int i = 0; i < count; ++i)
            {
                copy.Add(this[i]);
            }
            return copy;
        }
        //----< remove a token from semiExpression >-------------------------

        public bool remove(int i)
        {
            if (0 <= i && i < semiExp.Count)
            {
                semiExp.RemoveAt(i);
                return true;
            }
            return false;
        }
        //----< remove a token from semiExpression >-------------------------

        public bool remove(string token)
        {
            if (semiExp.Contains(token))
            {
                semiExp.Remove(token);
                return true;
            }
            return false;
        }
        //
#if(TEST_SEMI)

        //----< test stub >--------------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("\n  Testing semiExp Operations");
            Console.Write("\n ============================\n");

            CSemiExp test = new CSemiExp();
            test.returnNewLines = true;
            test.displayNewLines = true;

            string testFile = "Parser.cs";
            if (!test.open(testFile))
                Console.Write("\n  Can't open file {0}", testFile);
            while (test.getSemi())
                test.display();

            test.initialize();
            test.insert(0, "this");
            test.insert(1, "is");
            test.insert(2, "a");
            test.insert(3, "test");
            test.display();

            Console.Write("\n  2nd token = \"{0}\"\n", test[1]);

            Console.Write("\n  removing first token:");
            test.remove(0);
            test.display();
            Console.Write("\n");

            Console.Write("\n  removing token \"test\":");
            test.remove("test");
            test.display();
            Console.Write("\n");
        }
#endif
    }
}
