/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using HdlgFileProperty;
using Serilog;
using System.Diagnostics;
using System.Globalization;

namespace HDLG_winforms
{
    public partial class BrowserForm : Form
    {
        private readonly string rootDirectory;
        private readonly FilePropertyBrowser propertyBrowser;
        private readonly ILogger logger;

        public BrowserForm(string rootDirectory, FilePropertyBrowser propertyBrowser, ILogger logger)
        {
            InitializeComponent();
            this.rootDirectory = rootDirectory;
            this.propertyBrowser = propertyBrowser;
            this.logger = logger;
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {
            try
            {
                var rootNode = new TreeNode(rootDirectory);
                rootNode.Tag = new NodeInfo { IsDirectory = true, Path = rootDirectory };
                rootNode.Nodes.Add(new TreeNode("Loading..."));
                treeView1.Nodes.Add(rootNode);
                rootNode.Expand();
            }
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
			catch (Exception ex)
#pragma warning restore CA1031
			{
				logger.Error(ex, "Error loading root directory in BrowserForm");
                MessageBox.Show(this, "An error occurred while loading the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class NodeInfo
        {
            public bool IsDirectory { get; set; }
            public string Path { get; set; } = string.Empty;
        }

        /// <summary>
        /// Validates that a resolved path stays within the root directory to prevent path traversal attacks
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <returns>True if the path is within the root directory</returns>
        private bool IsPathWithinRoot(string path)
        {
            string resolvedPath = Path.GetFullPath(path);
            string resolvedRoot = Path.GetFullPath(rootDirectory);

            // Ensure root ends with separator for prefix comparison
            if (!resolvedRoot.EndsWith(Path.DirectorySeparatorChar))
            {
                resolvedRoot += Path.DirectorySeparatorChar;
            }

            return resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase)
                || string.Equals(resolvedPath, Path.GetFullPath(rootDirectory), StringComparison.OrdinalIgnoreCase);
        }

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null || e.Node.Tag is not NodeInfo info || !info.IsDirectory)
                return;

            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {
                e.Node.Nodes.Clear();
                Cursor = Cursors.WaitCursor;

                try
                {
                    if (!IsPathWithinRoot(info.Path))
                    {
                        logger.Warning($"Path traversal blocked: {info.Path} is outside root directory {rootDirectory}");
                        e.Node.Nodes.Add(new TreeNode("Access Denied"));
                        return;
                    }

                    var dirInfo = new DirectoryInfo(info.Path);

                    var dirNodes = new List<TreeNode>();
                    foreach (var dir in dirInfo.EnumerateDirectories())
                    {
                        var node = new TreeNode(dir.Name);
                        node.Tag = new NodeInfo { IsDirectory = true, Path = dir.FullName };
                        node.Nodes.Add(new TreeNode("Loading..."));
                        dirNodes.Add(node);
                    }
                    if (dirNodes.Count > 0) e.Node.Nodes.AddRange(dirNodes.ToArray());

                    var fileNodes = new List<TreeNode>();
                    foreach (var file in dirInfo.EnumerateFiles())
                    {
                        var node = new TreeNode(file.Name);
                        node.Tag = new NodeInfo { IsDirectory = false, Path = file.FullName };
                        fileNodes.Add(node);
                    }
                    if (fileNodes.Count > 0) e.Node.Nodes.AddRange(fileNodes.ToArray());
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Warning(ex, $"Access denied to directory: {info.Path}");
                    e.Node.Nodes.Add(new TreeNode("Access Denied"));
                }
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
				catch (Exception ex)
#pragma warning restore CA1031
				{
					logger.Error(ex, $"Error loading directory: {info.Path}");
                    e.Node.Nodes.Add(new TreeNode("Error"));
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Localization", "CA1303:Do not pass literals as localized parameters")]
        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listViewProperties.Items.Clear();
            btnOpenFile.Enabled = false;

            if (e.Node == null || e.Node.Tag is not NodeInfo info)
            {
                lblSelectedFileName.Text = "Select a file to view properties";
                return;
            }

            if (info.IsDirectory)
            {
                var dirInfo = new DirectoryInfo(info.Path);
                lblSelectedFileName.Text = dirInfo.Name;
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                btnOpenFile.Enabled = true;

                var fileInfo = new FileInfo(info.Path);

                if (!IsPathWithinRoot(info.Path))
                {
                    logger.Warning($"Path traversal blocked: {info.Path} is outside root directory {rootDirectory}");
                    AddPropertyToListView("Error", "Access denied: path is outside the root directory.");
                    btnOpenFile.Enabled = false;
                    return;
                }

                lblSelectedFileName.Text = fileInfo.Name;

                AddPropertyToListView("Name", fileInfo.Name);
                AddPropertyToListView("Path", fileInfo.FullName);
                AddPropertyToListView("Extension", fileInfo.Extension);
                AddPropertyToListView("Size (bytes)", fileInfo.Length.ToString(CultureInfo.CurrentCulture));
                AddPropertyToListView("Creation Time", fileInfo.CreationTime.ToString("g", CultureInfo.CurrentCulture));
                AddPropertyToListView("Last Access Time", fileInfo.LastAccessTime.ToString("g", CultureInfo.CurrentCulture));
                AddPropertyToListView("Last Write Time", fileInfo.LastWriteTime.ToString("g", CultureInfo.CurrentCulture));

                var props = propertyBrowser.GetFileProperty(info.Path);
                if (props != null)
                {
                    foreach (var kvp in props)
                    {
                        AddPropertyToListView(kvp.Key, kvp.Value?.ToString() ?? "");
                    }
                }
            }
#pragma warning disable CA1031 // Ne pas attraper les types d'exception généraux
			catch (Exception ex)
#pragma warning restore CA1031
			{
				logger.Error(ex, $"Error reading properties for file: {info.Path}");
                AddPropertyToListView("Error", ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AddPropertyToListView(string name, string value)
        {
            var item = new ListViewItem(name);
            item.SubItems.Add(value);
            listViewProperties.Items.Add(item);
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode?.Tag is NodeInfo info && !info.IsDirectory)
            {
                MainWindow.OpenWithDefaultProgram(info.Path);
            }
        }
    }
}
