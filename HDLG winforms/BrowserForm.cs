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
using System.Security;

namespace HDLG_winforms
{
	public partial class BrowserForm : Form
	{
		private readonly string rootDirectory;
		private readonly string _resolvedRootDirectory;
		private readonly string _resolvedRootDirectoryWithSeparator;
		private readonly FilePropertyBrowser propertyBrowser;
		private readonly ILogger logger;
		private readonly Action<string>? _showError;

		public BrowserForm (string rootDirectory, FilePropertyBrowser propertyBrowser, ILogger logger, Action<string>? showError = null)
		{
			InitializeComponent( );
			_showError = showError;
			Icon = AppBranding.LoadApplicationIcon( );
			AppUiBootstrap.RemoveFormBranding( this );
			this.rootDirectory = rootDirectory;
			this.propertyBrowser = propertyBrowser;
			this.logger = logger;

			// Performance optimization: Cache resolved root directory strings to prevent redundant allocations and
			// I/O operations in hot paths like IsPathWithinRoot when expanding tree nodes.
			_resolvedRootDirectory = Path.GetFullPath( rootDirectory );
			_resolvedRootDirectoryWithSeparator = _resolvedRootDirectory.EndsWith( Path.DirectorySeparatorChar )
				? _resolvedRootDirectory
				: _resolvedRootDirectory + Path.DirectorySeparatorChar;
		}

		private void BrowserForm_Load (object sender, EventArgs e)
		{
			try
			{
				var rootNode = new TreeNode( rootDirectory );
				rootNode.Tag = new NodeInfo { IsDirectory = true, Path = rootDirectory };
				rootNode.Nodes.Add( new TreeNode( "Loading..." ) );
				treeView1.Nodes.Add( rootNode );
				rootNode.Expand( );
			}
			catch (UnauthorizedAccessException ex)
			{
				logger.Warning( ex, "Access denied loading root directory in BrowserForm" );
				MessageBox.Show( this, "Error: Access Denied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			catch (SecurityException ex)
			{
				logger.Warning( ex, "Security exception loading root directory in BrowserForm" );
				MessageBox.Show( this, "Error: Access Denied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			catch (IOException ex)
			{
				logger.Error( ex, "IO Error loading root directory in BrowserForm" );
				if (_showError != null) _showError( "An IO error occurred while loading the directory." );
				else MessageBox.Show( this, "An IO error occurred while loading the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				logger.Error( ex, "Error loading root directory in BrowserForm" );
				MessageBox.Show( this, "An unexpected error occurred while loading the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
#pragma warning restore CA1031 // Do not catch general exception types
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
					// Use GetFileSystemInfos() instead of EnumerateFileSystemInfos() to avoid intermediate List<T> allocations
					// Because we need the data on the UI thread without allocations, we fetch the array here.
					// Note: While GetFileSystemInfos creates an array, it is more efficient than populating two List<T> instances
					// due to avoiding List capacity resizing and allowing exact allocation of the TreeNode array.
					var fsInfos = await Task.Run( () =>
					{
						var dirInfo = new DirectoryInfo( info.Path );
						return dirInfo.GetFileSystemInfos( );
					} ).ConfigureAwait( true );

					// Safe WinForms practice: construct TreeNodes on the UI thread after I/O is complete
					// Performance optimization: Use a single List to gather all nodes with capacity sized to exact need.
					// This prevents List resizing allocations and allows a single array allocation for AddRange.
					// We iterate twice to ensure directories appear before files, avoiding expensive List.Insert operations.
					var allNodes = new List<TreeNode>( fsInfos.Length );

					for (int i = 0; i < fsInfos.Length; i++)
					{
						if (fsInfos [i] is DirectoryInfo dir)
						{
							if ((dir.Attributes & FileAttributes.ReparsePoint) != 0) continue;

							var node = new TreeNode( dir.Name );
							node.Tag = new NodeInfo { IsDirectory = true, Path = dir.FullName };
							node.Nodes.Add( new TreeNode( "Loading..." ) );
							allNodes.Add( node );
						}
					}

					for (int i = 0; i < fsInfos.Length; i++)
					{
						if (fsInfos [i] is FileInfo file)
						{
							var node = new TreeNode( file.Name );
							node.Tag = new NodeInfo { IsDirectory = false, Path = file.FullName };
							allNodes.Add( node );
						}
					}

					e.Node.TreeView?.BeginUpdate( );
					e.Node.Nodes.AddRange( allNodes.ToArray( ) );
					e.Node.TreeView?.EndUpdate( );
				}
				catch (UnauthorizedAccessException ex)
				{
					logger.Warning( ex, "Access denied to directory: {Path}", info.Path );
					e.Node.Nodes.Add( new TreeNode( "Access Denied" ) );
				}
				catch (SecurityException ex)
				{
					logger.Warning( ex, "Security exception accessing directory: {Path}", info.Path );
					e.Node.Nodes.Add( new TreeNode( "Access Denied" ) );
				}
				catch (IOException ex)
				{
					logger.Error( ex, "IO Error loading directory: {Path}", info.Path );
					e.Node.Nodes.Add( new TreeNode( "IO Error" ) );
				}
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception ex)
				{
					logger.Error( ex, "Error loading directory: {Path}", info.Path );
					e.Node.Nodes.Add( new TreeNode( "Error" ) );
				}
#pragma warning restore CA1031 // Do not catch general exception types
				finally
				{
					Cursor = Cursors.Default;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Localization", "CA1303:Do not pass literals as localized parameters" )]
		private async void TreeView1_AfterSelect (object sender, TreeViewEventArgs e)
		{
			Cursor = Cursors.Default;
			listViewProperties.Items.Clear( );
			btnOpenFile.Enabled = false;

			if (e.Node == null || e.Node.Tag is not NodeInfo info)
			{
				lblSelectedFileName.Text = "Select a file to view properties";
				return;
			}

			if (info.IsDirectory)
			{
				var dirInfo = new DirectoryInfo( info.Path );
				lblSelectedFileName.Text = dirInfo.Name;
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

				lblSelectedFileName.Text = fileInfo.Name;

				listViewProperties.BeginUpdate( );
				try
				{
					var items = new ListViewItem [7];
					items [0] = CreateListViewItem( "Name", fileInfo.Name );
					items [1] = CreateListViewItem( "Path", fileInfo.FullName );
					items [2] = CreateListViewItem( "Extension", fileInfo.Extension );
					items [3] = CreateListViewItem( "Size (bytes)", fileInfo.Length.ToString( CultureInfo.CurrentCulture ) );
					items [4] = CreateListViewItem( "Creation Time", fileInfo.CreationTime.ToString( "g", CultureInfo.CurrentCulture ) );
					items [5] = CreateListViewItem( "Last Access Time", fileInfo.LastAccessTime.ToString( "g", CultureInfo.CurrentCulture ) );
					items [6] = CreateListViewItem( "Last Write Time", fileInfo.LastWriteTime.ToString( "g", CultureInfo.CurrentCulture ) );
					listViewProperties.Items.AddRange( items );
				}
				finally
				{
					listViewProperties.EndUpdate( );
				}

				var props = await propertyBrowser.GetFilePropertyAsync( fileInfo ).ConfigureAwait( true );

				if (treeView1.SelectedNode != e.Node)
				{
					return;
				}

				if (props != null && props.Count > 0)
				{
					listViewProperties.BeginUpdate( );
					try
					{
						// Performance optimization: Type-check and cast IReadOnlyDictionary to Dictionary to allow
						// the foreach loop to use the struct-based enumerator, preventing interface boxing allocations.
						if (props is Dictionary<string, IConvertible> propsDict)
						{
							var items = new ListViewItem [propsDict.Count];
							int i = 0;
							foreach (var kvp in propsDict)
							{
								items [i++] = CreateListViewItem( kvp.Key, kvp.Value?.ToString( ) ?? "" );
							}
							listViewProperties.Items.AddRange( items );
						}
						else
						{
							var items = new ListViewItem [props.Count];
							int i = 0;
							foreach (var kvp in props)
							{
								items [i++] = CreateListViewItem( kvp.Key, kvp.Value?.ToString( ) ?? "" );
							}
							listViewProperties.Items.AddRange( items );
						}
					}
					finally
					{
						listViewProperties.EndUpdate( );
					}
				}

				btnOpenFile.Enabled = true;
			}
			catch (UnauthorizedAccessException ex)
			{
				if (treeView1.SelectedNode == e.Node)
				{
					logger.Warning( ex, "Access denied reading properties for file: {Path}", info.Path );
					AddPropertyToListView( "Error", "Access Denied" );
				}
			}
			catch (SecurityException ex)
			{
				if (treeView1.SelectedNode == e.Node)
				{
					logger.Warning( ex, "Security exception reading properties for file: {Path}", info.Path );
					AddPropertyToListView( "Error", "Access Denied" );
				}
			}
			catch (IOException ex)
			{
				if (treeView1.SelectedNode == e.Node)
				{
					logger.Error( ex, "IO Error reading properties for file: {Path}", info.Path );
					AddPropertyToListView( "Error", "An IO error occurred." );
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception ex)
			{
				if (treeView1.SelectedNode == e.Node)
				{
					logger.Error( ex, "Error reading properties for file: {Path}", info.Path );
					AddPropertyToListView( "Error", "An unexpected error occurred." );
				}
			}
#pragma warning restore CA1031 // Do not catch general exception types
			finally
			{
				if (treeView1.SelectedNode == e.Node)
				{
					Cursor = Cursors.Default;
				}
			}
		}

		private static ListViewItem CreateListViewItem (string name, string value)
		{
			var item = new ListViewItem( name );
			item.SubItems.Add( value );
			return item;
		}

		private void AddPropertyToListView (string name, string value)
		{
			listViewProperties.Items.Add( CreateListViewItem( name, value ) );
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
