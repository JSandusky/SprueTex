using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Text.RegularExpressions;

namespace SprueKit.Controls
{
    public class UriTextBox : TextBox, IDropTarget
    {
        public Regex[] RegexChecks { get; set; }

        public UriTextBox()
        {
            SetValue(GongSolutions.Wpf.DragDrop.DragDrop.IsDropTargetProperty, true);
            SetValue(GongSolutions.Wpf.DragDrop.DragDrop.DropHandlerProperty, this);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            Data.FileData file = dropInfo.Data as Data.FileData;
            if (file != null)
            {
                bool passes = false;
                if (RegexChecks != null)
                {
                    foreach (var check in RegexChecks)
                        if (check.IsMatch(file.FilePath))
                            passes = true;
                }

                if (passes)
                {
                    dropInfo.Effects = System.Windows.DragDropEffects.Link | System.Windows.DragDropEffects.Move;
                    return;
                }
            }
            else if (dropInfo.Data is DataObject)
            {
                DataObject dataObject = dropInfo.Data as DataObject;
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    bool passes = false;
                    if (RegexChecks != null)
                    {
                        foreach (var check in RegexChecks)
                            if (check.IsMatch(files[0]))
                                passes = true;
                    }

                    if (passes)
                    {
                        dropInfo.Effects = System.Windows.DragDropEffects.Link | System.Windows.DragDropEffects.Move;
                        return;
                    }
                }
            }

            dropInfo.Effects = System.Windows.DragDropEffects.None;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            Data.FileData file = dropInfo.Data as Data.FileData;
            if (file != null)
                Text = file.FilePath;
            else if (dropInfo.Data is DataObject)
            {
                DataObject dataObject = dropInfo.Data as DataObject;
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    Text = files[0];
                }
            }
            dropInfo.Effects = System.Windows.DragDropEffects.Link;
        }
    }
}
