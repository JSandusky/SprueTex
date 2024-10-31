using SprueKit.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SprueKit.Data.TexGen
{
    public class TexGenRegenerateTask : TaskItem
    {
        Graph.Graph sourceGraph_;
        Graph.Graph cloneGraph_;
        TexGenNode sourceNode_;
        TexGenNode cloneNode_;
        Graphics.TexGraph.TexGraphViewport targetViewport_;
        int targetCounter_;
        bool isGeneratingHQ_ = false;
        int targetX_ = 0;
        int targetY_ = 0;

        public TexGenRegenerateTask(Graphics.TexGraph.TexGraphViewport view, Graph.Graph srcGrp, TexGenNode node) : base(null)
        {
            targetViewport_ = view;
            sourceGraph_ = srcGrp;
            cloneGraph_ = sourceGraph_.Clone();
            sourceNode_ = node;
            cloneNode_ = cloneGraph_.Nodes.FirstOrDefault(n => n.NodeID == sourceNode_.NodeID) as TexGenNode;
            targetCounter_ = ++sourceNode_.TaskCounter;
        }

        public override string TaskName {
            get {
                if (cloneNode_ == null)
                    return string.Empty;
                if (string.IsNullOrWhiteSpace(cloneNode_.Name))
                    return string.Format("{0}", cloneNode_.GetType().Name);
                return string.Format("{0}", cloneNode_.Name);
            }
        }

        public bool OnUpdateStatus(float time)
        {
            if (IsCanceled || sourceNode_.TaskCounter != targetCounter_)
                return false;
            if (isGeneratingHQ_)
                Message.Text = string.Format("{0}: {1}% @ {2}x{3}", TaskName, (int)(time * 100), targetX_, targetY_);
            else
                Message.Text = string.Format("{0}: {1}%", TaskName, (int)(time * 100));
            return true;
        }

        public override void TaskLaunch()
        {
            if (IsCanceled || sourceNode_.TaskCounter != targetCounter_)
                return;

            if (cloneNode_ != null)
            {
                int previewDim = new IOCDependency<Settings.TextureGraphSettings>().Object.PreviewResolution == Settings.TextureGraphPreviewResolution.Large ? 128 : 64;
                Message.Text = string.Format("{0}, priming", cloneNode_.Name);
                Stopwatch timer = new Stopwatch();
                timer.Start();

                cloneGraph_.Prime(new Vector2(previewDim, previewDim));
                cloneNode_.RefreshPreview(previewDim, previewDim, OnUpdateStatus);

                // Check before burdening the dispatcher
                if (sourceNode_.TaskCounter != targetCounter_)
                    return;
                App.Current.Dispatcher.Invoke(new Action(() => {
                    // still have to check because we may execute later than a better thread
                    if (sourceNode_.TaskCounter != targetCounter_)
                        return;
                    sourceNode_.Preview = cloneNode_.Preview;
                }));
                timer.Stop();
                ErrorHandler.inst().Info(string.Format("Generated texture '{0}' in {1}", cloneNode_.DisplayName, timer.Elapsed.ToString()));

                if (cloneNode_ is TextureOutputNode)
                {
                    Stopwatch subTimer = new Stopwatch();
                    subTimer.Start();
                    TextureOutputNode output = cloneNode_ as TextureOutputNode;
                    targetX_ = output.PreviewSize.X;
                    targetY_ = output.PreviewSize.Y;
                    isGeneratingHQ_ = true;
                    Message.Text = string.Format("{0}, priming", cloneNode_.Name);
                    cloneGraph_.Prime(new Vector2(output.PreviewSize.X, output.PreviewSize.Y));
                    var outputBMP = output.GeneratePreview(output.PreviewSize.X, output.PreviewSize.Y, OnUpdateStatus);

                    // Check before burdening the dispatcher
                    if (sourceNode_.TaskCounter != targetCounter_)
                        return;
                    subTimer.Stop();
                    ErrorHandler.inst().Info(string.Format("Generated detailed texture '{0}' @ {2}x{3} in {1}", cloneNode_.DisplayName, subTimer.Elapsed.ToString(), output.TargetSize.X, output.TargetSize.Y));
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        // Check again just in case
                        if (sourceNode_.TaskCounter != targetCounter_)
                            return;
                        targetViewport_.SetTexture(output.OutputChannel, outputBMP);
                        outputBMP.Dispose();
                    }));

                }
            }
        }
    }
}