using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SprueKit.Commands
{
    public class MacroCommand : UndoRedoCmd, IUndoStorage
    {
        List<UndoRedoCmd> Commands = new List<UndoRedoCmd>();

        public bool IsValid() { return Commands.Count > 0; }

        public void Prep()
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                if (Commands.Count > 1)
                    Message = String.Format("{0} actions: {1}", Commands.Count, Commands[0].Message);
                else
                    Message = Commands[0].Message;
            }
        }

        public override ImageSource Icon { get { return Commands[0].Icon; } }

        public void Add(UndoRedoCmd cmd)
        {
            Commands.Add(cmd);
        }

        public override void Merge(UndoRedoCmd cmd) { }

        protected override void Execute(bool isRedo)
        {
            if (isRedo)
            {
                foreach (var cmd in Commands)
                    cmd.Redo();
            }
            else
            {
                foreach (var cmd in Commands)
                    cmd.Undo();
            }
        }
    }

    public class MacroCommandBlock : IDisposable
    {
        static Stack<MacroCommand> block_ = new Stack<MacroCommand>();
        static Stack<IUndoStorage> targetStacks_ = new Stack<IUndoStorage>();
        public MacroCommandBlock(string msg = null)
        {
            block_.Push(new MacroCommand() { Message = msg });
        }

        public static IUndoStorage GetUndoStorage(IUndoStorage tgt)
        {
            if (block_.Count > 0)
            {
                // not enough block stacks
                if (targetStacks_.Count < block_.Count)
                    targetStacks_.Push(tgt);
                return block_.Peek();
            }
            return tgt;
        }

        public void Dispose()
        {
            if (targetStacks_.Count > 0)
            {
                var stack = targetStacks_.Pop();
                var block = block_.Peek();
                if (block.IsValid())
                {
                    block.Prep();
                    stack.Add(block);
                }
            }
            block_.Pop();
        }
    }
}
