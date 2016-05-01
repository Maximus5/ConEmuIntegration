﻿//
// Copyright 2016 David Roller
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using ConEmuIntegration.ConEmu;
using ConEmuIntegration.ToolWindow;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace ConEmuIntegration.SolutionExplorer
{
    internal sealed class OpenInConEmu
    {
        public void Open()
        {
            if(ProductEnvironment.Instance.Package == null)
            {
                return;
            }

            var provider = ProductEnvironment.Instance.Package as IServiceProvider;
            var dte = provider.GetService(typeof(SDTE)) as DTE2;
            if (dte.SelectedItems.Count <= 0)
            {
                return;
            }

            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.Project != null)
                {
                    // C# Project has the FullPath property
                    if (FolderOfProject(selectedItem, "FullPath"))
                    {
                        return;
                    }

                    // C++ Project has the ProjectFile property
                    if (FolderOfProject(selectedItem, "ProjectFile"))
                    {
                        return;
                    }
                }

                if (selectedItem.ProjectItem != null)
                {
                    if (FolderOfProjectItem(selectedItem))
                    {
                        return;
                    }
                }
            }
        }

        private bool FolderOfProjectItem(SelectedItem selectedItem)
        {
            if (selectedItem.ProjectItem == null)
            {
                return false;
            }

            var projectItem = selectedItem.ProjectItem;
            var fullPathProperty = projectItem.Properties.Item("FullPath");

            if (fullPathProperty == null)
            {
                return false;
            }

            var fullPath = new FileInfo(fullPathProperty.Value.ToString());
            ExecuteGuiMacro("Print(@\"cd " + 
                fullPath.Directory.FullName.Replace("\"", "\"\"") + "\",\"\n\")");

            return true;
        }

        private void ExecuteGuiMacro(string macro)
        {
            if (ProductEnvironment.Instance.ConEmuProcess == null)
            {
                return;
            }

            string conemu = ProductEnvironment.Instance.GetConEmuLibrary();
            var macroHelper = new ConEmuMacro(conemu);
            macroHelper.Execute(ProductEnvironment.Instance.ConEmuProcess.Id.ToString(), macro);
        }

        private bool HasProperty(Properties properties, string propertyName)
        {
            if (properties != null)
            {
                foreach (Property item in properties)
                {
                    if (item != null && item.Name == propertyName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FolderOfProject(SelectedItem selectedItem, string property)
        {
            if (selectedItem.Project == null)
            {
                return false;
            }

            var project = selectedItem.Project;
            if (HasProperty(project.Properties, property) == false)
            {
                return false;
            }

            var fullPathProperty = project.Properties.Item(property);
            if (fullPathProperty == null)
            {
                return false;
            }

            var fullPath = new FileInfo(fullPathProperty.Value.ToString());
            ExecuteGuiMacro("Print(@\"cd " + 
                fullPath.Directory.FullName.Replace("\"", "\"\"") + "\",\"\n\")");

            return true;
        }
    }
}
