using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.CodeGen
{
    public partial class EventEntryPoint : Graph.GraphNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Event ...";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Start", IsInput = false, IsOutput = true, IsFlow = true });
        }
    }

    public partial class IfNode : Graph.GraphNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "If ...";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Test" });
            AddOutput(new Graph.GraphSocket(this) { Name = "True", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "False", IsOutput = true, IsFlow = true });
        }

        public override void Execute(object param)
        {
            if (InputSockets[1].GetBool())
                ForceExecuteSocketDownstream(OutputSockets[0]);
            else
                ForceExecuteSocketDownstream(OutputSockets[1]);
        }
    }

    public partial class SwitchNode : Graph.GraphNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Switch";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name= "Value", IsInput = true, IsFlow = false });

        }

        public override void Execute(object param)
        {
            var selectedValue = InputSockets[1].GetInt();
            if (selectedValue < OutputSockets.Count)
                ForceExecuteSocketDownstream(OutputSockets[selectedValue]);
        }
    }

    public partial class WhileNode : Graph.GraphNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "While ...";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Test" });
            AddOutput(new Graph.GraphSocket(this) { Name = "Finished", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "Loop", IsOutput = true, IsFlow = true });
        }

        public override void Execute(object param)
        {
            int limit = 1000;
            while (InputSockets[1].GetBool() && limit > 0)
            {
                ForceExecuteSocketDownstream(OutputSockets[1]);
                ForceExecuteSocketUpstream(param, InputSockets[1]);
                --limit;
            }
            ForceExecuteSocketDownstream(OutputSockets[0]);
        }
    }

    public partial class FlipFlopNode : Graph.GraphNode
    {
        bool state_ = false;

        public override void Construct()
        {
            base.Construct();
            Name = "FlipFlop ...";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "A", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "B", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "Is A", IsOutput = true, IsFlow = false });
        }

        public override void Execute(object param)
        {
            state_ = !state_;
            OutputSockets[2].Data = state_;
            if (state_)
                ForceExecuteSocketDownstream(OutputSockets[0]);
            else
                ForceExecuteSocketDownstream(OutputSockets[1]);
        }
    }

    public partial class ForNode : Graph.GraphNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "While ...";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Increment" });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Target" });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true, Name = "Break" });

            AddOutput(new Graph.GraphSocket(this) { Name = "Finished", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "Loop", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "Index", IsOutput = true, IsFlow = false });
        }

        public override void Execute(object param)
        {
            //TODO
        }
    }

    public partial class DoOnce : Graph.GraphNode
    {
        bool state_ = false;

        public override void Construct()
        {
            base.Construct();
            Name = "Do Once";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Reset", IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Start Closed?" });
            AddOutput(new Graph.GraphSocket(this) { Name = "Exec", IsOutput = true, IsFlow = true });
        }

        public override void Execute(object param)
        {
            if (state_ == false)
            {
                state_ = true;
                ForceExecuteSocketDownstream(OutputSockets[0]);
            }
        }
    }

    public partial class DoN : Graph.GraphNode
    {
        int counter_ = 0;

        public override void Construct()
        {
            base.Construct();
            Name = "Do N...";
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "N" });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Reset", IsInput = true, IsFlow = true });

            AddOutput(new Graph.GraphSocket(this) { Name = "Exec", IsOutput = true, IsFlow = true });
            AddOutput(new Graph.GraphSocket(this) { Name = "Counter", IsOutput = true, IsFlow = false });
        }

        public override void Execute(object param)
        {
            if (counter_ < InputSockets[0].GetInt())
            {
                ++counter_;
                OutputSockets[1].Data = counter_;
                ForceExecuteSocketDownstream(OutputSockets[0]);
            }
        }
    }

    public partial class Gate : Graph.GraphNode
    {
        bool state_ = true;
        public override void Construct()
        {
            base.Construct();
            Name = "Gate";
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Enter", IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Open", IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Close", IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { Name = "Toggle", IsInput = true, IsFlow = true });
            AddInput(new Data.Graph.GraphSocket(this) { IsInput = true, IsFlow = false, Name = "Start Closed?" });

            AddOutput(new Graph.GraphSocket(this) { Name = "Exec", IsOutput = true, IsFlow = true });
        }

        public override void Execute(object param)
        {
            if (state_)
                ForceExecuteSocketDownstream(OutputSockets[0]);
        }

        void Open() { state_ = true; }
        void Close() { state_ = false; }
        void Toggle() { state_ = !state_; }
    }
}
