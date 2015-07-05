/////////////////////////////////////////////////////////////////////////
// FileMngr.cs - File Handler for the project                          //
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
 * Defines the structure of files
 * Gets the list of path and pattern to search for files
 * Searches the directory for the specified file with the pattern
 * Gets the full file path and the relative paths of the files
 * Creates a project tree structure
 * 
 * 
 * Public Interface
 * ------------------
 * FileMngr fm = new FileMngr();
 * fm.files = files;
 * files = fm.files;
 * fm.recurse;
 * fm.patterns(".cs");
 * fm.getFiles();
 * fm.createProjectTree();
 * fm.getRelativePath("C:\temp\test.txt");
 * fm.findFiles("c:\temp");
 * 
 * Build Process
 * -------------
 * Required Files:
 *      FileMngr.cs
 * 
 * Compiler Command:
 * -----------------
 *   csc /target:exe FileMngr.cs
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DependencyAnalyzer
{
    /// <summary>
    /// Structure to hold files by combining all the files under a directory into one project
    /// </summary>
    public class ProjectTree
    {
        public string parentAbsolutePath { get; set; }
        public string parentRelativePath { get; set; }
        public List<String> filesList;
        public ProjectTree()
        {
            filesList = new List<String>();
        }
    }

    public class FileMngr
    {
        List<string> files;
        List<ProjectTree> projectList;

        /// <summary>
        /// getter and setter for files
        /// </summary>
        public List<string> Files
        {
            get { return files; }
            set { files = value; }
        }
        private bool recurse;

        /// <summary>
        /// getter and setter for recurse boolean
        /// </summary>
        public bool Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }
        private List<string> patterns;

        /// <summary>
        /// getter and setter for the patterns 
        /// </summary>
        public List<string> Patterns
        {
            get { return patterns; }
            set { patterns = value; }
        }

        /// <summary>
        /// inititalizes the lists on creation of the object
        /// </summary>
        public FileMngr()
        {
            files = new List<string>();
            patterns = new List<string>();
            projectList = new List<ProjectTree>();
        }

        /// <summary>
        /// returns the project tree files
        /// </summary>
        /// <returns></returns>
        public List<ProjectTree> getFiles()
        {
            return projectList;
        }

        /// <summary>
        /// Creates a project tree structure by combining all the files under a directory into a project
        /// </summary>
        public void createProjectTree()
        {
            for (int i = 0; i < files.Count; ++i)
            {
                bool parentpresent = false;
                string currentFilename = System.IO.Path.GetFileName(files[i]);
                string parentDirName = System.IO.Path.GetDirectoryName(files[i]);
                string relativePath = getRelativePath(files[i]);
                if (relativePath.Contains("\\"))
                {
                    int index = relativePath.LastIndexOf("\\");
                    relativePath = relativePath.Substring(0, index);
                }
                else
                    relativePath = "Current Dir";


                for (int j = 0; j < projectList.Count; j++)
                {
                    ProjectTree temp = projectList[j];
                    if (temp.parentAbsolutePath == parentDirName)
                    {
                        parentpresent = true;
                        temp.filesList.Add(currentFilename);
                    }
                }
                if (!parentpresent)
                {
                    ProjectTree currentTree = new ProjectTree();
                    currentTree.parentAbsolutePath = parentDirName;
                    currentTree.parentRelativePath = relativePath;
                    currentTree.filesList.Add(currentFilename);
                    projectList.Add(currentTree);
                }
            }
        }

        /// <summary>
        /// Gets the relative path for a file
        /// </summary>
        /// <param name="f">the full file path</param>
        /// <returns>the relative path</returns>
        public string getRelativePath(string f)
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo(f);
            string path = "";

            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;

            if (!fullFile.StartsWith(fullDirectory))
            {
                Console.WriteLine("Unable to make relative path");
            }
            else
            {
                // The +1 is to avoid the directory separator
                path = fullFile.Substring(fullDirectory.Length + 1);
                Console.WriteLine("Relative path: {0}", path);
            }
            return path;
        }

        /// <summary>
        /// get the list of files for each path provided
        /// </summary>
        /// <param name="path">the path for which the file name has to be provided</param>
        public void findFiles(string path)
        {
            try
            {
                foreach (string pattern in patterns)
                {
                    string[] newFiles = Directory.GetFiles(path, pattern);
                    ArrayList filesAL = new ArrayList();
                    for (int i = 0; i < newFiles.Length; ++i)
                    {
                        if (!newFiles[i].Contains("Temporary") && !newFiles[i].Contains("AssemblyInfo.cs"))
                            filesAL.Add(Path.GetFullPath(newFiles[i]));
                    }
                    files.AddRange((String[])filesAL.ToArray(typeof(string)));
                }
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir);
                }
            }
            catch (DirectoryNotFoundException) { Console.WriteLine("\nThe provided directory cannot be found {0}\n", path); }
            catch (FileNotFoundException) { Console.WriteLine("\nThe provided file cannot be found {0}\n", path); }
            catch (FileLoadException) { Console.WriteLine("\nError Loading file {0}\n", path); }
            catch (PathTooLongException) { Console.WriteLine("\nThe provided path is too long to read. Try moving the file to another location {0}\n", path); }
            catch (Exception) { Console.WriteLine("\nError in {0}\n", path); }
        }
    }

    /// <summary>
    /// test stub for the file
    /// </summary>
#if(TEST_FILEMGR)
    class TestFileMgr
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Testing File Manager Class");
            Console.Write("\n =======================\n");

            FileMngr fm = new FileMngr();
            fm.Patterns.Add("*.cs");
            fm.findFiles(".");
            foreach (string file in fm.Files)
            {
                Console.Write("\n  {0}", file);
            }
            Console.Write("\n\n");
            Console.ReadLine();
        }
    }
#endif
}
