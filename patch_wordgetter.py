with open("HDLG.Tests/PropertyGetterTests.cs", "r") as f:
    lines = f.readlines()

for i, line in enumerate(lines):
    if "WordPropertyGetterTestSetup.EnsureTestFilesExist();" in line:
        pass
