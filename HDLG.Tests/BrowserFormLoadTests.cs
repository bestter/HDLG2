using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using HdlgFileProperty;
using HDLG_winforms;
using Krypton.Toolkit;
using Moq;
using Serilog;
using System.Windows.Forms;

namespace HDLG.Tests
{
	[Collection(nameof(WinFormsUiTestCollection))]
	public class BrowserFormLoadTests
	{
		private static void RunSta(Action action)
		{
			Exception? caught = null;
			var thread = new Thread(() =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					caught = ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			if (caught != null)
			{
				throw caught;
			}
		}

		/// <summary>
		/// Verifies IOException handling on directory expand.
		/// Note: KryptonTreeView swallows exceptions thrown from BeforeExpand event handlers, so
		/// BrowserForm_Load's try/catch around rootNode.Expand() is not reachable for event-sourced
		/// failures. The real IO path is TreeView1_BeforeExpand (async), which surfaces "IO Error".
		/// </summary>
		[Fact]
		public void BrowserForm_BeforeExpand_CatchesIOException_And_ShowsIOErrorNode()
		{
			RunSta(() =>
			{
				AppUiBootstrap.Configure();
				string tempDir = Path.Combine(Path.GetTempPath(), "HDLG_UiTests_BrowserFormIO_" + Guid.NewGuid());
				System.IO.Directory.CreateDirectory(tempDir);
				try
				{
					var mockLogger = new Mock<ILogger>(MockBehavior.Loose);
					bool errorLogged = false;
					// Serilog overload used: Error(Exception, string messageTemplate, T0 propertyValue)
					mockLogger
						.Setup(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()))
						.Callback((Exception ex, string messageTemplate, string _) =>
						{
							if (ex is IOException
								&& messageTemplate.Contains("IO Error loading directory", StringComparison.Ordinal))
							{
								errorLogged = true;
							}
						});

					var propBrowser = new FilePropertyBrowser(mockLogger.Object, new ImagePropertyGetter());
					using var form = new BrowserForm(tempDir, propBrowser, mockLogger.Object);

					// Native handles required so Expand raises BeforeExpand through Krypton's internal TreeView.
					_ = form.Handle;
					var treeViewField = form.GetType().GetField("treeView1", BindingFlags.Instance | BindingFlags.NonPublic);
					var treeView = (KryptonTreeView)treeViewField!.GetValue(form)!;
					_ = treeView.Handle;
					_ = treeView.TreeView.Handle;

					// Remove the directory after construction so Expand's enumeration throws IOException.
					System.IO.Directory.Delete(tempDir, true);

					var loadMethod = form.GetType().GetMethod("BrowserForm_Load", BindingFlags.Instance | BindingFlags.NonPublic);
					loadMethod!.Invoke(form, new object[] { form, EventArgs.Empty });

					treeView.Nodes.Count.Should().Be(1);
					var rootNode = treeView.Nodes[0];

					// TreeView1_BeforeExpand is async void; pump the WinForms sync context until it completes.
					var deadline = Stopwatch.StartNew();
					while (deadline.Elapsed < TimeSpan.FromSeconds(5))
					{
						Application.DoEvents();
						if (rootNode.Nodes.Count > 0 && rootNode.Nodes[0].Text != "Loading...")
						{
							break;
						}
						Thread.Sleep(20);
					}

					// Exactly one error node: merge artifacts previously duplicated error UI entries.
					rootNode.Nodes.Count.Should().Be(1);
					rootNode.Nodes[0].Text.Should().Be("IO Error");
					errorLogged.Should().BeTrue("IOException during expand should be logged");
				}
				finally
				{
					if (System.IO.Directory.Exists(tempDir))
					{
						System.IO.Directory.Delete(tempDir, true);
					}
				}
			});
		}
	}
}
