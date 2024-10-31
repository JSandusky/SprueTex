using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;

using System.Windows.Forms;
using SprueKit.Commands;
using SprueKit.Dlg;
using System.Xml.Serialization;
using Notify;

namespace SprueKit
{
    public delegate void NewDocumentHandler(Document doc);
    public delegate void DocumentSavedHandler(Document doc);
    public delegate void DocumentClosedHandler(Document doc);

    public interface IDocumentControl
    {
        Document Document { get; set; }
    }

    /// <summary>
    /// Interface for documents and controls that need to respond to clipboard commands
    /// </summary>
    public interface IClipboardControl
    {
        bool Cut();
        bool Copy();
        bool Paste();
    }

    public class DocumentControls : IDisposable
    {
        public System.Windows.UIElement LeftPanelControl { get; set; }
        public System.Windows.UIElement RightPanelControl { get; set; }
        public System.Windows.UIElement ContentControl { get; set; }

        public List<IDisposable> Disposables { get; private set; } = new List<IDisposable>();

        public virtual void Dispose()
        {
            foreach (var dispo in Disposables)
                dispo.Dispose();
        }
    }

    /// <summary>
    /// Base-class for document types. Contains the selection context and undo/redo stacks
    /// </summary>
    public class Document : BaseClass, IDisposable
    {

        public virtual void Dispose()
        {

        }

        static int nextID_ = 0;

        #region Events
        public event DocumentSavedHandler DocumentSaved = delegate { };
        public event DocumentClosedHandler DocumentClosed = delegate { };
        #endregion

        Commands.CommandInfo[] cmds_ = null;
        public Commands.CommandInfo[] DocumentCommands { get { return cmds_; } set { cmds_ = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public DocumentControls Controls { get; private set; } = new DocumentControls();

        public string DocumentTypeName { get; protected set; } = "<ERROR-UNSPECIFIED-DOC-NAME>";
        public string FileMask { get; protected set; }

        Uri uri_;
        public Uri FileURI { get { return uri_; } set { uri_ = value; OnPropertyChanged("DisplayName"); OnPropertyChanged("DocumentName"); OnPropertyChanged(); } }

        bool dirty_ = true;
        public bool IsDirty { get { return dirty_; } set { dirty_ = value; OnPropertyChanged(); } }

        public int ID { get; private set; }
        public SelectionContext Selection { get; private set; } = new SelectionContext();
        public UndoStack UndoRedo { get; private set; } = new UndoStack();

        public Tracker Tracker_UndoRedo { get; private set; } = new Tracker();

        public virtual string DisplayName { get
            {
                if (FileURI != null)
                    return string.Format("{0} : {1}", System.IO.Path.GetFileNameWithoutExtension(FileURI.AbsolutePath), DocumentTypeName);
                return string.Format("<unnamed> : {0}", DocumentTypeName);
            }
        }

        public virtual string DocumentName { get {
                if (FileURI != null)
                    return System.IO.Path.GetFileNameWithoutExtension(FileURI.AbsolutePath);
                return "<unnamed>";
            }
        }

        bool active_ = false;
        public bool IsActive { get { return active_; } set { active_ = value; OnPropertyChanged(); } }

        public List<Control> AssociatedControls { get; private set; } = new List<Control>();

        public void AddControl(Control ctrl)
        {
            AssociatedControls.Add(ctrl);
        }

        public T GetControl<T>() where T : class
        {
            foreach (var ctrl in AssociatedControls)
            {
                if (ctrl is T)
                    return ctrl as T;
            }
            return default(T);
        }

        public Document()
        {
            ID = ++nextID_;
        }

        public virtual void OnActivate()
        {
            IsActive = true;
        }

        public virtual void OnDeactivate()
        {
            IsActive = false;
        }

        public virtual bool Save(bool safetyPrompt = false)
        {
            if (FileURI == null)
            {
                System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                dlg.AddExtension = true;
                dlg.Filter = FileMask;
                dlg.Title = "Save " + DocumentTypeName;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    FileURI = new Uri(dlg.FileName);
                    if (WriteFile(FileURI))
                    {
                        IsDirty = false;
                        UserData.inst().AddRecentFile(new Uri(dlg.FileName));
                        return true;
                    }
                }
                return false;
            }
            else //if (IsDirty)
            {
                Debug.Assert(FileURI != null && FileURI.IsFile, "Document FileURI must be valid and routed to a file");
                if (safetyPrompt)
                {
                    if (Dlg.ConfirmDlg.Show(string.Format("Save changes to file '{0}'", System.IO.Path.GetFileName(FileURI.LocalPath)), "Save"))
                    { 
                        if (WriteFile(FileURI))
                        {
                            UserData.inst().AddRecentFile(FileURI);
                            IsDirty = false;
                            return true;
                        }
                    }
                }
                else
                {
                    if (WriteFile(FileURI))
                    {
                        UserData.inst().AddRecentFile(FileURI);
                        IsDirty = false;
                        return true;
                    }
                }
                
            }
            return true;
        }

        public virtual void Export()
        {

        }

        public virtual bool SaveAs()
        {
            System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.Filter = FileMask;
            dlg.Title = "Save " + DocumentTypeName + " As...";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileURI = new Uri(dlg.FileName);
                if (WriteFile(FileURI))
                {
                    UserData.inst().AddRecentFile(FileURI);
                    IsDirty = false;
                    return true;
                }
            }
            return false;
        }

        public virtual bool WriteFile(Uri path) { return false; }

        public virtual bool Close()
        {
            if (FileURI == null || IsDirty)
            {
                bool pass = true;
                if (FileURI != null) // exists but is dirty
                {
                    var result = SaveIgnoreCancelDlg.Show(string.Format("Save changes to '{0}'", FileURI), "Save Changes?");
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        Save();
                        return true;
                    }
                    else if (result == System.Windows.MessageBoxResult.Cancel)
                        return false;
                }
                else
                {
                    var result = SaveIgnoreCancelDlg.Show("This brand new file has never been saved.", string.Format("Save New {0}", DocumentTypeName));
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        return Save();
                    }
                    else if (result == System.Windows.MessageBoxResult.Cancel)
                        return false;
                }
                if (pass)
                    return true;
            }
            return true;
        }
    }

    /// <summary>
    /// Caches a collection
    /// </summary>
    /// <typeparam name="T">Type to be stored</typeparam>
    public class DocumentCache<T> : Dictionary<int,T> where T : class, new()
    {
        public virtual T Get(Document doc)
        {
            if (doc == null)
                return null;
            T ret = null;
            if (TryGetValue(doc.ID, out ret))
                return ret;
            return null;
        }

        public virtual void Add(Document doc, T o)
        {
            this[doc.ID] = o;
        }

        public virtual T GetOrCreate(Document doc)
        {
            // No doc then no result
            if (doc == null)
                return null;

            // First check if we have one stored
            T ret = null;
            if (TryGetValue(doc.ID, out ret))
                return ret;

            // Create new
            ret = new T();
            if (typeof(IDocumentControl).IsAssignableFrom(typeof(T)))
                ((IDocumentControl)ret).Document = doc;
            this[doc.ID] = ret;

            // Attach for cleanup
            doc.DocumentClosed += (d) => {
                Remove(d.ID);
            };

            return ret;
        }

        public bool Remove(Document doc)
        {
            return Remove(doc.ID);
        }
    }

    /// <summary>
    /// Manages the collections of open documents
    /// </summary>
    public class DocumentManager : BaseClass
    {
        Document activeDocument_;

        public Document ActiveDocument { get { return activeDocument_; } private set { activeDocument_ = value;  OnPropertyChanged(); } }

        public delegate void NewDocumentHandler(Document newDoc);
        public event NewDocumentHandler OnDocumentOpened = delegate { };

        public delegate void DocumentClosedHandler(Document closing);
        public event DocumentClosedHandler OnDocumentClosed = delegate { };

        public delegate void ActiveDocumentChangedHandler(Document newDoc, Document oldDoc);
        public event ActiveDocumentChangedHandler OnActiveDocumentChanged = delegate { };

        public ObservableCollection<Document> OpenDocuments { get; private set; } = new ObservableCollection<Document>();

        public T ActiveDoc<T>() where T : class
        {
            return ActiveDocument as T;
        }

        public void AddDocument(Document newDoc)
        {
            OpenDocuments.Add(newDoc);
            OnDocumentOpened(newDoc);
            SetActiveDocument(newDoc);
        }

        public void SetActiveDocument(Document newDoc)
        {
            if (newDoc == activeDocument_)
                return;

            var oldDoc = activeDocument_;
            if (activeDocument_ != null)
                activeDocument_.OnDeactivate();

            ActiveDocument = newDoc;
            OnActiveDocumentChanged(newDoc, oldDoc);

            if (newDoc != null)
                newDoc.OnActivate();
        }

        public bool CloseDocument(Document doc)
        {
            if (doc.Close())
            {
                OpenDocuments.Remove(doc);
                OnDocumentClosed(doc);
                if (doc == activeDocument_)
                {
                    if (OpenDocuments.Count > 0)
                        SetActiveDocument(OpenDocuments[0]);
                    else
                        SetActiveDocument(null);
                }
                doc.Controls.Dispose();
                doc.Dispose();
                return true;
            }
            return false;
        }

        public void SaveAll()
        {
            foreach (var doc in OpenDocuments)
                doc.Save();
        }

        public void CloseCurrent()
        {
            if (ActiveDocument != null)
                CloseDocument(ActiveDocument);
        }

        public bool CloseAllFiles()
        {
            var docs = OpenDocuments.ToArray();
            foreach (var doc in docs)
            {
                if (!CloseDocument(doc))
                    return false;
            }
            OpenDocuments.Clear();
            return true;
        }
    }
}
