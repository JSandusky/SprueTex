using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows.Controls;

namespace SprueKit.Controls
{

    public class BaseDocumentFrame<T> : ModernFrame where T : class, new()
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();
        DocumentCache<T> controlCache = new DocumentCache<T>();

        public BaseDocumentFrame()
        {
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;
            documentManager.Object.OnDocumentClosed += Object_OnDocumentClosed;
        }

        private void Object_OnDocumentClosed(Document closing)
        {
            controlCache.Remove(closing);
        }

        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            if (newDoc != null)
                Content = controlCache.GetOrCreate(newDoc);
            else
                Content = null;
        }
    }

    //public class DocumentFrame : BaseDocumentFrame<SceneTree>
    //{
    //    
    //}
}
