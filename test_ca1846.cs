using System;

class Program {
    static void Main() {
        string path = "some/path/here";
        int startIndex = 0;
        int i = 4;

        string s1 = Uri.EscapeDataString(path.AsSpan(startIndex, i - startIndex).ToString());
    }
}
