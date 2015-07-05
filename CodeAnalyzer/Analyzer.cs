/////////////////////////////////////////////////////////////////////////
// Analyzer.cs - Analyzes the files                                    //
//                                                                     //
//  Language:      C#                                                  //
//  Platform:      Windows 7 Premium                                   //
//  Application:   Dependency Analyzer                                 //
//                                                                     //
// Author: Pramukh Shenoy                                              //
// Original Author: Jim Fawcett                                        //
/////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module Analyses the specified file
 * Analyzes by calling Pass 1 and Pass 2 of the operations
 * 
 * 
 * Public Interface
 * ------------------
 * Analyzer analyzer = new Analyzer();
 * analyzer.Files
 * analyzer.relationshipflag
 * analyzer.analyze(8080)
 * analyzer.getDefaultElemData("debug", "8080");
 * analyzer.analyzePartII
 * 
 * Build Process
 * -------------
 * Required Files:
 *      Analyzer.cs
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe Analyzer.cs
 * 
 */

using CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DependencyAnalyzer
{
    /// <summary>
    /// Respnsible for handling the pass1 and pass2 of the analysis
    /// </summary>
    public class Analyzer
    {
        private List<string> files;

        /// <summary>
        /// gets and sets the files
        /// </summary>
        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }
        private bool relationshipflag;

        /// <summary>
        /// gets the sets the relationship flag which is used as a check for pass 2 of analysis
        /// </summary>
        public bool Relationshipflag
        {
            get { return relationshipflag; }
            set { relationshipflag = value; }
        }

        //read the list of files, one by one and calls BuildCodeAnalyzer and parser functions
        public void analyze(string serverName)
        {
            Console.Write("\n  CODE ANALYZER");
            Console.Write("\n ======================\n");

            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;

            foreach (object file in files)
            {
                Console.Write("\n\n  Processing file {0}\n", file as string);

                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }

                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------\n");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                CodeAnalysis.Parser parser = builder.build();

                Repository repo = Repository.getInstance();
                Elem elem = getDefaultElemData(file.ToString(), serverName);
                repo.analyzedata.Add(elem);

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                semi.close();
            }
        }

        /// <summary>
        /// creates an elem type data 
        /// used to store package and server name
        /// </summary>
        /// <param name="package">the package name to store in the elem</param>
        /// <param name="serverName">the server name to store in the elem</param>
        /// <returns>Elem</returns>
        public Elem getDefaultElemData(string package, string serverName)
        {
            int index = package.LastIndexOf("\\") + 1;
            string fileName = "";
            if (index != -1)
                fileName = package.Substring(index);
            Elem temp = new Elem();
            temp.begin = 0;
            temp.end = 0;
            temp.type = "dummy";
            temp.packageName = fileName;
            temp.serverName = serverName;
            return temp;
        }

        /// <summary>
        /// Starts the part 2 of analysis to find out the package and relationship dependency
        /// </summary>
        public void analyzePartII()
        {
            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;

            foreach (object file in files)
            {
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return;
                }

                BuildCodeAnalyzerRelationships builderreln = new BuildCodeAnalyzerRelationships(semi);
                CodeAnalysis.Parser parser = builderreln.build();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                semi.close();
            }
        }
    }

    /// <summary>
    /// test stub
    /// </summary>
#if(TEST_ANALYZE)
    class TestAnalyze
    {
        static void Main(string[] args)
        {
            List<string> paths = new List<string>();
            List<string> files = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                string[] newFiles = Directory.GetFiles(args[i], "*.cs");
                ArrayList filesAL = new ArrayList();
                for (int j = 0; j < newFiles.Length; ++j)
                {
                    if (!newFiles[j].Contains("Temporary") && !newFiles[j].Contains("AssemblyInfo.cs"))
                        filesAL.Add(Path.GetFullPath(newFiles[j]));
                }
                files.AddRange((String[])filesAL.ToArray(typeof(string)));
            }

            Analyzer analyze = new Analyzer();
            analyze.Files = files;
            analyze.analyze("");
        }
    }
#endif
}
