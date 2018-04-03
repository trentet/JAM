using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace CryptoGateway.FileSystem.VShell.Interfaces
{
    public interface IMiniMimeMessageNode
    {
        DateTime? ReceivedDateTime { get; set; }
        string OriginalMsgID { get; set; }
    }

    public class ThreadedMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ThreadedMessage(IMiniMimeMessageNode data)
        {
            MsgNode = data;
        }

        public IMiniMimeMessageNode MsgNode
        {
            get;
            set;
        }

        private bool _isMsgNodeExpaned = true;

        public bool IsMsgNodeExpaned
        {
            get { return _isMsgNodeExpaned; }
            set
            {
                if (_isMsgNodeExpaned != value)
                {
                    _isMsgNodeExpaned = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsMsgNodeExpaned"));
                }
            }
        }

        private bool _isMsgNodeSelected = false;

        public bool IsMsgNodeSelected
        {
            get { return _isMsgNodeSelected; }
            set
            {
                if (_isMsgNodeSelected != value)
                {
                    _isMsgNodeSelected = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsMsgNodeSelected"));
                }
            }
        }

        private bool _showTimeSeqNumber = false;

        public bool ShowTimeSeqNumber
        {
            get { return _showTimeSeqNumber; }
            set
            {
                if (_showTimeSeqNumber != value)
                {
                    _showTimeSeqNumber = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ShowTimeSeqNumber"));
                }
            }
        }

        private int _timeSeqNumber = 0;

        public int TimeSeqNumber
        {
            get { return _timeSeqNumber; }
            set
            {
                if (_timeSeqNumber != value)
                {
                    _timeSeqNumber = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TimeSeqNumber"));
                }
            }
        }

        public string MsgDataPath = null;

        private WeakReference _parentMsg = null;

        public ThreadedMessage ParentMsg
        {
            get { return _parentMsg == null ? null : _parentMsg.Target as ThreadedMessage; }
            set { _parentMsg = new WeakReference(value); }
        }

        private List<ThreadedMessage> _replyMsgs = null;

        public IOrderedEnumerable<ThreadedMessage> ReplyMsgs
        {
            get
            {
                if (_replyMsgs == null)
                    _replyMsgs = new List<ThreadedMessage>();
                return from d in _replyMsgs orderby d.MsgNode.ReceivedDateTime ascending select d;
            }
        }

        public void AddReplyMsg(ThreadedMessage msg)
        {
            if (!(from d in ReplyMsgs where msg.MsgNode.OriginalMsgID == d.MsgNode.OriginalMsgID select d).Any())
            {
                msg.ParentMsg = this;
                _replyMsgs.Add(msg);
            }
        }

        public void UpdateBranchSeqNumber(bool DisplayNumber, out ThreadedMessage[] OrderedList, bool Refresh = false)
        {
            if (_orderedBranchList == null || Refresh)
            {
                List<ThreadedMessage> l = new List<ThreadedMessage>();
                List<ThreadedMessage> pl = new List<ThreadedMessage>();
                List<ThreadedMessage> cl = new List<ThreadedMessage>();
                pl.Add(this);
                l.Add(this);
                while (true)
                {
                    foreach (ThreadedMessage msg in pl)
                    {
                        foreach (ThreadedMessage cmsg in msg.ReplyMsgs)
                            cl.Add(cmsg);
                    }
                    if (cl.Count == 0)
                        break;
                    l.AddRange(cl);
                    pl = cl;
                    cl = new List<ThreadedMessage>();
                }
                OrderedList = (from d in l orderby d.MsgNode.ReceivedDateTime.Value where d.MsgNode.ReceivedDateTime.HasValue select d).ToArray();
                _orderedBranchList = OrderedList;
                for (int i = 0; i < OrderedList.Length; i++)
                {
                    OrderedList[i].TimeSeqNumber = i + 1;
                    OrderedList[i].ShowTimeSeqNumber = DisplayNumber;
                }
            }
            else
            {
                OrderedList = _orderedBranchList;
                for (int i = 0; i < OrderedList.Length; i++)
                    OrderedList[i].ShowTimeSeqNumber = DisplayNumber;
            }
        }

        private ThreadedMessage[] _orderedBranchList = null;

    }
}
