using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics
{
    public abstract class VisualAnimation3D
    {
        public virtual void Prepare(Visual3D owner) {  }
        public abstract bool Update(Visual3D target, float timeStep);
        public abstract void ForceFinished(Visual3D target);
    }

    public class Visual3D
    {
        VisualAnimation3D animation_;
        List<Visual3D> children_ = new List<Visual3D>();

        public List<Visual3D> Children { get { return children_; } }

        public VisualAnimation3D ActiveAnimation {
            get { return animation_; }
            set
            {
                if (animation_ != null)
                    animation_.ForceFinished(this);
                animation_ = value;
                if (animation_ != null)
                    animation_.Prepare(this);
            }
        }

        public void UpdateAnimations(float td)
        {
            if (animation_ != null)
            {
                if (animation_.Update(this, td))
                    animation_ = null;
            }

            foreach (Visual3D child in children_)
                child.UpdateAnimations(td);
        }
    }
}
