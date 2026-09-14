using System;
using System.Text;

class Program {
    static void Main() {
        string path = "hello/world";
        StringBuilder sb = new StringBuilder();
        int startIndex = 0;
        int i = 5;

        // This will compile if there is a span overload
        sb.Append(Uri.EscapeDataString(path.AsSpan(startIndex, i - startIndex)));
    }
}
