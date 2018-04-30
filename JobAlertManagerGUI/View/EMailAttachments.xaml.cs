using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CryptoGateway.FileSystem.VShell;
using CryptoGateway.FileSystem.VShell.Interfaces;
using JobAlertManagerGUI.Model;
using LumiSoft.Net.Mime;
using Microsoft.Win32;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    ///     Interaction logic for EMailAttachments.xaml
    /// </summary>
    public partial class EMailAttachments : UserControl
    {
        private bool IsLoadCompleted;

        public EMailAttachments()
        {
            InitializeComponent();
            GBB.Header = Properties.Resources.AttachmentsWordQ;
            GBC.Header = Properties.Resources.PreviewWord;
            (LstAttach.View as GridView).Columns[0].Header = Properties.Resources.NameWord;
            var dir = Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length).Replace('/', '\\');
            dir = dir.Substring(0, dir.LastIndexOf('\\'));
            var categ = new DirectoryCatalog(dir);
            var cname = AttributedModelServices.GetContractName(typeof(IImageViewer));
            Expression<Func<ExportDefinition, bool>> exp = a => a.ContractName == cname;
            var id = new ImportDefinition(exp, cname, ImportCardinality.ExactlyOne, true, true);
            var l = categ.GetExports(id).ToList();
            if (l.Count == 1)
            {
                var cc = new CompositionContainer(categ);
                cc.ComposeParts(this);
                cviewer.Content = ImageViewer;
            }
        }

        [Import(typeof(IImageViewer))] private IImageViewer ImageViewer { get; set; }

        private string LastDepositFolder
        {
            get => RootModel.LastDepositFolder;
            set => RootModel.LastDepositFolder = value;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoadCompleted) IsLoadCompleted = true;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Mime)
            {
                var m = e.NewValue as Mime;
                if (m != null)
                    LstAttach.ItemsSource = from d in m.Attachments select new MimeWrapper(d);
                else
                    LstAttach.ItemsSource = null;
            }
        }

        private void OnSelectAttachment(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var ma = e.AddedItems[0] as MimeWrapper;
                if (!HttpContentServer.IsServing)
                {
                    Action act = () => { HttpContentServer.Start(); };
                    act.BeginInvoke(ar => { ShowContent(ma, true); }, null);
                }
                else
                {
                    ShowContent(ma, false);
                }
            }
        }

        private void ShowContent(MimeWrapper ma, bool async = false)
        {
            string mimetype = null;
            var itemname = GetItemName(ma.Entity);
            if (!string.IsNullOrWhiteSpace(ma.Entity.ContentTypeString))
            {
                mimetype = ma.Entity.ContentTypeString;
                var ipos = mimetype.IndexOf(';');
                if (ipos != -1) mimetype = mimetype.Substring(0, ipos);
            }
            else
            {
                var ext = itemname;
                if (ext.LastIndexOf('.') != -1)
                {
                    ext = ext.Substring(ext.LastIndexOf('.'));
                    var rk = Registry.ClassesRoot.OpenSubKey(ext.ToLower());
                    if (rk != null)
                    {
                        if (rk.GetValue("Content Type") != null)
                            mimetype = rk.GetValue("Content Type") as string;
                        rk.Close();
                    }
                }
            }

            if (mimetype != null && mimetype.StartsWith("image/") && ImageViewer != null)
            {
                var cid = string.IsNullOrWhiteSpace(ma.Entity.ContentID) ? itemname : ma.Entity.ContentID;
                HttpContentServer.MediaBuffer = ma.Entity.Data;
                HttpContentServer.ContentTypeString = mimetype;
                HttpContentServer.State = HttpContentServerState.Data;
                if (async)
                    Dispatcher.Invoke(
                        (Action) (() => { ImageViewer.SourceUri = new Uri(HttpContentServer.Prefix + "?id=" + cid); }),
                        null);
                else
                    ImageViewer.SourceUri = new Uri(HttpContentServer.Prefix + "?id=" + cid);
            }
        }

        private string GetItemName(MimeEntity entity)
        {
            return entity.ContentDisposition_FileName != null
                ? entity.ContentDisposition_FileName
                : (entity.ContentDescription != null
                    ? entity.ContentDescription
                    : (entity.ContentType_Name != null ? entity.ContentType_Name : "???"));
        }

        private void OnSaveAttachment(object sender, RoutedEventArgs e)
        {
            var btn = e.OriginalSource as Button;
            if (!(btn.DataContext is MimeWrapper))
                return;
            var de = (btn.DataContext as MimeWrapper).Entity;
            if (de != null)
            {
                var ex = new TransCompSelectFolderEventArgs(ComponentEvents.TransCompSelectFolderEvent);
                ex.EventTitle = Properties.Resources.AttachmentSaveDirSelWord;
                ex.InitialFolderPath = LastDepositFolder;
                RaiseEvent(ex);
                if (ex.Handled && ex.IsSelectionMade && !string.IsNullOrEmpty(ex.SelectedFolderPath))
                    if (Directory.Exists(ex.SelectedFolderPath))
                    {
                        LastDepositFolder = ex.SelectedFolderPath;
                        SaveFile(ex.SelectedFolderPath, GetItemName(de), de);
                    }
                    else if (ex.SelectedFolderPath.LastIndexOf('\\') != -1)
                    {
                        var dir = ex.SelectedFolderPath.Substring(0, ex.SelectedFolderPath.LastIndexOf('\\') + 1);
                        if (Directory.Exists(dir))
                            SaveFile(dir, ex.SelectedFolderPath.Substring(dir.Length), de);
                    }
            }
        }

        private void SaveFile(string dir, string fname, MimeEntity entity)
        {
            var filename = dir.TrimEnd('\\') + "\\" + fname;
            var save = true;
            if (File.Exists(filename))
            {
                var mr = MessageBox.Show(Properties.Resources.FileExistWarningWords, Properties.Resources.WarningWord,
                    MessageBoxButton.YesNo);
                if (mr != MessageBoxResult.Yes)
                    save = false;
            }

            if (save)
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(entity.Data, 0, entity.Data.Length);
                }
        }
    }
}