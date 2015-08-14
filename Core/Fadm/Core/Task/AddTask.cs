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
using System.Threading.Tasks;
using System.Xml.Linq;
using Fadm.Utilities;

namespace Fadm.Core.FadmTask
{
    /// <summary>
    /// The add task in charge of adding Fadm to solutions and project files.
    /// </summary>
    public class AddTask : ITask
    {
        /// <summary>
        /// Identifies a Project node in csproj file.
        /// </summary>
        private const string NODE_PROJECT = "{http://schemas.microsoft.com/developer/msbuild/2003}Project";

        /// <summary>
        /// Identifies an Import node in csproj file.
        /// </summary>
        private const string NODE_IMPORT = "{http://schemas.microsoft.com/developer/msbuild/2003}Import";

        /// <summary>
        /// Identifies a Target node in csproj file.
        /// </summary>
        private const string NODE_TARGET = "{http://schemas.microsoft.com/developer/msbuild/2003}Target";

        /// <summary>
        /// Identifies an Exec node in csproj file.
        /// </summary>
        private const string NODE_EXEC = "{http://schemas.microsoft.com/developer/msbuild/2003}Exec";

        /// <summary>
        /// Sql regex extracting projects information.
        /// </summary>
        private Regex slnRegex;

        /// <summary>
        /// The target file path.
        /// </summary>
        private string targetfilepath;

        /// <summary>
        /// Initializes a new instance of <see cref="AddTask"/>.
        /// <param name="path">The file to add the installer to.</param>
        /// </summary>
        public AddTask(string path)
        {
            // Input validation
            Validate.IsNotNullOrWhitespace(path, "path must not be null.");

            // Initialize
            slnRegex = new Regex(@"^Project\(""{(.+)}""\)\ *=\ *""(.+)""\ *,\ *""(.+)""\ *,\ *""{(.+)}""$", RegexOptions.Compiled);
            targetfilepath = path;
        }

        /// <summary>
        /// Executes Fadm installer to a solution or a project file.
        /// </summary>
        /// <returns>The execution result.</returns>
        public async Task<ExecutionResult> ExecuteAsync()
        {
            // Validate file existence
            if (!File.Exists(targetfilepath))
            {
                return new ExecutionResult(ExecutionResultStatus.Error, string.Format("The file '{0}' doesn't exist", targetfilepath));
            }

            try
            {
                // If the file is not a solution file, directly process the file
                string extension = Path.GetExtension(targetfilepath);
                if (".sln" != extension)
                {
                    return await ProcessProjectFileAsync(targetfilepath);
                }

                // Parse the solution file in order to extract the file to process and close the file as soon as possible
                List<string> fileToProcess = new List<string>();
                using (TextReader reader = new StreamReader(targetfilepath))
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
                            string filePath = Path.Combine(Path.GetDirectoryName(targetfilepath), projectFile);

                            // sln file Could reference directories as project to organize the solution.
                            // There is no way to differencate both definitions so testing that the file exists is required.
                            if (File.Exists(filePath))
                            {
                                fileToProcess.Add(filePath);
                            }
                        }
                    }
                }

                // Start processing
                List<Task<ExecutionResult>> tasks = new List<Task<ExecutionResult>>();
                foreach (string filePath in fileToProcess)
                {
                    tasks.Add(this.ProcessProjectFileAsync(filePath));
                }

                // Return execution result
                await Task.WhenAll(tasks);
                return new ExecutionResult(ExecutionResultStatus.Success, string.Format("Solution '{0}' processed", targetfilepath), (from r in tasks select r.Result).ToArray());
            }

            // Report error if any
            catch (Exception exception)
            {
                return new ExecutionResult(ExecutionResultStatus.Error, exception.Message);
            }

        }

        /// <summary>
        /// Processes project file injecting Famd install in the build process.
        /// </summary>
        /// <param name="path">The file to process.</param>
        /// <returns>The execution result.</returns>
        private async Task<ExecutionResult> ProcessProjectFileAsync(string path)
        {
            try
            {
                // Read the document from file system
                XDocument document;
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Load the document
                    string content = await reader.ReadToEndAsync();
                    document = XDocument.Parse(content);
                }

                // Import project cached query
                var importProject = (
                from i in document
                    .Descendants(NODE_PROJECT)
                    .Descendants(NODE_IMPORT)
                where @"$(MSBuildToolsPath)\Microsoft.CSharp.targets" == (string)i.Attribute("Project")
                select i).ToArray();

                // After build cached query
                var afterBuild = (
                from t in document
                    .Descendants(NODE_PROJECT)
                    .Descendants(NODE_TARGET)
                where "AfterBuild" == (string)t.Attribute("Name")
                select t).ToArray();

                // Fadm execute cached query
                var afterBuildExec = (
                from e in afterBuild
                    .Descendants(NODE_EXEC)
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
                        .Descendants(NODE_PROJECT)
                        .First()
                        .Add(importProject = new[] { new XElement(NODE_IMPORT, new XAttribute("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets")) });
                }

                // Generate target node if needed
                if (!afterBuild.Any())
                {
                    document
                        .Descendants(NODE_PROJECT)
                        .First()
                        .Add(afterBuild = new[] { new XElement(NODE_TARGET, new XAttribute("Name", "AfterBuild")) });
                }

                // Generate exec node if needed
                if (!afterBuildExec.Any())
                {
                    afterBuild
                        .First()
                        .Add(afterBuildExec = new[] { new XElement(NODE_EXEC, new XAttribute("Command", "Fadm install $(TargetPath)")) });
                }

                // Save the document to the file system
                if (null != document)
                {
                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(document.ToString());
                    }
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