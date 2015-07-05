/////////////////////////////////////////////////////////////////////////
// IService.cs - Interface for BasicService demo                       //
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
 * Defines A contract for communication between the client and the server
 * 
 * Build Process
 * -------------
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe IService.cs
 * 
 */

//NOTE: This package does not contain any test stub as it a ServiceContract

using CodeAnalysis;
using System.Collections.Generic;
using System.ServiceModel;

namespace DependencyAnalyzer
{
    /// <summary>
    /// Defines the service contract used for Client and Server communication
    /// </summary>
    [ServiceContract(Namespace = "HandCraftedService")]
    public interface IBasicService
    {
        /// <summary>
        /// server receives message from client
        /// </summary>
        /// <param name="msg">message to be sent by client to server</param>
        [OperationContract]
        void sendMessage(string msg);

        /// <summary>
        /// server sends message to client
        /// </summary>
        /// <returns>the message to be sent to client</returns>
        [OperationContract]
        string getMessage();

        /// <summary>
        /// Server sends the list of files it has under the path given by client
        /// </summary>
        /// <param name="path">the path in the server to search for files</param>
        /// <param name="recurse">specifies whether recursive search needs to e performed</param>
        /// <returns>the List of files</returns>
        [OperationContract]
        List<ProjectTree> getFilesList(string path, bool recurse);

        /// <summary>
        /// Server receives the list of files selected by user
        /// </summary>
        /// <param name="files">The list of files selected by the user</param>
        /// <param name="serverName">The current server name</param>
        /// <returns>The List of analyzed results after Pass 1</returns>
        [OperationContract]
        List<Elem> setFilesList(List<string> files, string serverName);

        /// <summary>
        /// Server receives the merged type table to start round 2 of analysis
        /// </summary>
        /// <param name="typeTable">The merged type table</param>
        [OperationContract]
        void sendMergedTypeTable(List<Elem> typeTable);

        /// <summary>
        /// Server starts the Round 2 of analysis
        /// </summary>
        /// <param name="files">The list of files selected by the user</param>
        [OperationContract]
        void getAnalyzedData(List<string> files);

        /// <summary>
        /// Server sends the Repository containing the Round 2 Analysis
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Repository getRepo();

        /// <summary>
        /// Client informs the server to set the value of Repo to null
        /// Is required each time Analyze button is clicked in order to remove existing data
        /// </summary>
        [OperationContract]
        void setRepo();
    }
}
