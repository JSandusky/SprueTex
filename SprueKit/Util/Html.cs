using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    public class CSSClass
    {
        public string Selector { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public class HTMLStyleSheet
    {
        public List<CSSClass> Classes { get; private set; } = new List<CSSClass>();
    }

    public class HTMLBuilder
    {
        // tag, TRUE if inline pop
        Stack<KeyValuePair<string, bool>> tagStack_ = new Stack<KeyValuePair<string, bool>>();
        StringBuilder stream_ = new StringBuilder();

        public HTMLBuilder(string title)
        {
            WriteHeader(title);
        }

        public HTMLBuilder(string title, HTMLStyleSheet styleSheet)
        {
            WriteHeader(title, styleSheet);
        }

        public HTMLBuilder(string title, string css)
        {
            WriteHeader(title, css);
        }

        public void FinishDocument()
        {
            while (tagStack_.Count > 0)
                PopTag();
        }

        public string HTMLText { get { return stream_.ToString(); } }

        public void Raw(string text)
        {
            stream_.Append(text);
        }

        /// Writes raw text data.
        public void Text(string text)
        {
            stream_.Append(System.Web.HttpUtility.HtmlEncode(text));
        }
        /// Write an img tag with source. Does not require popping the tag as it is self closing.
        public void Img(string path, string style = "")
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<img src=\"{0}\"", path);
            if (!string.IsNullOrEmpty(style))
                stream_.AppendFormat(" class=\"{0}\"", style);
            stream_.Append(" />");
            LineBreak();
        }
        public void Img(string path, int width, int height, string style = "")
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<img src=\"{0}\" width=\"{1}\" height=\"{2}\"", path, width, height);
            if (!string.IsNullOrEmpty(style))
                stream_.AppendFormat(" class=\"{0}\"", style);
            stream_.Append(" />");
            LineBreak();
        }
        public void EmbedGIF(string base64)
        {
            stream_.AppendFormat("<img src=\"data:image/gif;base64,{0}\" />", base64);
        }
        public void ImgEmbedded(System.Drawing.Bitmap image, int targetDim, string style = "")
        {
            float xFactor = ((float)image.Width) / (float)targetDim;
            float yFactor = ((float)image.Height) / (float)targetDim;

            int nx = image.Width > image.Height ? image.Width / targetDim : (int)(image.Width * (1.0f / yFactor)),
                ny = image.Height > image.Width ? image.Height / targetDim : (int)(image.Width * (1.0f / xFactor));

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            byte[] data = stream.ToArray();
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<img src=\"data:image/png;base64,{0}\" />", Convert.ToBase64String(data));
            LineBreak();
        }
        /// Write a base64 embedded image. Does not require popping the tag as it is self closing.
        public void ImgEmbedded(SprueBindings.ImageData image, int targetDim, string style = "")
        {
            float xFactor = ((float)image.Width ) / (float)targetDim;
            float yFactor = ((float)image.Height) / (float)targetDim;

            int nx = image.Width > image.Height ? image.Width / targetDim : (int)(image.Width *  (1.0f / yFactor)), 
                ny = image.Height > image.Width ? image.Height / targetDim : (int)(image.Width * (1.0f / xFactor));

            var img = image.GetResized(nx, ny);
            byte[] data = SprueBindings.ImageData.ToMemory(img, ErrorHandler.inst());
            stream_.AppendFormat("<img src=\"data:image/png;base64,{0}\" />", Convert.ToBase64String(data));
        }

        /// Write an anchor tag for #NAME links. Does not require popping the tag as it is self closing.
        public void Anchor(string name)
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<a name=\"#{0}\"></a>", name);
            LineBreak();
        }
        public void AnchorLink(string name, string text)
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<a href=#\"{0}\">{1}</a> ", name, text);
            LineBreak();
        }
        /// Write an anchor/link tag with an href.
        public void Link(string url, string style = "")
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<a href=\"{0}\"", url);
            if (!string.IsNullOrEmpty(style))
                stream_.AppendFormat(" class=\"{0}\"", style);
            stream_.Append(" >");
            tagStack_.Push(new KeyValuePair<string,bool>("a", true));
        }
        public void Header(string text, int level) { Header(level); Text(text); PopTag(); }
        /// Write a h# tag.
        public void Header(int level, string style = "") { PushTag(string.Format("h{0}", level), style, false, true); }
        /// Write a div tag.
        public void Div(string style = "") { PushTag("div", style); }

        public void DivIndent() { stream_.Append("<div style=\"margin-left: 30px\">"); tagStack_.Push(new KeyValuePair<string, bool>("div", false)); }

        /// Write a paragrapht ag.
        public void P(string style = "") { PushTag("p", style); }

        /// Write a table tag.
        public void Table(string style = "") { PushTag("table", style); }
        /// Write a table header cell.
        public void Th(int colSpan = 1, int rowSpan = 1, string style = "") { TableCell("th", colSpan, rowSpan, style); }
        /// Write a table row.
        public void Tr(string style = "") { PushTag("tr", style); }
        /// Write a table cell.
        public void Td(int colSpan = 1, int rowSpan = 1, string style = "") { TableCell("td", colSpan, rowSpan, style); }
        public void TD(string text) { Td(); Text(text); PopTag(); }
        public void TD_Bold(string text) { Td(); Bold(text); PopTag(); }
        /// Writes a break tag.
        public void Br() { stream_.Append("<br />"); }
        /// Writes a horizontal rule.
        public void Hr() { stream_.Append("<hr />"); }

        /// Write an onordered list.
        public void UL(string style = "") { PushTag("ul", style); LineBreak(); }
        /// Write an ordered list.
        public void OL(string style = "") { PushTag("ol", style); }
        /// Write a list item.
        public void LI(string style = "") { PushTag("li", style); }
        /// Helper function for creating a plain text <li>text</li>
        public void ListItem(string text, string style = "") { PushTag("li", style); Text(text); PopTag(); }
        /// Write a bold tag.
        public void Bold() { PushTag("b", ""); }

        /// Write an italic tag.
        public void Italic() { PushTag("i", ""); }
        /// Write an underline tag.
        public void Underline() { PushTag("u", ""); }
        /// Write a bold tag with text included.
        public void Bold(string text) { PushTag("b", ""); Text(text); PopTag(); }
        /// Write an italic tag with text included.
        public void Italic(string text) { PushTag("i", ""); Text(text); PopTag(); }
        /// Write an underline tag with text included.
        public void Underline(string text) { PushTag("u", ""); Text(text); PopTag(); }

        int inlineCount_ = 0;
        /// Pops a tag for closing from the stack.
        public void PopTag()
        {
            if (tagStack_.Count > 0)
            {
                var top = tagStack_.Peek();
                if (top.Value == true)
                {
                    --inlineCount_;
                    stream_.AppendFormat("</{0}> ", top.Key);
                    tagStack_.Pop();
                }
                else
                {
                    LineBreak();
                    WriteIndent(tagStack_.Count - 1);
                    stream_.AppendFormat("</{0}>", top.Key);
                    LineBreak();
                    tagStack_.Pop();
                }
            }
        }

        /// Writes an indent to reach the target depth.
        public void WriteIndent(int depth)
        {
            if (inlineCount_ == 0) // don't indent if we're in an inline grouping
            {
                for (int i = 0; i < depth; ++i)
                    stream_.Append("    ");
            }
        }

        /// Returns the current depth in the tag stack.
        public int GetDepth() { return tagStack_.Count; }

        /// Writes the <html><head><title>_____</title></head><body> cluster.
        protected void WriteHeader(string title)
        {
            stream_.Append(string.Format(
@"<html>
    <head>
        <title>{0}</title>
        <style>
            body {{ 
                color: #cccccc; 
                background: #222222;
                font-family: Arial !important;
            }}
            a {{
                color: #99FF99;
            }}
        </style>
    </head>
    <body>", title));

        }
        /// Writes the header and writes the contents of the stylesheet into the style tag.
        protected void WriteHeader(string title, HTMLStyleSheet stylesheet)
        {
            stream_.Append(string.Format(
@"<html>
    <head>
        <title>{0}</title>
        <style>", title));

            foreach (var clazz in stylesheet.Classes)
            {
                LineBreak();
                WriteIndent(3);

                stream_.AppendFormat("{0} {{\r\n", clazz.Selector);

                foreach (var property in clazz.Properties)
                {
                    LineBreak();
                    WriteIndent(4);
                    stream_.AppendFormat("{0}: {1};\r\n", property.Key, property.Value);
                }

                LineBreak();
                WriteIndent(3);
                stream_.Append("}");
            }

            stream_.Append(
@"      </style>
    </head>
    <body>");
        }

        protected void WriteHeader(string title, string css)
        {
            PushTag("html");
            PushTag("head");
            PushTag("title");
            stream_.Append(title);
            PopTag(); // title
            PushTag("style");

            Text(css);

            PopTag(); // style
            PopTag(); // head
            PushTag("body");
        }

        public void Close()
        {
            stream_.Append("</body></html>");
        }


        /// Writes the </body></html> cluster for when the document is finished.
        protected void WriteFooter()
        {
            PopTag();
            PopTag();
        }

        protected void TableCell(string tag, int colSpan, int rowSpan, string clazz)
        {
            WriteIndent(tagStack_.Count);
            stream_.AppendFormat("<{0} colspan=\"{1}\" rowspan=\"{2}\" valign=\"top\" ", tag, colSpan, rowSpan);// << "<" << tag << " colspan=\"" << colSpan << "\" rowspan=\"" << rowSpan << "\"";
            if (!string.IsNullOrEmpty(clazz))
                stream_.AppendFormat(" class=\"{1}\"", clazz);
            stream_.Append('>');
            tagStack_.Push(new KeyValuePair<string, bool>("td", true));
        }

        protected void PushTag(string tag, bool selfClosing = false, bool inlinePop = false)
        {
            PushTag(tag, "", selfClosing, inlinePop);
        }

        protected void PushTag(string tag, string clazz, bool selfClosing = false, bool inlinePop = false)
        {
            if (!inlinePop)
            {
                LineBreak();
                WriteIndent(tagStack_.Count);
            }
            stream_.AppendFormat("<{0}", tag);
            if (!string.IsNullOrEmpty(clazz))
                stream_.AppendFormat(" class=\"{0}\"", clazz);

            stream_.Append(selfClosing ? " />" : " >");
            if (!selfClosing)
                tagStack_.Push(new KeyValuePair<string,bool>(tag, inlinePop));
            if (inlinePop && !selfClosing)
                ++inlineCount_;
        }

        void LineBreak()
        {
            // don't insert line breaks unless we're outside of an inline block
            if (inlineCount_ <= 0)
                stream_.Append("\r\n");
        }
    }
}
