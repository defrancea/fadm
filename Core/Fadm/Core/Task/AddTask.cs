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
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
                // Read the document from file system
                XDocument document;
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    // Load the document
                    document = XDocument.Load(stream);

                    // Import project cached query
                    var importProject = (
                    from i in document
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Project")
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Import")
                    where @"$(MSBuildToolsPath)\Microsoft.CSharp.targets" == (string)i.Attribute("Project")
                    select i).ToArray();

                    // After build cached query
                    var afterBuild = (
                    from t in document
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Project")
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Target")
                    where "AfterBuild" == (string)t.Attribute("Name")
                    select t).ToArray();

                    // Fadm execute cached query
                    var afterBuildExec = (
                    from e in afterBuild
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Exec")
                    where "Fadm install $(TargetPath)" == (string)e.Attribute("Command")
                    select e).ToArray();

                    // Detect if the file already contains Fadm after build operation
                    if (importProject.Any() && afterBuildExec.Any())
                    {
                        return new ExecutionResult(ExecutionResultStatus.Warning, string.Format("Fadm already in '{0}'", path));
                    }

                    // Generate build import
                    if (!importProject.Any())
                    {
                        document
                            .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Project")
                            .First()
                            .Add(importProject = new [] { new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}Import", new XAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets")) });
                    }

                    // Generate target node if needed
                    if (!afterBuild.Any())
                    {
                        document
                            .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}Project")
                            .First()
                            .Add(afterBuild = new XElement[] { new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}Target", new XAttribute("Name", "AfterBuild")) });
                    }

                    // Generate exec node if needed
                    if (!afterBuildExec.Any())
                    {
                        afterBuild
                            .First()
                            .Add(afterBuildExec = new XElement[] { new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}Exec", new XAttribute("Command", "Fadm install $(TargetPath)")) });
                    }
                }

                // Save the document to the file system
                if (null != document)
                {
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