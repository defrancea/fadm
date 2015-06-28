/*
 * Copyright (c) 2015, Fadm. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Fadm.Utilities;

namespace Fadm.Core.Task
{
    /// <summary>
    /// The add task in charge of adding Fadm to solutions and project files.
    /// </summary>
    public class AddTask : IAddTask
    {
        /// <summary>
        /// Sql regex extracting projects information.
        /// </summary>
        private Regex slnRegex;

        /// <summary>
        /// MSBUild descriptor's namespace.
        /// </summary>
        private string msBuildNamespace;
        
        /// <summary>
        /// Initializes a new instance of <see cref="slnRegex"/>.
        /// </summary>
        public AddTask()
        {
            slnRegex = new Regex(@"^Project\(""{(.+)}""\)\ *=\ *""(.+)""\ *,\ *""(.+)""\ *,\ *""{(.+)}""$", RegexOptions.Compiled);
            msBuildNamespace = @"http://schemas.microsoft.com/developer/msbuild/2003";
        }

        /// <summary>
        /// Adds Fadm installer to a solution or a project file.
        /// </summary>
        /// <param name="path">The file to add the installer to.</param>
        /// <returns>The execution result.</returns>
        public ExecutionResult Add(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Validate file existence
            if (!File.Exists(path))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", path));
            }

            // If the file is not a solution file, directly process the file
            string extension = Path.GetExtension(path);
            if (".sln" != extension)
            {
                return ProcessProjectFile(path);
            }

            // Process the solution file in order to determine projects to process
            List<ExecutionResult> projectProcessingExecutionResult = new List<ExecutionResult>();
            using (TextReader reader = new StreamReader(path))
            {
                // Read the file line per line
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // In we find a math, extract the third group (project's file) and process it
                    Match match = slnRegex.Match(line);
                    if (match.Success)
                    {
                        string projectFile = match.Groups[3].Value;
                        string filePath = Path.Combine(Path.GetDirectoryName(path), projectFile);

                        // sln file Could reference directories as project to organize the solution.
                        // There is no way to differencate both definitions so testing that the file exists is required.
                        if (File.Exists(filePath))
                        {
                            projectProcessingExecutionResult.Add(this.ProcessProjectFile(filePath));
                        }
                    }
                }
            }

            // Return execution result.
            return new ExecutionResult(ExecutionResultStatus.Success, string.Format("Solution '{0}' processed", path), projectProcessingExecutionResult.ToArray());
        }

        /// <summary>
        /// Processes project file injecting Famd install in the build process.
        /// </summary>
        /// <param name="path">The file to process.</param>
        /// <returns></returns>
        private ExecutionResult ProcessProjectFile(string path)
        {
            try
            {
                // Parse the project as a xml document
                XmlDocument document = new XmlDocument();
                document.Load(path);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("ns", msBuildNamespace);

                // Detect if the file already contains Fadm after build operation
                if (IsFadmAdded(path, namespaceManager))
                {
                    return new ExecutionResult(ExecutionResultStatus.Warning, string.Format("Fadm already in '{0}'", path));
                }

                // Generate build import
                EnsureBuildImport(document, namespaceManager);

                // Generate target node if needed
                XmlNode postBuildEventNode = EnsureTargetAfterBuild(document, namespaceManager);

                // Generate exec if needed
                EnsureTargetAfterBuildExecute(document, postBuildEventNode, namespaceManager);

                // Save the file
                document.Save(path);

                // Return the execution reuslt
                return new ExecutionResult(ExecutionResultStatus.Success, string.Format("Fadm added to '{0}'", path));
            }
            catch (Exception exception)
            {
                //  Return the execution result
                return new ExecutionResult(ExecutionResultStatus.Error, exception.Message);
            }
        }

        /// <summary>
        /// Ensures that the document conains an after build target.
        /// </summary>
        /// <param name="document">The xml document.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <returns>After build target node.</returns>
        private XmlNode EnsureBuildImport(XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            // Retrieve the node using xpath
            XmlNode buildImportNode = document.SelectSingleNode(@"//ns:Project/ns:Import[@Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets']", namespaceManager);

            // Return it as it
            if (null != buildImportNode)
            {
                return buildImportNode;
            }

            // Or create it if it doesn't exist
            else
            {
                XmlElement createdBuildImportNode = document.CreateElement("Import", msBuildNamespace);
                createdBuildImportNode.SetAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets");
                document.DocumentElement.AppendChild(createdBuildImportNode);
                return createdBuildImportNode;
            }
        }

        /// <summary>
        /// Ensures that the document conains an after build target.
        /// </summary>
        /// <param name="document">The xml document.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <returns>After build target node.</returns>
        private XmlNode EnsureTargetAfterBuild(XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            // Retrieve the node using xpath
            XmlNode postBuildEventNode = document.SelectSingleNode(@"//ns:Project/ns:Target[@Name='AfterBuild']", namespaceManager);

            // Return it as it
            if (null != postBuildEventNode)
            {
                return postBuildEventNode;
            }

            // Or create it if it doesn't exist
            else
            {
                XmlElement afterBuildEventNode = document.CreateElement("Target", msBuildNamespace);
                afterBuildEventNode.SetAttribute("Name", "AfterBuild");
                document.DocumentElement.AppendChild(afterBuildEventNode);
                return afterBuildEventNode;
            }
        }

        /// <summary>
        /// Ensures that the exec after build target node contains the Fadm exec command.
        /// </summary>
        /// <param name="document">The xml document.</param>
        /// <param name="node">The node to add the exec command.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <returns></returns>
        private XmlNode EnsureTargetAfterBuildExecute(XmlDocument document, XmlNode node, XmlNamespaceManager namespaceManager)
        {
            // Retrieve the node using xpath
            XmlNode execNode = node.SelectSingleNode(@"ns:Exec[@Command='Fadm install $(TargetPath)']", namespaceManager);

            // Return it as it
            if (null != execNode)
            {
                return execNode;
            }

            // Or create it if it doesn't exist
            else
            {
                XmlElement execElementNode = document.CreateElement("Exec", msBuildNamespace);
                execElementNode.SetAttribute("Command", @"Fadm install $(TargetPath)");
                node.AppendChild(execElementNode);
                return execElementNode;
            }
        }

        /// <summary>
        /// Determines wheter Fadm is already installed on this file.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <returns>True if already installed, False otherwise.</returns>
        private bool IsFadmAdded(string path, XmlNamespaceManager namespaceManager)
        {
            // Parse the project as a xml document
            XmlDocument document = new XmlDocument();
            document.Load(path);

            // After build execution added by Fadm
            XmlNode postBuildEventNode = document.SelectSingleNode(@"//ns:Project/ns:Target[@Name='AfterBuild']/ns:Exec[@Command='Fadm install $(TargetPath)']", namespaceManager);
            XmlNode buildImportNode = document.SelectSingleNode(@"//ns:Project/ns:Import[@Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets']", namespaceManager);

            // Return true if both nodes are in the document
            return (null != postBuildEventNode && null != buildImportNode);
        }
    }
}