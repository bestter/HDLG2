using System.Diagnostics;
using System.Reflection;
using FluentAssertions;
using HdlgFileProperty;
using HDLG_winforms;
using Moq;
using Serilog;
using System.Security;
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
        /// The real IO path is TreeView1_BeforeExpand (async), which surfaces "IO Error".
        /// </summary>
        [Fact]
        public void BrowserForm_Load_CatchesSecurityException_And_LogsWarning()
        {
            RunSta(() =>
            {
                AppUiBootstrap.Configure();
                string tempDir = Path.Combine(Path.GetTempPath(), "HDLG_UiTests_BrowserFormSec_" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(tempDir);
                try
                {
                    var mockLogger = new Mock<ILogger>(MockBehavior.Loose);
                    bool errorLogged = false;

                    mockLogger
                        .Setup(x => x.Warning(It.IsAny<Exception>(), It.IsAny<string>()))
                        .Callback((Exception ex, string messageTemplate) =>
                        {
                            if (ex is SecurityException
                                && messageTemplate.Contains("Security exception loading root directory", StringComparison.Ordinal))
                            {
                                errorLogged = true;
                            }
                        });

                    var propBrowser = new FilePropertyBrowser(mockLogger.Object, new ImagePropertyGetter());
                    using var form = new BrowserForm(tempDir, propBrowser, mockLogger.Object);

                    // Hook the TreeView's BeforeExpand event to throw a SecurityException.
                    // This is triggered synchronously during BrowserForm_Load when rootNode.Expand() is called.
                    var treeViewField = form.GetType().GetField("treeView1", BindingFlags.Instance | BindingFlags.NonPublic);
                    var treeView = (TreeView)treeViewField!.GetValue(form)!;

                    treeView.BeforeExpand += (s, e) => throw new SecurityException("Injected SecurityException");

                    var loadMethod = form.GetType().GetMethod("BrowserForm_Load", BindingFlags.Instance | BindingFlags.NonPublic);
                    loadMethod!.Invoke(form, new object[] { form, EventArgs.Empty });

                    errorLogged.Should().BeTrue("SecurityException during load should be logged as a warning");
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

                    // Native handles required so Expand raises BeforeExpand.
                    _ = form.Handle;
                    var treeViewField = form.GetType().GetField("treeView1", BindingFlags.Instance | BindingFlags.NonPublic);
                    var treeView = (TreeView)treeViewField!.GetValue(form)!;
                    _ = treeView.Handle;

                    // Remove the directory after construction so Expand's enumeration throws IOException.
                    System.IO.Directory.Delete(tempDir, true);

                    var loadMethod = form.GetType().GetMethod("BrowserForm_Load", BindingFlags.Instance | BindingFlags.NonPublic);
                    loadMethod!.Invoke(form, new object[] { form, EventArgs.Empty });

                    treeView.Nodes.Count.Should().Be(1);
                    var rootNode = treeView.Nodes[0];

                    // TreeView1_BeforeExpand is async void; pump the WinForms sync context until it completes.
                    long startTimestamp = Stopwatch.GetTimestamp();
                    while (Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds < 5000)
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
