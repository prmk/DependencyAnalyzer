/////////////////////////////////////////////////////////////////////////
// XMLGenerator.cs - Generates XML                                     //
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
 * Gets the analyzed data from the calling function
 * loops through all the data and generates XML
 * 
 * 
 * Public Interface
 * ------------------
 * XmlGenerator xm = new XmlGenetator();
 * xm.generateXML();
 * 
 * Build Process
 * -------------
 * Required Files:
 *  XMLGenerator.cs
 *      
 * Compiler Command:
 * -----------------
 *   csc /target:exe XMLGenerator.cs
 * 
 *   
 * 
 */
using CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DependencyAnalyzer
{
    public class XmlGenerator
    {
        XDocument xmlDocument;
        XComment xmlDocumentComment;
        XElement xmRootElement;
        Repository repo;

        //constructor in which all the member variables are initialised     
        public XmlGenerator()
        {
            try
            {
                xmlDocument = new XDocument();
                xmlDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                xmlDocumentComment = new XComment("Generates XML Output for the Analyzed data");
                xmRootElement = new XElement("AnalysisResult");
            }
            catch
            {
                Console.WriteLine("Error occurred while generating the XML file");
            }
        }

        /// <summary>
        /// Public interface to generate the XML
        /// </summary>
        /// <param name="repo">Takes in the repository to generate the XML from</param>
        /// <returns>the path of the generated xml</returns>
        public string generateXML(Repository repo)
        {
            string path = "";
            setRepo(repo);
            generateXmlTypesDetails();
            generatePackageDependencyDetails();
            generateRelationshipsDetails();
            path = saveXml();
            return path;
        }


        private void setRepo(Repository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// method used to save the xml file
        /// </summary>
        /// <returns>the path of the saved xml file</returns>
        private string saveXml()
        {
            xmlDocument.Save("Results.xml");
            string path = System.IO.Directory.GetCurrentDirectory().ToString() + "\\Results.xml";
            return path;
        }

        /// <summary>
        /// Creates XML for Type Details        
        /// </summary>
        private void generateXmlTypesDetails()
        {
            try
            {
                Repository rep = Repository.getInstance();
                List<Elem> repoTable = rep.analyzedata;
                xmlDocument.Add(xmlDocumentComment);
                xmlDocument.Add(xmRootElement);

                if (repoTable.Count == 0)
                    return;

                XElement firstLevelChild = new XElement("TypeDetailsFileWise");
                XElement secondLevelChild = null;
                xmRootElement.Add(firstLevelChild);
                foreach (Elem elem in repoTable)
                {
                    if (elem.begin == 0 && elem.end == 0 && elem.type != "")
                    {
                        secondLevelChild = new XElement("FilenameName", elem.packageName);
                        xmRootElement.Add(secondLevelChild);
                    }
                    else if (elem.type.Equals("namespace") || elem.type.Equals("class") || elem.type.Equals("interface") || elem.type.Equals("function"))
                    {
                        if (secondLevelChild != null)
                        {
                            secondLevelChild.Add(new XElement("TypeDetails"));
                            secondLevelChild.Add(new XElement("Type", elem.type));
                            secondLevelChild.Add(new XElement("Name", elem.name));
                            secondLevelChild.Add(new XElement("Begin", elem.begin));
                            secondLevelChild.Add(new XElement("End", elem.end));
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while generating the xml file");
            }
        }

        /// <summary>
        /// Creates XML for Package DependencyDetails        
        /// </summary>
        private void generatePackageDependencyDetails()
        {
            List<PackageDependencyElem> packageDependencyTable = repo.packagedata;

            if (packageDependencyTable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NoPackageDependencies");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("PackageDependency");
            XElement secondLevelChild = null;

            foreach (PackageDependencyElem ie in packageDependencyTable)
            {
                secondLevelChild = new XElement("ParentPackage", ie.parentPackage);
                secondLevelChild.Add(new XElement("ChildPackage", ie.childPackageName));
            }
            firstLevelChild.Add(secondLevelChild);
            xmRootElement.Add(firstLevelChild);
            Console.WriteLine();
        }


        /// <summary>
        /// Creates XML for all kinds of relationships
        /// </summary>
        private void generateRelationshipsDetails()
        {
            try
            {
                generateInheritanceTable();
                generateAggergationTable();
                generateCompositionTable();
                generateUsingTable();
            }
            catch (Exception)
            {
                Console.WriteLine("Error occurred while displaying relationships");
            }
        }

        /// <summary>
        /// Creates XML for Inheritance data
        /// </summary>
        private void generateInheritanceTable()
        {
            List<InheritanceElem> inheritancetable = repo.inheritancedata;

            if (inheritancetable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NoInheritance");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("Inheritance");
            XElement secondLevelChild = null;

            foreach (InheritanceElem ie in inheritancetable)
            {
                foreach (string child in ie.children)
                {
                    secondLevelChild = new XElement("Parent", ie.parent);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("inherits", "Inherits");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Child", child);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
            Console.WriteLine();
        }

        /// <summary>
        /// Creates XML for Aggregation data
        /// </summary>
        private void generateAggergationTable()
        {
            List<AggregationElem> aggregatedtable = repo.aggregationdata;

            if (aggregatedtable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NoAggregation");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("Aggregation");
            XElement secondLevelChild = null;

            foreach (AggregationElem ae in aggregatedtable)
            {
                foreach (string agg in ae.aggregated)
                {
                    secondLevelChild = new XElement("Aggregator", ae.aggregator);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("aggregates", "Aggregates");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Child", agg);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "OfType");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", ae.type);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
        }

        /// <summary>
        /// Creates XML for Composition data
        /// </summary>
        private void generateCompositionTable()
        {
            List<CompositionElem> compositiontable = repo.compositiondata;

            if (compositiontable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("NoComposition");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            XElement firstLevelChild = new XElement("Composition");
            XElement secondLevelChild = null;

            foreach (CompositionElem ce in compositiontable)
            {
                foreach (string comp in ce.composedelement)
                {
                    secondLevelChild = new XElement("Class", ce.compositor);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("composes", "Composes");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("element", comp.ToString());
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "OfType");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", ce.type);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
        }

        /// <summary>
        /// Creates XML for using data
        /// </summary>
        private void generateUsingTable()
        {
            List<UsingElem> usingtable = repo.usingdata;
            XElement firstLevelChild = new XElement("USINGDETAILS");
            XElement secondLevelChild = null;

            if (usingtable.Count == 0)
            {
                XElement firstLevelChildForNoData = new XElement("UsingRelation");
                xmRootElement.Add(firstLevelChildForNoData);
                return;
            }

            foreach (UsingElem ue in usingtable)
            {
                foreach (TypeDetails elt in ue.typeslist)
                {
                    secondLevelChild = new XElement("Class", ue.parent);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("uses", "uses");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("element", elt.usedtypename);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("ofType", "OfType");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("Type", elt.type);
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("function", "FunctionName");
                    firstLevelChild.Add(secondLevelChild);
                    secondLevelChild = new XElement("function", ue.parent);
                    firstLevelChild.Add(secondLevelChild);
                }
            }
            xmRootElement.Add(firstLevelChild);
        }


        public class TestXml
        {
            public static void Main(String[] args)
            {
                XmlGenerator test = new XmlGenerator();
                Repository rep = Repository.getInstance();
                try
                {
                    if (rep.analyzedata.Count == 0)
                    {
                        Console.WriteLine("The repository is empty. Nothing to display");
                    }

                    test.generateXmlTypesDetails();
                    test.generateRelationshipsDetails();
                }
                catch
                {
                    Console.WriteLine("Looks like the data is empty.");
                    Console.ReadKey();
                }
            }
        }
    }
}
