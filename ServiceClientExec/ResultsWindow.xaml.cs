/////////////////////////////////////////////////////////////////////////
// Results.xaml.cs - WPF Client file                                   //
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
 * Displays the analyzed data to the user
 * It scans through the repository and displays all the analyzed data on the screen
 * Allows the user to show only ananlyzed data or package dependency data or both
 * 
 * 
 * Public Interface
 * ------------------
 * ResultWindow rs = new ResultWindow(repos);
 * 
 * Build Process
 * -------------
 * Required Files:
 *      ResultsWindow.xaml.cs
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe ResultsWindow.xaml.cs
 * 
 */

using CodeAnalysis;
using DependencyAnalyzer;
using System.Windows;

namespace ServiceClientExec
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        Repository repos;

        /// <summary>
        /// Displays the analyzed results to the user
        /// displays only or either of type analysis results and the package dependency results based on the user selection
        /// </summary>
        /// <param name="repos">the analyzed result repository</param>
        public ResultsWindow(Repository repos)
        {
            this.repos = repos;
            InitializeComponent();
            Title = "Package and Dependency Analyze Results";
            TypeAnalysisCheckBox.IsChecked = true;
            PackageDependencyCheckBox.IsChecked = true;
            showResults();
            GenerateXML();
        }

        /// <summary>
        /// Call the XML Generate method and displays the location of the stored path
        /// </summary>
        private void GenerateXML()
        {
            XmlGenerator xm = new XmlGenerator();
            string path = xm.generateXML(repos);
            XMLLabel.Text = "XML File Path: " + path;
        }

        /// <summary>
        /// displays the final results to the user in the result window
        /// </summary>
        private void showResults()
        {
            showInheritance();
            showAggregation();
            showComposition();
            showUsing();
            showPackages();
        }

        /// <summary>
        /// displays inheritance data in the result window
        /// </summary>
        private void showInheritance()
        {
            foreach (InheritanceElem ie in repos.inheritancedata)
            {
                string msg = "";
                foreach (string child in ie.children)
                {
                    msg = ie.parent + " is the parent of: " + child;
                    InheritanceListBox.Items.Insert(0, msg);
                }
            }
        }

        /// <summary>
        /// displays the aggregation data in the result window
        /// </summary>
        private void showAggregation()
        {
            foreach (AggregationElem ae in repos.aggregationdata)
            {
                string msg = "";
                foreach (string aggregate in ae.aggregated)
                {
                    msg = ae.aggregator + " aggregates " + ae.type + " " + aggregate;
                    AggregationListBox.Items.Insert(0, msg);
                }
            }
        }

        /// <summary>
        /// displays the composition data in the result window
        /// </summary>
        private void showComposition()
        {
            foreach (CompositionElem ce in repos.compositiondata)
            {
                string msg = "";
                foreach (string composed in ce.composedelement)
                {
                    msg = ce.compositor + " composes " + ce.type + " " + composed;
                    CompositionListBox.Items.Insert(0, msg);
                }
            }
        }

        /// <summary>
        /// displays the using data in the result window
        /// </summary>
        private void showUsing()
        {
            foreach (UsingElem ue in repos.usingdata)
            {
                string msg = "";
                foreach (TypeDetails type in ue.typeslist)
                {
                    msg = ue.parent + " has function " + ue.containingfunction +
                        " and has " + type.type + " " + type.usedtypename;
                    UsingListBox.Items.Insert(0, msg);
                }
            }
        }

        /// <summary>
        /// displays the package dependency data in the result window
        /// </summary>
        private void showPackages()
        {
            foreach (PackageDependencyElem pe in repos.packagedata)
            {
                string msg = "";
                msg = "Parent Package " + pe.parentPackage + " from server " + pe.parentServerName +
                        ". Child package: " + pe.childPackageName + " from server " + pe.childServerName;
                PackageListBox.Items.Insert(0, msg);
            }
        }

        /// <summary>
        /// Hides and shows the Type Analysis results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeAnalysisCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)TypeAnalysisCheckBox.IsChecked)
            {
                Grid1.Visibility = Visibility.Visible;
                Grid2.Visibility = Visibility.Visible;
                Grid3.Visibility = Visibility.Visible;
                Grid4.Visibility = Visibility.Visible;
            }
            else
            {
                Grid1.Visibility = Visibility.Collapsed;
                Grid2.Visibility = Visibility.Collapsed;
                Grid3.Visibility = Visibility.Collapsed;
                Grid4.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Hides and shows the Package dependency results
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageDependencyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)PackageDependencyCheckBox.IsChecked)
                Grid5.Visibility = Visibility.Visible;
            else
                Grid5.Visibility = Visibility.Collapsed;
        }
    }
}
