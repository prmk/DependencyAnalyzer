/////////////////////////////////////////////////////////////////////////
// ProgClient.cs - Service Client for Programmatic BasicService        //
//                                                                    //
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
 * ProgClient client = new ProgClient();
 * client.CreateSendChannel("http://localhost:4000/IBasicService");
 * client.SendMessage(svc,"test");
 * client.GetMessage(svc);
 * client.GetFilesList(svc,".",false)
 * client.SetFilesList(svc,filesList,"4000");
 * client.mergeTypeTables(typeTable1,typeTable2);
 * client.SendMergedTypeTable(svc, mergedTypeTable);
 * client.GetAnalyzedData(svc,filesList);
 * client.GetRepo(svc);
 * client.SetRepo(svc);
 * client.mergeRepo(repo1,repo2);
 * 
 * 
 * 
 * Build Process
 * -------------
 * Required Files:
 *      ProgClient.cs
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe ProgClient.cs
 * 
 * 
 */

using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DependencyAnalyzer
{
    public class ProgClient
    {
        /// <summary>
        /// Connects to the server at the specified at url
        /// </summary>
        /// <param name="url">the server url</param>
        /// <returns>the created channel of the service contract type IBasicService</returns>
        public IBasicService CreateSendChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IBasicService> factory = new ChannelFactory<IBasicService>(binding, address);
            return factory.CreateChannel();
        }

        /// <summary>
        /// sends a message to the server connected at the svc channel
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <param name="msg">The message to be sent to the server</param>
        public void SendMessage(IBasicService svc, string msg)
        {
            svc.sendMessage(msg);
        }

        /// <summary>
        /// Gets the message from the server
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <returns>the message sent from the server</returns>
        public string GetMessage(IBasicService svc)
        {
            return svc.getMessage();
        }

        /// <summary>
        /// Gets the list of files under the server under the path given by the user
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <param name="path">the path to be searched for in the server</param>
        /// <returns>The list of files in the server under the specified path</returns>
        public List<ProjectTree> GetFilesList(IBasicService svc, string path, bool recurse)
        {
            return svc.getFilesList(path, recurse);
        }

        /// <summary>
        /// Sets the list of files selected by the user and sends it to the server
        /// The server returns the analyzed files back to the client
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <param name="files">The list of files selected by the user</param>
        /// <param name="serverName">The name of the server</param>
        /// <returns>The List of analyzed files</returns>
        public List<Elem> SetFilesList(IBasicService svc, List<string> files, string serverName)
        {
            List<Elem> table = svc.setFilesList(files, serverName);
            return table;
        }

        /// <summary>
        /// Takes 2 typetables and merges it into one typetable by putting the contents of typetable2 into typetable1
        /// </summary>
        /// <param name="typeTable1">The source type table</param>
        /// <param name="typeTable2">The destination type table</param>
        /// <returns>The merged type table</returns>
        public List<Elem> mergeTypeTables(List<Elem> typeTable1, List<Elem> typeTable2)
        {
            if (typeTable2 != null)
            {
                foreach (Elem elem in typeTable2)
                {
                    typeTable1.Add(elem);
                }
            }

            return typeTable1;
        }

        /// <summary>
        /// Client sends the merged type table to the server
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <param name="typeTableMerged">The merged type table to be sent to the server</param>
        public void SendMergedTypeTable(IBasicService svc, List<Elem> typeTableMerged)
        {
            svc.sendMergedTypeTable(typeTableMerged);
        }

        /// <summary>
        /// Call to start the Pass 2 of the analysis from the server
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <param name="files">THe list of files selected by the user</param>
        public void GetAnalyzedData(IBasicService svc, List<string> files)
        {
            svc.getAnalyzedData(files);
        }

        /// <summary>
        /// Gets the analyzed data after pass 2 of the analysis from the server
        /// </summary>
        /// <param name="svc">The IBasicService contract of the connected server</param>
        /// <returns>The analyzed data from the server</returns>
        public Repository GetRepo(IBasicService svc)
        {
            return svc.getRepo();
        }

        /// <summary>
        /// Merges the 2 repositories sent from the 2 servers by appending the data from the repo2 into repo1
        /// </summary>
        /// <param name="repo1">The destination repository</param>
        /// <param name="repo2">The source repository</param>
        /// <returns>the merged repository repo1</returns>
        public Repository mergeRepo(Repository repo1, Repository repo2)
        {
            List<InheritanceElem> inheritancedata1 = repo1.inheritancedata;
            List<AggregationElem> aggregationdata1 = repo1.aggregationdata;
            List<CompositionElem> compositiondata1 = repo1.compositiondata;
            List<UsingElem> usingdata1 = repo1.usingdata;
            List<PackageDependencyElem> packagedata1 = repo1.packagedata;
            List<InheritanceElem> inheritancedata2 = repo2.inheritancedata;
            List<AggregationElem> aggregationdata2 = repo2.aggregationdata;
            List<CompositionElem> compositiondata2 = repo2.compositiondata;
            List<UsingElem> usingdata2 = repo2.usingdata;
            List<PackageDependencyElem> packagedata2 = repo2.packagedata;

            foreach (InheritanceElem ie in inheritancedata2)
                inheritancedata1.Add(ie);
            foreach (AggregationElem ae in aggregationdata2)
                aggregationdata1.Add(ae);
            foreach (CompositionElem ce in compositiondata2)
                compositiondata1.Add(ce);
            foreach (UsingElem ue in usingdata2)
                usingdata1.Add(ue);
            foreach (PackageDependencyElem pd in packagedata2)
                packagedata1.Add(pd);

            repo1.inheritancedata = inheritancedata1;
            repo1.aggregationdata = aggregationdata1;
            repo1.compositiondata = compositiondata1;
            repo1.usingdata = usingdata1;
            repo1.packagedata = packagedata1;

            return repo1;
        }

        /// <summary>
        /// Empties the repos
        /// </summary>
        /// <param name="svc">The IBasicService object whose repo has to be set to empty</param>
        public void SetRepo(IBasicService svc)
        {
            svc.setRepo();
        }

        /// <summary>
        /// test the functionality of the client by connecting to the server if available
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "BasicHttp Client";
            Console.Write("\n  Starting Programmatic Basic Service Client");
            Console.Write("\n ============================================\n");

            string url = "http://localhost:4000/IBasicService";
            ProgClient client = new ProgClient();
            while (true)
            {
                try
                {
                    IBasicService svc = client.CreateSendChannel(url);

                    string msg = "This is a test message from client";
                    client.SendMessage(svc, msg);

                    msg = client.GetMessage(svc);
                    Console.Write("\n  Message recieved from Service: {0}\n\n", msg);
                    List<ProjectTree> files = client.GetFilesList(svc, ".", false);
                    //foreach (string file in files)
                    //  Console.WriteLine("{0}\n", file);
                    //client.SetFilesList(svc, files, "server1");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown " + ex.Message);
                    Console.WriteLine("Retrying...");
                }
            }
            Console.ReadKey();
        }

    }
}