using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics
{
    /// <summary>
    /// Rhythmically pulses between two colors.
    /// </summary>
    public class SelectionColorer
    {
        public float DeltaTime = 0.0f;

        public void Update(float time)
        {
            DeltaTime += time;
        }

        public void Update(GameTime time)
        {
            DeltaTime += time.ElapsedGameTime.Milliseconds / 1000.0f;
        }

        public Color BaseColor { get; set; } = Color.LimeGreen;

        public Color ActiveColor { get; set; } = Color.Gold;

        public Color CurrentColor { get { return Color.Lerp(BaseColor, ActiveColor, GetTimeCurve()); } }

        public float GetTimeCurve() { return 0.5f + Mathf.Sin(GetAnimTime()) * 0.5f; }

        public float GetAnimTime() { return DeltaTime * 6.28318530718f; }
    }
}
