/////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client executive and WPF Client file           //
//                                                                     /
//  Language:      C#                                                  //
//  Platform:      Windows 7 Premium                                   //
//  Application:   Dependency Analyzer                                 //
//                                                                     //
// Author: Pramukh Shenoy                                              //
// Original Author: Jim Fawcett                                        ///
/////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * Provides functionality to connect to server
 * Allows a client to connect to 2 different servers at a time
 * Shows the list of projects under each server connected
 * Allows the user to select one or more files from the list
 * Error Handling
 * Informs the server to start analyzing the files
 * Calls the Merge operation to merge the analyzed data
 * Passes the analysed data to the ResultWindow to be displayed to the user
 * 
 * 
 * Public Interface
 * ------------------
 * MainWindow mw = new MainWindow()
 * 
 * Build Process
 * -------------
 * Required Files:
 *      MainWindow.xaml.cs
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe MainWindow.xaml.cs
 * 
 */

using CodeAnalysis;
using DependencyAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ServiceClientExec
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProgClient client = null;
        IBasicService svcServer1Channel = null;
        IBasicService svcServer2Channel = null;
        string server1Port = "";
        string server2Port = "";
        string endPoint1 = "";
        string endPoint2 = "";
        bool server1recurse = false;
        bool server2recurse = false;

        List<ProjectTree> projectfiles1;
        List<ProjectTree> projectfiles2;
        List<string> server1Files;
        List<string> server2Files;
        List<Elem> server1TypeTable;
        List<Elem> server2TypeTable;
        List<Elem> mergedTypeTable;
        Repository repo1Server;
        Repository repo2Server;
        Repository repoMerged;
        ResultsWindow resultsWindow = null;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Dependency Analyze Client";
            AnalyzeButton.IsEnabled = false;
        }

        /// <summary>
        /// Is called on the click of Connect Button 1
        /// Connects to the server at the specefied port
        /// Displays the file names from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton1_Click(object sender, RoutedEventArgs e)
        {
            int projectFilesCount = 0;
            try
            {
                getConnection1();
                string msg = "This is a test message from client";
                client.SendMessage(svcServer1Channel, msg);
                msg = client.GetMessage(svcServer1Channel);
                listBox1.Items.Insert(0, msg);
                ConnectButton1.IsEnabled = false;
                server1recurse = (bool)RecursiveSearch1.IsChecked;
                projectfiles1 = client.GetFilesList(svcServer1Channel, "test1", server1recurse);
                server1Files = new List<string>();

                foreach (ProjectTree prjtree in projectfiles1)
                {
                    string displayPath = prjtree.parentRelativePath + " (" + prjtree.parentAbsolutePath + ")";
                    listBox1.Items.Insert(projectFilesCount++, displayPath);
                    foreach (string file in prjtree.filesList)
                    {
                        CheckBox itemCheckboxFiles = new CheckBox();
                        itemCheckboxFiles.Content = file;
                        listBox1.Items.Insert(projectFilesCount++, itemCheckboxFiles);
                    }
                }
                if (!AnalyzeButton.IsEnabled)
                    AnalyzeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                showErrorWindow(ex.Message);
            }
        }

        /// <summary>
        /// Is called on the click of Connect Button 2
        /// Connects to the server at the specefied port
        /// Displays the file names from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getConnection2();
                int projectFilesCount = 0;
                string msg = "This is a test message from client port" + RemotePortTextBox2;
                client.SendMessage(svcServer2Channel, msg);
                msg = client.GetMessage(svcServer2Channel);
                listBox2.Items.Insert(0, msg);
                ConnectButton2.IsEnabled = false;
                server2recurse = (bool)RecursiveSearch2.IsChecked;
                //server2Files = client.GetFilesList(svcServer2Channel, "..//", server2recurse);
                projectfiles2 = client.GetFilesList(svcServer2Channel, "test2", server2recurse);
                server2Files = new List<string>();
                foreach (ProjectTree prjtree in projectfiles2)
                {
                    string displayPath = prjtree.parentRelativePath + " (" + prjtree.parentAbsolutePath + ")";
                    listBox2.Items.Insert(projectFilesCount++, displayPath);
                    foreach (string file in prjtree.filesList)
                    {
                        CheckBox itemCheckboxFiles = new CheckBox();
                        itemCheckboxFiles.Content = file;
                        listBox2.Items.Insert(projectFilesCount++, itemCheckboxFiles);
                    }
                }
                if (!AnalyzeButton.IsEnabled)
                    AnalyzeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                showErrorWindow(ex.Message);
            }
        }

        /// <summary>
        /// Is called on click of Analyze button
        /// Gets the list of files selected by the user
        /// sends it to the server to start the analysis
        /// gets the analyzed results
        /// merges the analyzed type tables if there are 2 different connected servers
        /// sends the merged result to each of the servers
        /// sends signal to the server to start round 2 of the analysis 
        /// gets the repository instance from each of the servers
        /// merges the repository if there are 2 different connected servers
        /// creates an instance of result window and passed the final repository
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            clearFiles();
            if (svcServer1Channel != null)
            {
                client.SetRepo(svcServer1Channel);
                getUserSelectedFilesServer1();
                if (server1Files.Count > 0)
                    server1TypeTable = client.SetFilesList(svcServer1Channel, server1Files, server1Port);
                else
                {
                    showErrorWindow(1);
                    return;
                }
            }
            if (svcServer2Channel != null)
            {
                client.SetRepo(svcServer2Channel);
                getUserSelectedFilesServer2();
                if (server2Files.Count > 0)
                    server2TypeTable = client.SetFilesList(svcServer2Channel, server2Files, server2Port);
                else
                {
                    showErrorWindow(2);
                    return;
                }
            }
            if (svcServer1Channel != null && svcServer2Channel != null)
            {
                mergedTypeTable = client.mergeTypeTables(server1TypeTable, server2TypeTable);
                client.SendMergedTypeTable(svcServer1Channel, mergedTypeTable);
                client.SendMergedTypeTable(svcServer2Channel, mergedTypeTable);
            }
            if (svcServer1Channel != null)
            {
                client.GetAnalyzedData(svcServer1Channel, server1Files);
                repo1Server = client.GetRepo(svcServer1Channel);
            }
            if (svcServer2Channel != null)
            {
                client.GetAnalyzedData(svcServer2Channel, server2Files);
                repo2Server = client.GetRepo(svcServer2Channel);
            }

            if (svcServer1Channel != null && svcServer2Channel != null)
                repoMerged = client.mergeRepo(repo1Server, repo2Server);
            else if (svcServer1Channel != null && svcServer2Channel == null)
                repoMerged = repo1Server;
            else if (svcServer1Channel == null && svcServer2Channel != null)
                repoMerged = repo2Server;

            resultsWindow = new ResultsWindow(repoMerged);
            resultsWindow.Show();
        }

        /// <summary>
        /// Clears the list of files
        /// </summary>
        private void clearFiles()
        {
            if (server1Files != null)
                server1Files.Clear();
            if (server2Files != null)
                server2Files.Clear();
        }

        /// <summary>
        /// Gets the list of files of Server1 selected by the user
        /// </summary>
        private void getUserSelectedFilesServer1()
        {
            string parentDirectory = "";
            foreach (object obj in listBox1.Items)
            {
                try
                {
                    CheckBox selectedItems = (CheckBox)obj;

                    if (selectedItems != null && selectedItems.IsChecked == true)
                    {
                        String currentname = selectedItems.Content.ToString();
                        parentDirectory = parentDirectory + "\\" + currentname;
                        if (File.Exists(parentDirectory))
                            server1Files.Add(parentDirectory);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Directory!!! {0}", ex.ToString());
                    string path = obj.ToString();
                    int index = path.IndexOf("(");
                    path = path.Substring(index + 1);
                    path = path.Substring(0, path.Length - 1);
                    parentDirectory = path;
                }
            }
        }

        /// <summary>
        /// Gets the list of files of Server2 selected by the user
        /// </summary>
        private void getUserSelectedFilesServer2()
        {
            string parentDirectory = "";
            foreach (object obj in listBox2.Items)
            {
                try
                {
                    CheckBox selectedItems = (CheckBox)obj;

                    if (selectedItems != null && selectedItems.IsChecked == true)
                    {
                        String currentname = selectedItems.Content.ToString();
                        parentDirectory = parentDirectory + "\\" + currentname;
                        if (File.Exists(parentDirectory))
                            server2Files.Add(parentDirectory);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Directory!!! {0}", ex.ToString());
                    string path = obj.ToString();
                    int index = path.IndexOf("(");
                    path = path.Substring(index + 1);
                    path = path.Substring(0, path.Length - 1);
                    parentDirectory = path;
                }
            }
        }

        /// <summary>
        /// creates a connection to server1 if not already connected by fetching the port number selected by the user
        /// </summary>
        void getConnection1()
        {
            if (ConnectButton1.IsEnabled)
            {
                server1Port = RemotePortTextBox1.Text;
                endPoint1 = RemoteAddressTextBox1.Text;
                string endpoint = endPoint1 + ":" + server1Port + "/IBasicService";
                try
                {
                    if (client == null)
                        client = new ProgClient();
                    svcServer1Channel = client.CreateSendChannel(endpoint);

                }
                catch (Exception ex)
                {
                    showErrorWindow(ex.Message, server1Port);
                }
            }
        }

        /// <summary>
        /// creates a connection to server2 if not already connected by fetching the port number seleted by the user
        /// </summary>
        void getConnection2()
        {
            if (ConnectButton2.IsEnabled)
            {
                server2Port = RemotePortTextBox2.Text;
                endPoint2 = RemoteAddressTextBox2.Text;
                string endpoint = endPoint2 + ":" + server2Port + "/IBasicService";
                try
                {
                    if (client == null)
                        client = new ProgClient();
                    svcServer2Channel = client.CreateSendChannel(endpoint);
                }
                catch (Exception ex)
                {
                    showErrorWindow(ex.Message, server2Port);
                }
            }
        }

        /// <summary>
        /// displays an error window and the message to be displayed to the user along with the port number where the error has occurred
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="portNo">the port number where the error has occurred</param>
        void showErrorWindow(string message, string portNo)
        {
            Window temp = new Window();
            StringBuilder msg = new StringBuilder(message);
            msg.Append("\nport = ");
            msg.Append(portNo);
            temp.Content = msg.ToString();
            temp.Height = 100;
            temp.Width = 500;
            temp.Show();
        }

        /// <summary>
        /// displays an error window and the message to be displayed to the user
        /// </summary>
        /// <param name="message">the error message</param>
        void showErrorWindow(string message)
        {
            Window temp = new Window();
            temp.Content = message;
            temp.Height = 100;
            temp.Width = 500;
            temp.Show();
        }

        /// <summary>
        /// Shows the error message on the list boxes
        /// </summary>
        /// <param name="listbox">The list box number where the error has to be displayed</param>
        void showErrorWindow(int listbox)
        {
            string msg = "Please select a file to analyze";
            if (listbox == 1)
                listBox1.Items.Insert(0, msg);
            else
                listBox2.Items.Insert(0, msg);
            return;
        }
    }
}
