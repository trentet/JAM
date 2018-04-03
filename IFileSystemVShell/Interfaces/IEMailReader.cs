using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace CryptoGateway.FileSystem.VShell.Interfaces
{
    public delegate bool QueryMessageThreadHanlder(string MsgID, out ThreadedMessage StartMsg);

    public interface IEMailReader : IMessageReader
    {
        QueryMessageThreadHanlder QueryThread
        {
            get;
            set;
        }
    }
}
