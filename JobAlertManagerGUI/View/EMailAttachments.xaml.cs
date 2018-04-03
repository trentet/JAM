using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using CryptoGateway.FileSystem.VShell;
using CryptoGateway.FileSystem.VShell.Interfaces;
using LumiSoft.Net.Mime;
using JobAlertManagerGUI.Model;

namespace JobAlertManagerGUI.View
{
    /// <summary>
    /// Interaction logic for EMailAttachments.xaml
    /// </summary>
    public partial class EMailAttachments : UserControl
    {
        [Import(typeof(IImageViewer))]
        private IImageViewer ImageViewer
        {
            get;
            set;
        }

        public EMailAttachments()
        {
            InitializeComponent();
            GBB.Header = Properties.Resources.AttachmentsWordQ;
            GBC.Header = Properties.Resources.PreviewWord;
            (LstAttach.View as GridView).Columns[0].Header = Properties.Resources.NameWord;
            string dir = Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length).Replace('/', '\\');
            dir = dir.Substring(0, dir.LastIndexOf('\\'));
            DirectoryCatalog categ = new DirectoryCatalog(dir);
            string cname = AttributedModelServices.GetContractName(typeof(IImageViewer));
            System.Linq.Expressions.Expression<Func<ExportDefinition, bool>> exp = a => a.ContractName == cname;
            ImportDefinition id = new ImportDefinition(exp, cname, ImportCardinality.ExactlyOne, true, true);
            List<Tuple<ComposablePartDefinition, ExportDefinition>> l = categ.GetExports(id).ToList();
            if (l.Count == 1)
            {
                var cc = new CompositionContainer(categ);
                cc.ComposeParts(this);
                cviewer.Content = ImageViewer;
            }
        }

        private bool IsLoadCompleted = false;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoadCompleted)
            {
                IsLoadCompleted = true;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is LumiSoft.Net.Mime.Mime)
            {
                LumiSoft.Net.Mime.Mime m = e.NewValue as LumiSoft.Net.Mime.Mime;
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
                MimeWrapper ma = e.AddedItems[0] as MimeWrapper;
                if (!HttpContentServer.IsServing)
                {
                    Action act = () =>
                    {
                        HttpContentServer.Start();
                    };
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
            string itemname = GetItemName(ma.Entity);
            if (!string.IsNullOrWhiteSpace(ma.Entity.ContentTypeString))
            {
                mimetype = ma.Entity.ContentTypeString;
                int ipos = mimetype.IndexOf(';');
                if (ipos != -1)
                {
                    mimetype = mimetype.Substring(0, ipos);
                }
            }
            else
            {
                string ext = itemname;
                if (ext.LastIndexOf('.') != -1)
                {
                    ext = ext.Substring(ext.LastIndexOf('.'));
                    Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext.ToLower());
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
                string cid = string.IsNullOrWhiteSpace(ma.Entity.ContentID) ? itemname : ma.Entity.ContentID;
                HttpContentServer.MediaBuffer = ma.Entity.Data;
                HttpContentServer.ContentTypeString = mimetype;
                HttpContentServer.State = HttpContentServerState.Data;
                if (async)
                {
                    Dispatcher.Invoke(((Action)(() =>
                    {
                        ImageViewer.SourceUri = new Uri(HttpContentServer.Prefix + "?id=" + cid);
                    })), null);
                }
                else
                {
                    ImageViewer.SourceUri = new Uri(HttpContentServer.Prefix + "?id=" + cid);
                }
            }
        }

        private string LastDepositFolder
        {
            get { return RootModel.LastDepositFolder; }
            set { RootModel.LastDepositFolder = value; }
        }

        private string GetItemName(MimeEntity entity)
        {
            return entity.ContentDisposition_FileName != null ? entity.ContentDisposition_FileName : (entity.ContentDescription != null ? entity.ContentDescription : (entity.ContentType_Name != null ? entity.ContentType_Name : "???"));
        }

        private void OnSaveAttachment(object sender, RoutedEventArgs e)
        {
            Button btn = e.OriginalSource as Button;
            if (!(btn.DataContext is MimeWrapper))
                return;
            MimeEntity de = (btn.DataContext as MimeWrapper).Entity;
            if (de != null)
            {
                TransCompSelectFolderEventArgs ex = new TransCompSelectFolderEventArgs(ComponentEvents.TransCompSelectFolderEvent);
                ex.EventTitle = Properties.Resources.AttachmentSaveDirSelWord;
                ex.InitialFolderPath = LastDepositFolder;
                RaiseEvent(ex);
                if (ex.Handled && ex.IsSelectionMade && !string.IsNullOrEmpty(ex.SelectedFolderPath))
                {
                    if (Directory.Exists(ex.SelectedFolderPath))
                    {
                        LastDepositFolder = ex.SelectedFolderPath;
                        SaveFile(ex.SelectedFolderPath, GetItemName(de), de);
                    }
                    else if (ex.SelectedFolderPath.LastIndexOf('\\') != -1)
                    {
                        string dir = ex.SelectedFolderPath.Substring(0, ex.SelectedFolderPath.LastIndexOf('\\') + 1);
                        if (Directory.Exists(dir))
                            SaveFile(dir, ex.SelectedFolderPath.Substring(dir.Length), de);
                    }
                }
            }
        }

        private void SaveFile(string dir, string fname, MimeEntity entity)
        {
            string filename = dir.TrimEnd('\\') + "\\" + fname;
            bool save = true;
            if (File.Exists(filename))
            {
                MessageBoxResult mr = MessageBox.Show(Properties.Resources.FileExistWarningWords, Properties.Resources.WarningWord, MessageBoxButton.YesNo);
                if (mr != MessageBoxResult.Yes)
                    save = false;
            }
            if (save)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(entity.Data, 0, entity.Data.Length);
                }
            }

        }


    }
}
