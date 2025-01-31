﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Controls.Code
{
    public abstract class FoldingStrategy
    {
        public abstract void UpdateFoldings(FoldingManager manager, TextDocument document);
    }
}
