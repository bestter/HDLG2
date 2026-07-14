using FluentAssertions;
using HdlgFileProperty;
using HDLG_winforms;
using Krypton.Toolkit;
using Moq;
using Serilog;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System;

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

        [Fact]
        public void BrowserForm_Load_CatchesIOException_And_ShowsMessage()
        {
            RunSta(() =>
            {
                AppUiBootstrap.Configure();
                string tempDir = Path.Combine(Path.GetTempPath(), "HDLG_UiTests_BrowserFormThrows_" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(tempDir);
                try
                {
                    var mockLogger = new Mock<ILogger>();
                    bool errorLogged = false;
                    mockLogger.Setup(x => x.Error(It.IsAny<IOException>(), "IO Error loading root directory in BrowserForm"))
                              .Callback(() => errorLogged = true);

                    var propBrowser = new FilePropertyBrowser(mockLogger.Object, new ImagePropertyGetter());

                    bool messageShown = false;
                    string shownMessage = string.Empty;
                    Action<string> showError = (msg) =>
                    {
                        messageShown = true;
                        shownMessage = msg;
                    };

                    using var form = new BrowserForm(tempDir, propBrowser, mockLogger.Object, showError);

                    var treeViewField = form.GetType().GetField("treeView1", BindingFlags.Instance | BindingFlags.NonPublic);
                    var treeView = (KryptonTreeView)treeViewField!.GetValue(form)!;

                    treeView.BeforeExpand += (s, e) =>
                    {
                        throw new IOException("Simulated IO failure");
                    };

                    var loadMethod = form.GetType().GetMethod("BrowserForm_Load", BindingFlags.Instance | BindingFlags.NonPublic);

                    loadMethod!.Invoke(form, new object[] { form, EventArgs.Empty });

                    errorLogged.Should().BeTrue();
                    messageShown.Should().BeTrue();
                    shownMessage.Should().Be("An IO error occurred while loading the directory.");
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
