using System;
using System.Text;
using System.Windows.Input;
using System.Xml;

namespace SprueKit.Commands
{
    public class ShortCut : BaseClass
    {
        Key key_;
        ModifierKeys modifiers_;

        public Type[] AppliesToDocument { get; set; } = null;
        public Key Key { get { return key_; } set { key_ = value; OnPropertyChanged(); OnPropertyChanged("Text"); } }
        public ModifierKeys Modifiers { get { return modifiers_; } set { modifiers_ = value; OnPropertyChanged(); OnPropertyChanged("Text"); } }
        public CommandInfo Command { get; set; }

        public string Text { get { return ToString(); } set { SetBinding(value); } }

        public override string ToString()
        {
            if (!IsValid)
                return "< none >";
            StringBuilder ret = new StringBuilder();
            if (Modifiers.HasFlag(ModifierKeys.Control))
                ret.Append("CTRL + ");
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                ret.Append("SHIFT + ");
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                ret.Append("ALT + ");
            ret.Append(Key.ToString());
            return ret.ToString();
        }

        public bool IsValid { get { return Key != Key.None && Modifiers != ModifierKeys.None; } }

        public void Serialize(XmlElement parentElem)
        {
            var me = parentElem.CreateChild("shortcut");
            me.AddStringElement("command", Command.Name);
            me.AddStringElement("binding", Text);
        }

        public void SetBinding(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            modifiers_ = ModifierKeys.None;
            string[] terms = text.Split('+');
            foreach (string str in terms)
            {
                string trimmed = str.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    if (trimmed.Equals("CTRL"))
                        Modifiers = Modifiers | ModifierKeys.Control;
                    else if (trimmed.Equals("SHIFT"))
                        Modifiers = Modifiers | ModifierKeys.Shift;
                    else if (trimmed.Equals("ALT"))
                        Modifiers = Modifiers | ModifierKeys.Shift;
                    else
                    {
                        Key val = Key.None;
                        if (Enum.TryParse<Key>(trimmed, out val))
                            Key = val;
                    }
                }
            }
        }
    }
}
