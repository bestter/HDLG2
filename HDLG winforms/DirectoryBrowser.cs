using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLG_winforms
{    
    internal class DirectoryBrowser
    {
        private readonly string selectedDirectory;

        public DirectoryBrowser([NotNull] string selectedDirectory)
        {
            if (string.IsNullOrWhiteSpace(selectedDirectory))
            {
                throw new ArgumentException($"'{nameof(selectedDirectory)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(selectedDirectory));
            }

            this.selectedDirectory = selectedDirectory;
        }

        public string BrowseDirectoryAsHTML()
        {
            //DO
            DirectoryInfo directoryInfo = new(this.selectedDirectory);
            if (directoryInfo.Exists)
            {
                return directoryInfo.FullName;
            }
            else
            {
                throw new NotSupportedException($"Le répertoire {selectedDirectory} n'existe pas");
            }
        }
    }
}
