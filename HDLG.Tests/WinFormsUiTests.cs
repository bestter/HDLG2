using FluentAssertions;
using HdlgFileProperty;
using HDLG_winforms;
using Krypton.Toolkit;
using Serilog;
using System.Reflection;
using System.Windows.Forms;

namespace HDLG.Tests
{
	[Collection(nameof(WinFormsUiTestCollection))]
	public class WinFormsUiTests
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

		[Fact]
		public void MainWindow_InitializesWithKryptonDashboardControls()
		{
			RunSta(() =>
			{
				AppUiBootstrap.Configure();
				using var window = CreateMainWindow();

				window.Should().BeAssignableTo<KryptonForm>();
				FindControl<KryptonHeaderGroup>(window, "headerGroupDirectory").Should().NotBeNull();
				FindControl<KryptonHeaderGroup>(window, "headerGroupExport").Should().NotBeNull();
				FindControl<KryptonButton>(window, "btnChooseFolder").Should().NotBeNull();
				FindControl<KryptonButton>(window, "btnStartXml").Should().NotBeNull();
				FindControl<KryptonButton>(window, "btnStartHtml").Should().NotBeNull();
				FindControl<KryptonButton>(window, "btnStartUi").Should().NotBeNull();
				FindControl<KryptonButton>(window, "btnAbout").Should().NotBeNull();
				FindControl<KryptonProgressBar>(window, "progressBar1").Should().NotBeNull();
				FindControl<KryptonStatusStrip>(window, "statusStrip1").Should().NotBeNull();
			});
		}

		[Fact]
		public void BrowserForm_InitializesWithKryptonExplorerControls()
		{
			RunSta(() =>
			{
				AppUiBootstrap.Configure();
				string tempDir = Path.Combine(Path.GetTempPath(), "HDLG_UiTests_" + Guid.NewGuid());
				System.IO.Directory.CreateDirectory(tempDir);
				try
				{
					using var logger = new LoggerConfiguration().CreateLogger();
					using var form = new BrowserForm(tempDir, CreatePropertyBrowser(logger), logger);

					form.Should().BeAssignableTo<KryptonForm>();
					FindControl<KryptonTreeView>(form, "treeView1").Should().NotBeNull();
					FindControl<KryptonListView>(form, "listViewProperties").Should().NotBeNull();
					FindControl<KryptonButton>(form, "btnOpenFile").Should().NotBeNull();
					FindControl<KryptonHeaderGroup>(form, "headerGroupExplorer").Should().NotBeNull();
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

		[Fact]
		public void Credit_InitializesWithAboutContent()
		{
			RunSta(() =>
			{
				AppUiBootstrap.Configure();
				using var form = new Credit();

				form.Should().BeAssignableTo<KryptonForm>();
				FindControl<KryptonLabel>(form, "lblTitle").Should().NotBeNull();
				FindControl<KryptonLinkLabel>(form, "labelGPL").Should().NotBeNull();
				FindControl<PictureBox>(form, "pictureBox1").Should().NotBeNull();
				form.Text.Should().Be("About");
			});
		}

		private static MainWindow CreateMainWindow()
		{
			using var logger = new LoggerConfiguration().CreateLogger();
			return new MainWindow(
				new ImagePropertyGetter(),
				new WordPropertyGetter(),
				new ExcelPropertyGetter(),
				new PdfPropertyGetter(),
				new Mp3PropertyGetter(),
				logger);
		}

		private static FilePropertyBrowser CreatePropertyBrowser(ILogger logger)
		{
			return new FilePropertyBrowser(
				logger,
				new ImagePropertyGetter(),
				new WordPropertyGetter(),
				new ExcelPropertyGetter(),
				new PdfPropertyGetter(),
				new Mp3PropertyGetter());
		}

		private static T? FindControl<T>(Control root, string name) where T : Control
		{
			FieldInfo? field = root.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (field?.GetValue(root) is T directMatch)
			{
				return directMatch;
			}

			foreach (Control child in root.Controls)
			{
				if (string.Equals(child.Name, name, StringComparison.Ordinal) && child is T match)
				{
					return match;
				}

				T? nested = FindControl<T>(child, name);
				if (nested != null)
				{
					return nested;
				}
			}

			return null;
		}
	}
}