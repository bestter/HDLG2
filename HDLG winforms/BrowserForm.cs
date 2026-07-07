/*
 This file is part of HTML Directory List Generator.

HTML Directory List Generator is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

HTML Directory List Generator is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with HTML Directory List Generator. If not, see <https://www.gnu.org/licenses/>. 
 */
using HdlgFileProperty;
using Krypton.Toolkit;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.Security;

namespace HDLG_winforms
{
	public partial class BrowserForm : KryptonForm
	{
		private readonly string rootDirectory;
		private readonly string _resolvedRootDirectory;
		private readonly string _resolvedRootDirectoryWithSeparator;
		private readonly FilePropertyBrowser propertyBrowser;
		private readonly ILogger logger;

		public BrowserForm (string rootDirectory, FilePropertyBrowser propertyBrowser, ILogger logger)
		{
			InitializeComponent( );
			Icon = AppBranding.LoadApplicationIcon();
			AppUiBootstrap.RemoveFormBranding(this);
			this.rootDirectory = rootDirectory;
			this.propertyBrowser = propertyBrowser;
			this.logger = logger;

			// Performance optimization: Cache resolved root directory strings to prevent redundant allocations and
			// I/O operations in hot paths like IsPathWithinRoot when expanding tree nodes.
			_resolvedRootDirectory = Path.GetFullPath(rootDirectory);
			_resolvedRootDirectoryWithSeparator = _resolvedRootDirectory.EndsWith(Path.DirectorySeparatorChar)
				? _resolvedRootDirectory
				: _resolvedRootDirectory + Path.DirectorySeparatorChar;
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
			catch (IOException ex)
			{
				logger.Error(ex, "IO Error loading root directory in BrowserForm");
                MessageBox.Show(this, "An IO error occurred while loading the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
			catch (Exception ex)
			{
				logger.Error(ex, "Error loading root directory in BrowserForm");
				throw;
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
		private bool IsPathWithinRoot (string path)
		{
			string resolvedPath = Path.GetFullPath( path );

			return resolvedPath.StartsWith( _resolvedRootDirectoryWithSeparator, StringComparison.OrdinalIgnoreCase )
				|| string.Equals( resolvedPath, _resolvedRootDirectory, StringComparison.OrdinalIgnoreCase );
		}

		private async void TreeView1_BeforeExpand (object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node == null || e.Node.Tag is not NodeInfo info || !info.IsDirectory)
				return;

			if (e.Node.Nodes.Count == 1 && e.Node.Nodes [0].Text == "Loading...")
			{
				e.Node.Nodes.Clear( );
				Cursor = Cursors.WaitCursor;

				try
				{
					if (!IsPathWithinRoot( info.Path ))
					{
						logger.Warning( "Path traversal blocked: {Path} is outside root directory {RootDirectory}", info.Path, rootDirectory );
						e.Node.Nodes.Add( new TreeNode( "Access Denied" ) );
						return;
					}

					// Performance optimization: Offload expensive I/O directory enumeration to a background thread
					// to prevent blocking the WinForms UI thread, significantly improving perceived performance
					// and preventing layout freezes during folder expansion.
					var (dirInfos, fileInfos) = await Task.Run(() =>
					{
						var dirInfo = new DirectoryInfo( info.Path );

						var dirs = new List<DirectoryInfo>( );
						var files = new List<FileInfo>( );

						foreach (var fsInfo in dirInfo.EnumerateFileSystemInfos( ))
						{
							if (fsInfo is DirectoryInfo dir)
							{
								if ((dir.Attributes & FileAttributes.ReparsePoint) != 0) continue;
								dirs.Add( dir );
							}
							else if (fsInfo is FileInfo file)
							{
								files.Add( file );
							}
						}

						return (dirs, files);
					}).ConfigureAwait(true);

					// Safe WinForms practice: construct TreeNodes on the UI thread after I/O is complete
					// Performance optimization: allocate fixed-size arrays instead of List<T> to avoid ToArray() allocation overhead.
					var dirNodes = new TreeNode[dirInfos.Count];
					var fileNodes = new TreeNode[fileInfos.Count];

					for (int i = 0; i < dirInfos.Count; i++)
					{
						var dir = dirInfos[i];
						var node = new TreeNode( dir.Name );
						node.Tag = new NodeInfo { IsDirectory = true, Path = dir.FullName };
						node.Nodes.Add( new TreeNode( "Loading..." ) );
						dirNodes[i] = node;
					}

					for (int i = 0; i < fileInfos.Count; i++)
					{
						var file = fileInfos[i];
						var node = new TreeNode( file.Name );
						node.Tag = new NodeInfo { IsDirectory = false, Path = file.FullName };
						fileNodes[i] = node;
					}

                    e.Node.TreeView?.BeginUpdate();
                    e.Node.Nodes.AddRange(dirNodes);
                    e.Node.Nodes.AddRange(fileNodes);
                    e.Node.TreeView?.EndUpdate();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Warning(ex, "Access denied to directory: {Path}", info.Path);
                    e.Node.Nodes.Add(new TreeNode("Access Denied"));
                }
                catch (SecurityException ex)
                {
                    logger.Warning(ex, "Security exception accessing directory: {Path}", info.Path);
                    e.Node.Nodes.Add(new TreeNode("Access Denied"));
                }
				catch (IOException ex)
				{
					logger.Error(ex, "IO Error loading directory: {Path}", info.Path);
                    e.Node.Nodes.Add(new TreeNode("IO Error"));
                }
				catch (Exception ex)
				{
					logger.Error(ex, "Error loading directory: {Path}", info.Path);
					throw;
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Localization", "CA1303:Do not pass literals as localized parameters")]
        private async void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Cursor = Cursors.Default;
            listViewProperties.Items.Clear();
            btnOpenFile.Enabled = false;

			if (e.Node == null || e.Node.Tag is not NodeInfo info)
			{
				lblSelectedFileName.Values.Text = "Select a file to view properties";
				return;
			}

			if (info.IsDirectory)
			{
				var dirInfo = new DirectoryInfo( info.Path );
				lblSelectedFileName.Values.Text = dirInfo.Name;
				return;
			}

			Cursor = Cursors.WaitCursor;
			try
			{
				var fileInfo = new FileInfo( info.Path );

				if (!IsPathWithinRoot( info.Path ))
				{
					logger.Warning( "Path traversal blocked: {Path} is outside root directory {RootDirectory}", info.Path, rootDirectory );
					AddPropertyToListView( "Error", "Access denied: path is outside the root directory." );
					btnOpenFile.Enabled = false;
					return;
				}

				lblSelectedFileName.Values.Text = fileInfo.Name;

				listViewProperties.BeginUpdate();
				try
				{
					AddPropertyToListView( "Name", fileInfo.Name );
					AddPropertyToListView( "Path", fileInfo.FullName );
					AddPropertyToListView( "Extension", fileInfo.Extension );
					AddPropertyToListView( "Size (bytes)", fileInfo.Length.ToString( CultureInfo.CurrentCulture ) );
					AddPropertyToListView( "Creation Time", fileInfo.CreationTime.ToString( "g", CultureInfo.CurrentCulture ) );
					AddPropertyToListView( "Last Access Time", fileInfo.LastAccessTime.ToString( "g", CultureInfo.CurrentCulture ) );
					AddPropertyToListView( "Last Write Time", fileInfo.LastWriteTime.ToString( "g", CultureInfo.CurrentCulture ) );
				}
				finally
				{
					listViewProperties.EndUpdate();
				}

                var props = await propertyBrowser.GetFilePropertyAsync(fileInfo).ConfigureAwait(true);

                if (treeView1.SelectedNode != e.Node)
                {
                    return;
                }

                if (props != null && props.Count > 0)
                {
                    listViewProperties.BeginUpdate();
                    try
                    {
                        // Performance optimization: Type-check and cast IReadOnlyDictionary to Dictionary to allow
                        // the foreach loop to use the struct-based enumerator, preventing interface boxing allocations.
                        if (props is Dictionary<string, IConvertible> propsDict)
                        {
                            foreach (var kvp in propsDict)
                            {
                                AddPropertyToListView(kvp.Key, kvp.Value?.ToString() ?? "");
                            }
                        }
                        else
                        {
                            foreach (var kvp in props)
                            {
                                AddPropertyToListView(kvp.Key, kvp.Value?.ToString() ?? "");
                            }
                        }
                    }
                    finally
                    {
                        listViewProperties.EndUpdate();
                    }
                }

                btnOpenFile.Enabled = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                if (treeView1.SelectedNode == e.Node)
                {
                    logger.Warning(ex, "Access denied reading properties for file: {Path}", info.Path);
                    AddPropertyToListView("Error", "Access Denied");
                }
            }
            catch (SecurityException ex)
            {
                if (treeView1.SelectedNode == e.Node)
                {
                    logger.Warning(ex, "Security exception reading properties for file: {Path}", info.Path);
                    AddPropertyToListView("Error", "Access Denied");
                }
            }
			catch (IOException ex)
			{
                if (treeView1.SelectedNode == e.Node)
                {
				    logger.Error(ex, "IO Error reading properties for file: {Path}", info.Path);
                    AddPropertyToListView("Error", "An IO error occurred.");
                }
            }
			catch (Exception ex)
			{
                if (treeView1.SelectedNode == e.Node)
                {
				    logger.Error(ex, "Error reading properties for file: {Path}", info.Path);
				    throw;
                }
            }
            finally
            {
                if (treeView1.SelectedNode == e.Node)
                {
                    Cursor = Cursors.Default;
                }
            }
        }

		private void AddPropertyToListView (string name, string value)
		{
			var item = new ListViewItem( name );
			item.SubItems.Add( value );
			listViewProperties.Items.Add( item );
		}

		private void BtnOpenFile_Click (object sender, EventArgs e)
		{
			if (treeView1.SelectedNode?.Tag is NodeInfo info && !info.IsDirectory)
			{
				if (IsPathWithinRoot( info.Path ))
				{
					MainWindow.OpenWithDefaultProgram( info.Path );
				}
				else
				{
					logger.Warning( "Path traversal blocked on execution: {Path} is outside root directory {RootDirectory}", info.Path, rootDirectory );
					MessageBox.Show( this, "Access denied: path is outside the root directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}
		}
	}
}
