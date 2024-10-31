using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SprueKit
{
    public partial class App
    {
        public void InitShortCuts()
        {
            var mainGr = new App.ShortCutGroup {
                Name = "General",
                Description = "Basic commands for navigation and common needs",
                ShortCuts = new Commands.ShortCut[] {
                    // Navigation shortcuts
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Goto Home Screen", ToolTip = "Navigates to the home screen", Action = (d, o) => { App.Navigate(new Uri("Pages/LaunchScreen.xaml", UriKind.Relative)); } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Goto Settings Screen", ToolTip="Navigates to the settings creen", Action = (d, o) => { App.Navigate(new Uri("Pages/SettingsScreen.xaml", UriKind.Relative)); } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Goto Reports Screen", ToolTip="Navigates to the reports creen", Action = (d, o) => { App.Navigate(new Uri("Pages/ReportsScreen.xaml", UriKind.Relative)); } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Goto Guide Screen", ToolTip="Navigates to the guide creen", Action = (d, o) => { App.Navigate(new Uri("QuickGuide/QuickGuidePages.xaml", UriKind.Relative)); } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Open Quick Help", ToolTip="Navigates to the guide creen",
                        Action = (d, o) => {
                            Dlg.HelpWindow.ShowWindow();
                        }
                    }, Text = "CTRL + H" },

                    // Document shortcuts
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Save", ToolTip = "Saves the current document", Action = (d, o) => { var ioc = new IOCDependency<DocumentManager>().Object; if (ioc.ActiveDocument != null) ioc.ActiveDocument.Save(); } }, Text = "CTRL + S" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Save As", ToolTip = "Saves the current document as a new file", Action = (d, o) => { var ioc = new IOCDependency<DocumentManager>().Object; if (ioc.ActiveDocument != null) ioc.ActiveDocument.SaveAs(); } }, Text = "CTRL + SHIFT + S" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Save All", ToolTip = "Saves all open documents", Action = (d, o) => { new IOCDependency<DocumentManager>().Object.SaveAll(); } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Open File", ToolTip = "Opens a file for editing", Action = (d, o) => { Pages.LaunchScreen.OpenFile(); } }, Text = "CTRL + O" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Export", ToolTip = "Exports the current document", Action = (d, o) => {
                        var docMan = new IOCDependency<DocumentManager>().Object;
                        if (docMan.ActiveDocument != null)
                            docMan.ActiveDocument.Export();
                    } }, Text = "CTRL + E" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Close File", ToolTip = "Closes the current document",
                        Action = (d, o) => {
                            var docMan = new IOCDependency<DocumentManager>();
                            docMan.Object.CloseCurrent();
                            if (docMan.Object.OpenDocuments.Count == 0)
                                App.Navigate(new Uri("Pages/LaunchScreen.xaml", UriKind.Relative)); // goto home screen if no documents opened
                        } } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Close All", ToolTip = "Closes all opened documents",
                        Action = (d, o) => {
                            new IOCDependency<DocumentManager>().Object.CloseAllFiles();
                            App.Navigate(new Uri("Pages/LaunchScreen.xaml",UriKind.Relative)); // Goto home screen
                        } } },

                    // Document navigation shortcuts
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Next Document", ToolTip ="Navigates to the next opened document",
                    Action = (d,o) => {
                        if (DocumentManager.OpenDocuments.Count == 0)
                            return;

                        if (!App.IsOnPage(new Uri("Pages/DesignScreen.xaml", UriKind.Relative)))
                            App.Navigate(new Uri("Pages/DesignScreen.xaml", UriKind.Relative));
                        else
                        {
                            int idx = DocumentManager.OpenDocuments.IndexOf(DocumentManager.ActiveDocument);
                            if (idx != -1)
                            {
                                idx += 1;
                                if (idx >= DocumentManager.OpenDocuments.Count)
                                    idx = 0;
                                DocumentManager.SetActiveDocument(DocumentManager.OpenDocuments[idx]);
                            }
                        }
                    } }, Text = "CTRL + TAB" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo { Name = "Previous Document", ToolTip ="Navigates to the previous opened document",
                    Action = (d,o) => {
                        if (DocumentManager.OpenDocuments.Count == 0)
                            return;

                        if (!App.IsOnPage(new Uri("Pages/DesignScreen.xaml", UriKind.Relative)))
                            App.Navigate(new Uri("Pages/DesignScreen.xaml", UriKind.Relative));
                        else
                        {
                            int idx = DocumentManager.OpenDocuments.IndexOf(DocumentManager.ActiveDocument);
                            if (idx != -1)
                            {
                                idx -= 1;
                                if (idx < 0)
                                    idx =  DocumentManager.OpenDocuments.Count - 1;
                                DocumentManager.SetActiveDocument(DocumentManager.OpenDocuments[idx]);
                            }
                        }
                    } }, Text = "CTRL + SHIFT + TAB" },
                }
            };
            ShortCuts.Add(mainGr);

            ShortCuts.Add(new App.ShortCutGroup
            {
                Name = "History",
                Description = "Shortcuts for undo/redo",
                ShortCuts = new Commands.ShortCut[]
                {
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Undo", ToolTip="Reverses the top action in history",
                        Action = (doc, sel) =>
                        {
                            if (doc != null)
                                doc.UndoRedo.UndoOne();
                        },
                        Icon = WPFExt.GetEmbeddedImage("Images/appbar/appbar.undo.png")
                    }, Text = "CTRL + Z" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Redo", ToolTip="Reapplies the most recently undone command",
                        Action = (doc, sel) =>
                        {
                            if (doc != null)
                                doc.UndoRedo.RedoOne();
                        },
                        Icon = WPFExt.GetEmbeddedImage("Images/appbar/appbar.redo.png")
                    }, Text = "CTRL + Y" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Undo All", ToolTip = "Undoes all actions",
                        Action = (doc, sel) =>
                        {
                            if (doc != null)
                                doc.UndoRedo.UndoAll();
                        }
                    } },
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Redo All", ToolTip = "Reapplies all actions that were previously undone",
                        Action = (doc, sel) =>
                        {
                            if (doc != null)
                                doc.UndoRedo.RedoAll();
                        }
                    } },
                }
            });

            ShortCuts.Add(new App.ShortCutGroup
            {
                Name = "Clipboard",
                Description = "Shortcuts for clipboard actions",
                ShortCuts = new Commands.ShortCut[]
                {
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Cut", ToolTip = "Cut the current selection into the clipboard",
                        Action = (doc, sel) =>
                        {
                            var focused = FocusManager.GetFocusedElement(App.Current.MainWindow) as FrameworkElement;
                            if (focused != null && focused is IClipboardControl)
                                ((IClipboardControl)focused).Cut();
                            else if (focused != null && focused.Tag is IClipboardControl)
                                ((IClipboardControl)focused.Tag).Cut();
                            else if (focused is System.Windows.Controls.TextBox)
                                ((System.Windows.Controls.TextBox)focused).Cut();
                            else if (doc != null && doc is IClipboardControl)
                                ((IClipboardControl)doc).Cut();
                        }
                    }, Text = "CTRL + X" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Copy", ToolTip = "Copy the current selection into the clipboard",
                        Action = (doc, sel) =>
                        {
                            var focused = FocusManager.GetFocusedElement(App.Current.MainWindow) as FrameworkElement;
                            if (focused != null && focused is IClipboardControl)
                                ((IClipboardControl)focused).Copy();
                            else if (focused != null && focused.Tag is IClipboardControl)
                                ((IClipboardControl)focused.Tag).Copy();
                            else if (focused is System.Windows.Controls.TextBox)
                                ((System.Windows.Controls.TextBox)focused).Copy();
                            else if (doc != null && doc is IClipboardControl)
                                ((IClipboardControl)doc).Copy();
                        }
                    }, Text = "CTRL + C" },
                    new Commands.ShortCut { Command = new Commands.CommandInfo
                    {
                        Name = "Paste", ToolTip = "Paste the clipboard contents into the target",
                        Action = (doc, sel) =>
                        {
                            var focused = FocusManager.GetFocusedElement(App.Current.MainWindow) as FrameworkElement;
                            if (focused != null && focused is IClipboardControl)
                                ((IClipboardControl)focused).Paste();
                            else if (focused != null && focused.Tag is IClipboardControl)
                                ((IClipboardControl)focused.Tag).Paste();
                            else if (focused is System.Windows.Controls.TextBox)
                                ((System.Windows.Controls.TextBox)focused).Paste();
                            else if (doc != null && doc is IClipboardControl)
                                ((IClipboardControl)doc).Paste();
                        }
                    }, Text = "CTRL + V" }
                }
            });

        }

        void SprueKitShortCuts()
        { 
            ShortCuts.Add(new App.ShortCutGroup
            {
                Name = "Chain Piece Commands",
                AppliesToType = typeof(Data.ChainPiece),
                Description = "Shortcuts for editing modeling chains",
                ShortCuts = new Commands.ShortCut[] {
                    new Commands.ShortCut { Command = new Commands.CommandInfo {  Name ="Add Bone", ToolTip="Adds a bone to the end of the chain" } },
                }
            });

            ShortCuts.Add(new App.ShortCutGroup
            {
                Name = "Chain Bone Commands",
                AppliesToType = typeof(Data.ChainPiece.ChainBone),
                Description = "Fast manipulation of bones",
                ShortCuts = new Commands.ShortCut[] {
                    new Commands.ShortCut {Command = new Commands.CommandInfo {Name= "Delete Bone", ToolTip="Deletes the current selected bone" } },
                    new Commands.ShortCut {Command = new Commands.CommandInfo {Name= "Add Bone Before", ToolTip="Inserts a new bone before the currently selected one" } },
                    new Commands.ShortCut {Command = new Commands.CommandInfo {Name= "Add Bone After", ToolTip="Inserts a new bone after the currently selected one" } },
                }
            });
        }
    }

    public static class ShortCutExt
    {
        /// <summary>
        /// Finds a CommandInfo by shortcut group and command name.
        /// Used so commands can live in one global database.
        /// </summary>
        /// <param name="grp">Short cut group to look into</param>
        /// <param name="cmdName">Command to find</param>
        /// <returns>The found command or null</returns>
        public static Commands.CommandInfo GetCommand(string grp, string cmdName)
        {
            var foundGrp = App.ShortCuts.FirstOrDefault(s => s.Name.ToLowerInvariant().Equals(grp.ToLowerInvariant()));
            if (foundGrp != null)
            {
                var ret = foundGrp.ShortCuts.FirstOrDefault(sc => sc.Command.Name.ToLowerInvariant().Equals(cmdName.ToLowerInvariant()));
                if (ret != null)
                    return ret.Command;
            }
            return null;
        }
    }
}
