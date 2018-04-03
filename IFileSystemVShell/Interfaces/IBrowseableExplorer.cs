using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoGateway.FileSystem.VShell.Interfaces
{
    public interface IBrowseableExplorer
    {
        Uri SourceUri { get; set; }
    }

    public interface IMicrosoftOfficeView : IBrowseableExplorer
    {
    }
}
