namespace HDLG.Tests
{
	/// <summary>
	/// Serializes WinForms UI tests to avoid GDI+ cross-thread conflicts.
	/// </summary>
	[CollectionDefinition(nameof(WinFormsUiTestCollection), DisableParallelization = true)]
	public sealed class WinFormsUiTestCollection
	{
	}
}