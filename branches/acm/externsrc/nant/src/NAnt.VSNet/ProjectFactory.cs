// NAnt - A .NET build tool
// Copyright (C) 2001-2003 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Dmitry Jemerov <yole@yole.ru>

using System;
using System.Xml;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Util;

using NAnt.VSNet.Tasks;

namespace NAnt.VSNet {
    /// <summary>
    /// Factory class for VS.NET projects.
    /// </summary>
    public sealed class ProjectFactory {
        #region Private Instance Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFactory" />
        /// class.
        /// </summary>
        private ProjectFactory() {
        }

        #endregion Private Instance Constructor

        #region Static Constructor

        static ProjectFactory() {
            ClearCache();
        }

        #endregion Static Constructor

        #region Public Static Methods

        public static void ClearCache() {
            _cachedProjects = CollectionsUtil.CreateCaseInsensitiveHashtable();
            _cachedProjectGuids = CollectionsUtil.CreateCaseInsensitiveHashtable();
            _cachedProjectXml = CollectionsUtil.CreateCaseInsensitiveHashtable();
        }

        public static XmlDocument LoadProjectXml(string path) {
            if (!_cachedProjectXml.Contains(path)) {
                XmlDocument doc = new XmlDocument();

                if (!ProjectFactory.IsUrl(path)) {
                    using (StreamReader sr = new StreamReader(path, Encoding.Default, true)) {
                        doc.Load(sr);
                    }
                } else {
                    Uri uri = new Uri(path);
                    if (uri.Scheme == Uri.UriSchemeFile) {
                        using (StreamReader sr = new StreamReader(uri.LocalPath, Encoding.Default, true)) {
                            doc.Load(sr);
                        }
                    } else {
                        doc.LoadXml(WebDavClient.GetFileContentsStatic(path));
                    }
                }
            
                _cachedProjectXml[path] = doc;
            }
            
            return (XmlDocument) _cachedProjectXml[path];
        }    

        public static ProjectBase LoadProject(SolutionBase solution, SolutionTask solutionTask, TempFileCollection tfc, GacCache gacCache, ReferencesResolver referencesResolver, DirectoryInfo outputDir, string path) {
            // check if this a new project
            if (!_cachedProjects.Contains(path)) {
                ProjectBase project = CreateProject(solution, solutionTask, 
                    tfc, gacCache, referencesResolver, outputDir, path);
                _cachedProjects[path] = project;
            }

            return (ProjectBase) _cachedProjects[path];
        }

        public static bool IsUrl(string fileName) {
            if (fileName.StartsWith(Uri.UriSchemeFile) || fileName.StartsWith(Uri.UriSchemeHttp) || fileName.StartsWith(Uri.UriSchemeHttps)) {
                return true;
            }

            return false;
        }

        public static bool IsSupportedProjectType(string path) {
            string projectFileName = ProjectFactory.GetProjectFileName(path);
            string projectExt = Path.GetExtension(projectFileName).ToLower(
                CultureInfo.InvariantCulture);
            return projectExt == ".vbproj" || projectExt == ".csproj" 
                || projectExt == ".vcproj" || projectExt == ".vjsproj";
        }

        public static string LoadGuid(string fileName) {
            // check if a project with specified file is already cached
            if (_cachedProjects.ContainsKey(fileName)) {
                // return the guid of the cached project
                return ((ProjectBase) _cachedProjects[fileName]).Guid;
            }

            string projectFileName = ProjectFactory.GetProjectFileName(fileName);
            string projectExt = Path.GetExtension(projectFileName).ToLower(
                CultureInfo.InvariantCulture);

            // check if GUID of project is already cached
            if (!_cachedProjectGuids.Contains(fileName)) {
                if (projectExt == ".vbproj" || projectExt == ".csproj" || projectExt == ".vjsproj") {
                    // add project GUID to cache
                    _cachedProjectGuids[fileName] = ManagedProjectBase.LoadGuid(fileName);
                } else if (projectExt == ".vcproj") {
                    // add project GUID to cache
                    _cachedProjectGuids[fileName] = VcProject.LoadGuid(fileName);
                } else {
                    throw new BuildException(string.Format(CultureInfo.InvariantCulture, 
                        "Unknown project file extension '{0}'.", projectExt,
                        Location.UnknownLocation));
                }
            }

            // return project GUID from cache
            return (string) _cachedProjectGuids[fileName];
        }

        #endregion Public Static Methods

        #region Private Static Methods

        private static ProjectBase CreateProject(SolutionBase solution, SolutionTask solutionTask, TempFileCollection tfc, GacCache gacCache, ReferencesResolver referencesResolver, DirectoryInfo outputDir, string projectPath) {
            // determine the filename of the project
            string projectFileName = ProjectFactory.GetProjectFileName(projectPath);

            // determine the extension of the project file
            string projectExt = Path.GetExtension(projectFileName).ToLower(
                CultureInfo.InvariantCulture);

            // holds the XML definition of the project
            XmlElement xmlDefinition;

            try {
                XmlDocument doc = LoadProjectXml(projectPath);
                xmlDefinition = doc.DocumentElement;
            } catch (Exception ex) {
                throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                    "Error loading project '{0}'.", projectPath), Location.UnknownLocation, 
                    ex);
            }

            // first identify project based on known file extensions
            switch (projectExt) {
                case ".vbproj":
                    return new VBProject(solution, projectPath, xmlDefinition, 
                        solutionTask, tfc, gacCache, referencesResolver, 
                        outputDir);
                case ".csproj":
                    return new CSharpProject(solution, projectPath, xmlDefinition, 
                        solutionTask, tfc, gacCache, referencesResolver, 
                        outputDir);
                case ".vjsproj":
                    return new JSharpProject(solution, projectPath, xmlDefinition,
                        solutionTask, tfc, gacCache, referencesResolver,
                        outputDir);
                case ".vcproj":
                    return new VcProject(solution, projectPath, xmlDefinition, 
                        solutionTask, tfc, gacCache, referencesResolver, 
                        outputDir);
            }

            // next, identify project based on XML definition
            if (VBProject.IsSupported(xmlDefinition)) {
                return new VBProject(solution, projectPath, xmlDefinition, 
                    solutionTask, tfc, gacCache, referencesResolver, outputDir);
            } else if (CSharpProject.IsSupported(xmlDefinition)) {
                return new CSharpProject(solution, projectPath, xmlDefinition, 
                    solutionTask, tfc, gacCache, referencesResolver, outputDir);
            } else if (JSharpProject.IsSupported(xmlDefinition)) {
                return new JSharpProject(solution, projectPath, xmlDefinition, 
                    solutionTask, tfc, gacCache, referencesResolver, outputDir);
            } else if (VcProject.IsSupported(xmlDefinition)) {
                return new VcProject(solution, projectPath, xmlDefinition, 
                    solutionTask, tfc, gacCache, referencesResolver, outputDir);
            }

            // either the project file is invalid or we don't support it
            throw new BuildException(string.Format(CultureInfo.InvariantCulture,
                "Project '{0}' is invalid or not supported (at this time).",
                projectPath), Location.UnknownLocation);
        }

        private static string GetProjectFileName(string fileName) {
            string projectPath = null;

            if (ProjectFactory.IsUrl(fileName)) {
                // construct uri for project path
                Uri projectUri = new Uri(fileName);

                // get last segment of the uri (which should be the 
                // project file itself)
                projectPath = projectUri.LocalPath;
            } else {
                projectPath = fileName;
            }

            // return filename part
            return Path.GetFileName(projectPath); 
        }

        #endregion Private Static Methods

        #region Private Static Fields

        /// <summary>
        /// Holds a case-insensitive list of cached projects.
        /// </summary>
        /// <remarks>
        /// The key of the <see cref="Hashtable" /> is the path of the project
        /// file (for web projects this can be a URL) and the value is a 
        /// <see cref="Project" /> instance.
        /// </remarks>
        private static Hashtable _cachedProjects;

        /// <summary>
        /// Holds a case-insensitive list of cached project GUIDs.
        /// </summary>
        /// <remarks>
        /// The key of the <see cref="Hashtable" /> is the path of the project
        /// file (for web projects this can be a URL) and the value is the GUID
        /// of the project.
        /// </remarks>
        private static Hashtable _cachedProjectGuids;

        /// <summary>
        /// Holds a case-insensitive list of cached project GUIDs.
        /// </summary>
        /// <remarks>
        /// The key of the <see cref="Hashtable" /> is the path of the project
        /// file (for web projects this can be a URL) and the value is the Xml
        /// of the project.
        /// </remarks>
        private static Hashtable _cachedProjectXml;

        #endregion Private Static Fields
    }
}
