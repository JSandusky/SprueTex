using FirstFloor.ModernUI.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SprueKit.Commands;

namespace SprueKit.Dlg
{
    /// <summary>
    /// Interaction logic for QuickAction.xaml
    /// </summary>
    public partial class QuickAction : ModernDialog
    {
        public CollectionView CutViewSource { get; set; }
        List<Commands.ShortCut> ShortCuts { get; set; } = new List<Commands.ShortCut>();

        IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();

        public QuickAction()
        {
            InitializeComponent();
            Loaded += QuickAction_Loaded;
            PreviewKeyUp += QuickAction_KeyUp;

        // Grab global commands
            foreach (var grp in App.ShortCuts)
            {
                // Commands may only apply to a specific type of object
                // check type appropriate-ness
                if (grp.AppliesToType != null)
                {
                    if (docMan.Object.ActiveDocument != null && docMan.Object.ActiveDocument.Selection != null && docMan.Object.ActiveDocument.Selection.MostRecentlySelected != null)
                    {
                        if (grp.AppliesToType.IsAssignableFrom(docMan.Object.ActiveDocument.Selection.MostRecentlySelected.GetType()))
                            goto is_okay;
                    }
                    // Cannot use this command group with the current selection
                    continue;
                }

                // Verify that each command is allowed for the current document
                is_okay:
                foreach (var cut in grp.ShortCuts)
                {
                    if (cut.AppliesToDocument != null)
                    {
                        if (docMan.Object.ActiveDocument != null && cut.AppliesToDocument.Contains(docMan.Object.ActiveDocument.GetType()))
                            goto still_okay;
                        // not allowed, ignore it
                        continue;
                    }

                    still_okay:
                    this.itemsList.Items.Add(new ListBoxItem
                    {
                        Content = cut.Command.Name,
                        Tag = cut
                    });
                }
            }

        // Grab any commands the focused control pushes
            var focused = GeneralUtility.FindActionSource(FocusManager.GetFocusedElement(App.Current.MainWindow) as DependencyObject);
            if (focused != null)
            {
                CommandInfo[] cmds = ((IQuickActionSource)focused).GetCommands();
                foreach (var cmd in cmds)
                    itemsList.Items.Add(new ListBoxItem { Content = cmd.Name, Tag = cmd });
            }

        // Grab any commands the active document pushes
            if (docMan.Object.ActiveDocument != null)
            {
                var docActionSource = docMan.Object.ActiveDocument as IQuickActionSource;
                if (docActionSource != null)
                {
                    CommandInfo[] cmds = docActionSource.GetCommands();
                    if (cmds != null)
                    {
                        foreach (var cmd in cmds)
                            itemsList.Items.Add(new ListBoxItem { Content = cmd.Name, Tag = cmd });
                    }
                }
            }

            var view = CollectionViewSource.GetDefaultView(itemsList.Items);
            view.Filter = FilterFunction;

            Buttons = new Button[] {
                CancelButton
            };
        }

        private void QuickAction_Loaded(object sender, RoutedEventArgs e)
        {
            queryBox.Focus();
        }

        private void QuickAction_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (queryBox.Text.StartsWith("dev"))
                {
                    string paramString = queryBox.Text.Substring("dev ".Length);
                    string[] terms = paramString.Split(' ');
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 1; i < terms.Length; ++i)
                    {
                        sb.Append(terms[i]);
                        if (i < terms.Length - 1)
                            sb.Append(' ');
                    }
                    RunDevCommand.RunCommand(terms[0], sb.ToString());

                    e.Handled = true;
                    Close();
                    return;
                }

                ListBoxItem chosen = null;
                var view = CollectionViewSource.GetDefaultView(itemsList.Items);
                if (itemsList.SelectedItem != null)
                    chosen = itemsList.SelectedItem as ListBoxItem;
                else if (!view.IsEmpty)
                {
                    var enumerator = view.GetEnumerator();
                    enumerator.MoveNext(); // sets it to the first element
                    var firstElement = enumerator.Current;
                    if (firstElement != null)
                        chosen = firstElement as ListBoxItem;
                    //chosen = CollectionViewSource.GetDefaultView(itemsList.Items).CurrentItem as ListBoxItem;
                }

                if (chosen != null)
                    ExecuteCommand(chosen);
            }
        }

        void ExecuteCommand(ListBoxItem item)
        {
            CommandInfo cmd = null;
            if (item.Tag is Commands.ShortCut)
                cmd = ((Commands.ShortCut)item.Tag).Command;
            else if (item.Tag is CommandInfo)
                cmd = ((CommandInfo)item.Tag);

            if (cmd != null)
            {
                if (cmd.Action != null)
                {
                    if (docMan.Object.ActiveDocument != null)
                        cmd.Action(docMan.Object.ActiveDocument, docMan.Object.ActiveDocument.Selection.MostRecentlySelected);
                    else
                        cmd.Action(null, null);
                }
            }
            Close();
        }

        private void queryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var v = CollectionViewSource.GetDefaultView(itemsList.Items);
            v.Filter = FilterFunction;
            v.Refresh();
        }

        private void itemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (itemsList.SelectedItem != null)
            {
                var lb = itemsList.SelectedItem as ListBoxItem;
                if (lb != null)
                    ExecuteCommand(lb);
            }
        }

        string GetItemText(ListBoxItem item)
        {
            if (item.Tag is Commands.ShortCut)
                return ((Commands.ShortCut)item.Tag).Command.Name;
            return ((Commands.CommandInfo)item.Tag).Name;
        }

        private bool FilterFunction(object o)
        {
            var item = o as ListBoxItem;
            if (item != null)
            {
                if (!string.IsNullOrWhiteSpace(queryBox.Text))
                {
                    if (GetItemText(item).ToLowerInvariant().Contains(queryBox.Text.ToLowerInvariant()))
                        return true;
                    return false;
                }
            }
            return true;
        }
    }
}
