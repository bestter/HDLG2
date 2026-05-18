using HdlgFileProperty;
using Serilog.Core;
using System.Diagnostics;
using System.Globalization;

namespace HDLG_winforms
{
    public partial class BrowserForm : Form
    {
        private readonly string rootDirectory;
        private readonly FilePropertyBrowser propertyBrowser;
        private readonly Logger logger;

        public BrowserForm(string rootDirectory, FilePropertyBrowser propertyBrowser, Logger logger)
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
            catch (Exception ex)
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
                catch (Exception ex)
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
            catch (Exception ex)
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
