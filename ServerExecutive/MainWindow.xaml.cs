/////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Server executive and WPF Server file           //
//  ver 1.0                                                            //
//  Language:      C#                                                  //
//  Platform:      Windows 7 Premium                                   //
//  Application:   Dependency Analyzer                                 //
//  Author: Pramukh Shenoy                                              //
//  Original Author: Jim Fawcett                                        //
/////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * 
 * Provides User Interace to interact
 * Allows starting and stopping the  server
 * 
 * 
 * Public Interface
 * ------------------
 * MainWindow - initializes the WPF Server window 
 *   
 * 
 */

using DependencyAnalyzer;
using System;
using System.Text;
using System.Windows;

namespace ServerExecutive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProgHost server = null;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Dependency Analyzer Server";
            StopButton.IsEnabled = false;
        }

        /// <summary>
        /// Is called on the click of the server's Listen button
        /// starts the server at the specified port number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListenButton1_Click(object sender, RoutedEventArgs e)
        {
            string localPort1 = RemotePortTextBox1.Text;
            string endpoint1 = RemoteAddressTextBox.Text + ":" + localPort1 + "/IBasicService";

            try
            {
                server = new ProgHost();
                server.CreateReceiveChannel(endpoint1);
                listBox1.Items.Insert(0, "Started.. Waiting for a Client");
                ListenButton1.IsEnabled = false;
                StopButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Window temp = new Window();
                StringBuilder msg = new StringBuilder(ex.Message);
                msg.Append("\nport = ");
                msg.Append(localPort1.ToString());
                temp.Content = msg.ToString();
                temp.Height = 100;
                temp.Width = 500;
                temp.Show();
            }
        }

        /// <summary>
        /// stops the running server on closing the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            server.close();
        }

        /// <summary>
        /// Stops the running server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            server.close();
            ListenButton1.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }
}
