using System;
using System.IO;

namespace HDLG.Tests
{
    internal static class PdfPropertyGetterTestSetup
    {
        private const string TestPdfBase64 = "JVBERi0xLjcKJanNxNIKMyAwIG9iago8PC9MZW5ndGggNiAvTGVuZ3RoMSAwIC9GaWx0ZXIgWyAvRmxhdGVEZWNvZGUgXSA+PgpzdHJlYW0KeAEAAAABCmVuZHN0cmVhbQplbmRvYmoKMiAwIG9iago8PC9UeXBlIC9QYWdlIC9QYXJlbnQgMSAwIFIgL1Byb2NTZXQgWyAvUERGIC9UZXh0IC9JbWFnZUIgL0ltYWdlQyAvSW1hZ2VJIF0gL01lZGlhQm94IFsgMCAwIDIxMCAyOTcgXSAvQ29udGVudHMgMyAwIFIgPj4KZW5kb2JqCjEgMCBvYmoKPDwvVHlwZSAvUGFnZXMgL0tpZHMgWyAyIDAgUiBdIC9Db3VudCAxID4+CmVuZG9iago0IDAgb2JqCjw8L1R5cGUgL0NhdGFsb2cgL1BhZ2VzIDEgMCBSID4+CmVuZG9iago1IDAgb2JqCjw8L1RpdGxlIChUZXN0IFRpdGxlKSAvUHJvZHVjZXIgKFBkZlBpZykgPj4KZW5kb2JqCgp4cmVmCjAgNiAKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMjQxIDAwMDAwIG4gCjAwMDAwMDAxMDUgMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMjk5IDAwMDAwIG4gCjAwMDAwMDAzNDcgMDAwMDAgbiAKdHJhaWxlcgo8PC9TaXplIDYgL1Jvb3QgNCAwIFIgL0lEIFsgPDhGN0M0OTg5MkEzMzRDOTNCRkJDQzQ4QUZBMzc4RUM0PjxCQUZCQkExM0ExM0M0REZBODI0OTk4NzRERjM1MkI3Qz5dIC9JbmZvIDUgMCBSID4+CnN0YXJ0eHJlZgo0MDcKJSVFT0Y=";
        private const string TestPdfEmptyBase64 = "JVBERi0xLjcKJanNxNIKMyAwIG9iago8PC9MZW5ndGggNiAvTGVuZ3RoMSAwIC9GaWx0ZXIgWyAvRmxhdGVEZWNvZGUgXSA+PgpzdHJlYW0KeAEAAAABCmVuZHN0cmVhbQplbmRvYmoKMiAwIG9iago8PC9UeXBlIC9QYWdlIC9QYXJlbnQgMSAwIFIgL1Byb2NTZXQgWyAvUERGIC9UZXh0IC9JbWFnZUIgL0ltYWdlQyAvSW1hZ2VJIF0gL01lZGlhQm94IFsgMCAwIDIxMCAyOTcgXSAvQ29udGVudHMgMyAwIFIgPj4KZW5kb2JqCjEgMCBvYmoKPDwvVHlwZSAvUGFnZXMgL0tpZHMgWyAyIDAgUiBdIC9Db3VudCAxID4+CmVuZG9iago0IDAgb2JqCjw8L1R5cGUgL0NhdGFsb2cgL1BhZ2VzIDEgMCBSID4+CmVuZG9iago1IDAgb2JqCjw8L1Byb2R1Y2VyIChQZGZQaWcpID4+CmVuZG9iagoKeHJlZgowIDYgCjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDI0MSAwMDAwMCBuIAowMDAwMDAwMTA1IDAwMDAwIG4gCjAwMDAwMDAwMTUgMDAwMDAgbiAKMDAwMDAwMDI5OSAwMDAwMCBuIAowMDAwMDAwMzQ3IDAwMDAwIG4gCnRyYWlsZXIKPDwvU2l6ZSA2IC9Sb290IDQgMCBSIC9JRCBbIDw5REVCOEI2NERDRDc0MTQzOTgxNDA5MzkxMkYxREY4OD48MkI3OTRDNUY4QzA5NEMxQUExQzZEN0FGMUREMDI0MEU+XSAvSW5mbyA1IDAgUiA+PgpzdGFydHhyZWYKMzg3CiUlRU9G";
        private const string TestPdfInvalidBase64 = "aW52YWxpZCBkYXRh";
        private const string TestPdfEncryptedBase64 = "JVBERi0xLjcKJeLjz9MKMiAwIG9iago8PC9FbmNyeXB0IDMgMCBSIC9SZWdpc3RyeU5hbWUgKE9SQUNMRSkgL0lEIFs8MjRBNzg0Q0RCNEIxQjg0MkIzMjM2MzJDOTA5RThBQUE+IDwyNEE3ODRDREI0QjFCODQyQjMyMzYzMkM5MDlFOEFBQT5dIC9MZW5ndGggMTI4IC9PICI1Pz5COk8iIC9VPj4KZW5kb2JqCjMgMCBvYmoKPDwvRmlsdGVyIC9TdGFuZGFyZCAvViAxIC9SIDIgL08gKDwzRDRFNzc2MUEyOUQzMjUyMzgyOEM5QTg4RTUzMDkzMjQxQUU2RTcwMDcxOUI3OUMzQjBEOTBFQTkzRTc2RDQ3PikgL1UgKDxCREI3MzAxNEI2NzdBODc4QTk4MkRBNjRDNTM2MTg5NTAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwPikgL1AgLTYwIC9MZW5ndGggMTI4Pj4KZW5kb2JqCjEgMCBvYmoKPDwvVHlwZSAvQ2F0YWxvZyAvUGFnZXMgNCAwIFIgPj4KZW5kb2JqCjQgMCBvYmoKPDwvVHlwZSAvUGFnZXMgL0tpZHMgWzUgMCBSICBdIC9Db3VudCAxPj4KZW5kb2JqCjUgMCBvYmoKPDwvVHlwZSAvUGFnZSAvUGFyZW50IDQgMCBSIC9NZWRpYUJveCBbMCAwIDU5NSA4NDJdIC9Db250ZW50cyA2IDAgUj4+CmVuZG9iago2IDAgb2JqCjw8L0xlbmd0aCA3IDAwIFI+PgpzdHJlYW0KOjI4NTY1Mzc0RTIyQTU2Njk2RDY0N0IzMTA0QjA2OTExNkI4ODlBQUFBOUVDNzM3MjRDRDBEOEM4OUJDNzE4MDY0MUFCRENDNTM4CjIwRTk2NzNFRTUyOTFENDVCMTc5NEYyCgplbmRzdHJlYW0KZW5kb2JqCjcgMCBvYmoKOTIKZW5kb2JqCnhyZWYKMCA4CjAwMDAwMDAwMDAgNjU1MzUgZgoyMDAwMDAwMDAwIDAwMDAwIG4KMDAwMDAwMDAxNyAwMDAwMCBuCjAwMDAwMDAxNzQgMDAwMDAgbgoyMDAwMDAwMzI3IDAwMDAwIG4KMDAwMDAwMDM3NSAwMDAwMCBuCjAwMDAwMDA0MzIgMDAwMDAgbgoyMDAwMDAwNTM3IDAwMDAwIG4KdHJhaWxlcgo8PC9TaXplIDggL1Jvb3QgMSAwIFIgL0luZm8gMiAwIFIgL0lEIFs8MjRBNzg0Q0RCNEIxQjg0MkIzMjM2MzJDOTA5RThBQUE+IDwyNEE3ODRDREI0QjFCODQyQjMyMzYzMkM5MDlFOEFBQT5dIC9FbmNyeXB0IDMgMCBSPj4Kc3RhcnR4cmVmCjU2MQolJUVPRg==";

        public static void CreatePdfDocs()
        {
            if (!File.Exists("test_pdf.pdf"))
                File.WriteAllBytes("test_pdf.pdf", Convert.FromBase64String(TestPdfBase64));
            if (!File.Exists("test_pdf_empty.pdf"))
                File.WriteAllBytes("test_pdf_empty.pdf", Convert.FromBase64String(TestPdfEmptyBase64));
            if (!File.Exists("test_pdf_invalid.pdf"))
                File.WriteAllBytes("test_pdf_invalid.pdf", Convert.FromBase64String(TestPdfInvalidBase64));
            if (!File.Exists("test_pdf_encrypted.pdf"))
                File.WriteAllBytes("test_pdf_encrypted.pdf", Convert.FromBase64String(TestPdfEncryptedBase64));
        }

        public static void Cleanup()
        {
            if (File.Exists("test_pdf.pdf"))
                File.Delete("test_pdf.pdf");
            if (File.Exists("test_pdf_empty.pdf"))
                File.Delete("test_pdf_empty.pdf");
            if (File.Exists("test_pdf_invalid.pdf"))
                File.Delete("test_pdf_invalid.pdf");
            if (File.Exists("test_pdf_encrypted.pdf"))
                File.Delete("test_pdf_encrypted.pdf");
        }
    }
}
