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

namespace Fadm.Core.Task
{
    /// <summary>
    /// The add task in charge of adding Fadm to solutions and project files.
    /// </summary>
    public class AddTask : IAddTask
    {
        /// <summary>
        /// Adds Fadm installer to a solution or a project file.
        /// </summary>
        /// <param name="path">The file to add the installer to.</param>
        /// <returns>The execution result.</returns>
        public ExecutionResult Add(string path)
        {
            // Validate file existence
            if (!File.Exists(path))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", path));
            }

            // If the file is not a solution file, directly process the file
            string extension = Path.GetExtension(path);
            if (".sln" != extension)
            {
                ProcessProjectFile(path);
            }

            // Process the solution file in order to determine projects to process
            Regex solutionProjectRegex = new Regex(@"^Project\(""{(.+)}""\)\ *=\ *""(.+)""\ *,\ *""(.+)""\ *,\ *""{(.+)}""$");
            List<ExecutionResult> projectProcessingExecutionResult = new List<ExecutionResult>();
            using (TextReader reader = new StreamReader(path))
            {
                // Read the file line per line
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // In we find a math, extract the third group (project's file) and process it
                    Match m = solutionProjectRegex.Match(line);
                    if (m.Success)
                    {
                        string projectFile = m.Groups[3].Value;
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
                namespaceManager.AddNamespace("ns", @"http://schemas.microsoft.com/developer/msbuild/2003");

                // Select "AfterBuild" definition
                XmlNode postBuildEventNode = document.SelectSingleNode(@"//ns:Project/ns:Target[@Name='AfterBuild']", namespaceManager);

                if (null == postBuildEventNode)
                {
                    // Inject Target node directly inside Project node
                    XmlElement afterBuildEventNode = document.CreateElement("Target", @"http://schemas.microsoft.com/developer/msbuild/2003");
                    afterBuildEventNode.SetAttribute("Name", "AfterBuild");
                    document.DocumentElement.AppendChild(afterBuildEventNode);

                    // Inject Exec mode inside the Target node
                    XmlElement execElementNode = document.CreateElement("Exec", @"http://schemas.microsoft.com/developer/msbuild/2003");
                    execElementNode.SetAttribute("Command", @"Fadm install $(TargetPath)");
                    afterBuildEventNode.AppendChild(execElementNode);

                    // Save the file to the file system
                    document.Save(path);
                }

                // Return the execution reuslt
                return new ExecutionResult(ExecutionResultStatus.Success, string.Format("Fadm added to '{0}'", path));
            }
            catch (Exception exception)
            {
                //  Return the execution result
                return new ExecutionResult(ExecutionResultStatus.Error, exception.Message);
            }
        }
    }
}