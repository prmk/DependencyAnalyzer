/////////////////////////////////////////////////////////////////////////
// ProgHost.cs - Service Host for Programmatic BasicService demo       //
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
 * Service Host for the Application
 * Uses BasicHttpBinding
 * Creates a listening channel for the client to connect
 * Gets and sets the list of files
 * Gets and sends messages between client and itself
 * Gets the merged type table from the client
 * Analyzes the files - Pass 1 and Pass 2
 * Sends the analyzed results to the client
 * 
 * 
 * Public Interface
 * ------------------
 * ProgHost server = new ProgHost();
 * server.CreateReceiveChannel(endpoint1);
 * server.sendMessage("test message");
 * server.getMessage();
 * server.getFilesList(".",true);
 * server.setFilesList(files, "8080");
 * server.sendMergedTypeTable(typeTable);
 * server.getAnalyzedData(files);
 * server.getRepo();

 * 
 * Build Process
 * -------------
 * Required Files:
 *  ProgHost.cs
 *      
 * Compiler Command:
 * -----------------
 *   csc /target:exe ProgHost.cs
 * 
 *   
 * 
 */

using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DependencyAnalyzer
{
    /// <summary>
    /// Server for the project
    /// Implements the IBasicService contract
    /// Interacts with client
    /// Analyzes the files provided by client
    /// </summary>
    public class ProgHost : IBasicService
    {
        ServiceHost host = null;
        public string msg = "";
        public List<ProjectTree> filestree;
        public List<String> files;
        FileMngr fm = null;
        Analyzer analyzer;
        List<Elem> localTypeTable = new List<Elem>();

        /// <summary>
        /// Creates a channel for the client to connect
        /// </summary>
        /// <param name="url">The URL where the server will be hosted</param>
        public void CreateReceiveChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri address = new Uri(url);
            Type service = typeof(ProgHost);
            host = new ServiceHost(service, address);
            host.AddServiceEndpoint(typeof(IBasicService), binding, address);
            host.Open();
        }

        /// <summary>
        /// server receives message from client
        /// </summary>
        /// <param name="msg">message to be sent by client to server</param>
        public void sendMessage(string msg)
        {
            this.msg = "Service received message: " + msg;
        }

        /// <summary>
        /// server sends message to client
        /// </summary>
        /// <returns>the message to be sent to client</returns>
        public string getMessage()
        {
            return "Connected...";
        }

        /// <summary>
        /// Server sends the list of files it has under the path given by client
        /// </summary>
        /// <param name="path">the path in the server to search for files</param>
        /// <returns>the List of files</returns>
        public List<ProjectTree> getFilesList(string path, bool recurse)
        {
            fm = new FileMngr();
            fm.Patterns.Add("*.cs");
            fm.Recurse = recurse;
            fm.findFiles(path);
            fm.createProjectTree();
            filestree = fm.getFiles();

            return filestree;
        }

        /// <summary>
        /// Server receives the list of files selected by user
        /// </summary>
        /// <param name="files">The list of files selected by the user</param>
        /// <param name="serverName">The current server name</param>
        /// <returns>The List of analyzed results after Pass 1</returns>
        public List<Elem> setFilesList(List<string> files, string serverName)
        {
            this.files = files;
            analyze(serverName);
            Repository rep = Repository.getInstance();
            List<Elem> table = rep.analyzedata;
            return table;
        }

        /// <summary>
        /// calls the round 1 of analysis of CodeAnalyzer
        /// </summary>
        public void analyze(string serverName)
        {
            analyzer = new Analyzer();
            analyzer.Files = files;
            analyzer.Relationshipflag = true;
            analyzer.analyze(serverName);
        }

        /// <summary>
        /// used by client to send the merged type table. The server then updates its Repo
        /// </summary>
        /// <param name="typeTable">the merged type table</param>
        public void sendMergedTypeTable(List<Elem> typeTable)
        {
            Repository repo = Repository.getInstance();
            repo.analyzedata = typeTable;
        }

        /// <summary>
        /// Server starts the Round 2 of analysis
        /// </summary>
        /// <param name="files">The list of files selected by the user</param>
        public void getAnalyzedData(List<string> files)
        {
            analyzer = new Analyzer();
            analyzer.Files = files;
            analyzer.analyzePartII();
        }

        /// <summary>
        /// Server sends the Repository containing the Round 2 Analysis
        /// </summary>
        /// <returns></returns>
        public Repository getRepo()
        {
            return Repository.getInstance();
        }

        /// <summary>
        /// Client informs the server to set the value of Repo to null
        /// Is required each time Analyze button is clicked in order to remove existing data
        /// </summary>
        public void setRepo()
        {
            Repository repo = Repository.getInstance();
            if (repo != null)
                repo.setInstance();
        }

        /// <summary>
        /// Used to display the analyzed results on the console
        /// </summary>
        void show()
        {
            try
            {
                Console.WriteLine("\n\n  COMPLEXITY SUMMARY");
                Console.WriteLine("===============");
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.analyzedata;
                Console.WriteLine("\nType             Name                Begin       End         Lines      Scopes");
                Console.Write("==============================================================================");
                foreach (Elem e in table)
                {
                    if (e.type.Equals("namespace") || e.type.Equals("class") || e.type.Equals("interface"))
                    {
                        Console.Write("\n{0,10} {1,20} {2,8} {3,11}", e.type, e.name, e.begin, e.end);
                        Console.Write("{0,13}", (e.end - e.begin + 1));
                    }
                    else if (e.type.Equals("function"))
                    {
                        Console.Write("\n{0,10} {1,20} {2,8} {3,11}", e.type, e.name, e.begin, e.end);
                        Console.Write("{0,13} {1,8}", (e.end - e.begin + 1), e.scopecount);
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while displaying complexity");
                Console.WriteLine(ex.Message);
            }
        }

        public void close()
        {
            if (host != null)
                host.Close();
        }

        /// <summary>
        /// starts the server and keeps listening for the server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "BasicHttp Service Host";
            Console.Write("\n  Starting Programmatic Basic Service");
            Console.Write("\n =====================================\n");

            ProgHost server = null;
            try
            {
                server = new ProgHost();
                server.CreateReceiveChannel("http://localhost:4000/IBasicService");
                Console.Write("\n  Started BasicService - Press key to exit:\n");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n\n", ex.Message);
                Console.ReadKey();
                return;
            }

            server.close();
        }
    }
}