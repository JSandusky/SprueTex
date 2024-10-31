using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SprueKit.Controls
{
    public class CheckboxMatrix : Control
    {
        const int FLAG_DIM = 16;
        const int SPACING = 2;

        class FlagInfo
        {
            public int index;
            public int x;
            public int y;
        }

        FlagInfo[] flags_ = new FlagInfo[32];
        int hoverFlagIndex = -1;

        public delegate string CheckboxTooltipMethod(int index);
        public CheckboxTooltipMethod ToolMethod { get; set; }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(CheckboxMatrix),
            new FrameworkPropertyMetadata
            {
                DefaultValue = (uint)0,
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = OnValueChanged
            });

        SolidColorBrush checkBoxBrush = new SolidColorBrush(new Color { R = 51, G = 51, B = 51, A = 255 });
        Pen hoverBorderPen = new Pen(new SolidColorBrush(new Color { R = 27, G = 161, B = 226, A = 255 }), 2);
        SolidColorBrush checkedBrush = new SolidColorBrush(Colors.DarkGray);
        //SolidColorBrush checkedBrush = new SolidColorBrush(new Color { R = 193, G = 193, B = 193, A = 255 });

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as CheckboxMatrix;
            //self.InvalidateVisual();
        }

        public uint Value {
            get { return (uint)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public CheckboxMatrix()
        {
            Width = 17 * FLAG_DIM + 16 * 2;
            Height = FLAG_DIM * 2 + 2;
            for (int i = 0; i < flags_.Length; ++i)
            {
                flags_[i] = new FlagInfo { index = i };
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (hoverFlagIndex != -1)
                InvalidateVisual();
            hoverFlagIndex = -1;
            base.OnMouseLeave(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            //drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

            int maxX = 0;
            int curX = 0;
            int y = 0;

            for (uint i = 0; i < 2; ++i)
            {
                curX = 0;
                for (uint bit = 0; bit < 16; ++bit)
                {
                    // Add some spacing to seperate the bits into 2 halves
                    // Segmenting like this make it easier to visually remember bit layouts
                    if (bit == 8)
                        curX += FLAG_DIM;

                    uint bitOffset = 16 * i + bit;
                    //if (bitOffset > flags_.size())
                    //    break;

                    drawingContext.DrawRectangle(checkBoxBrush, bitOffset == hoverFlagIndex && IsMouseDirectlyOver ? hoverBorderPen : null, new Rect(curX, y, FLAG_DIM, FLAG_DIM));
                    if (((((uint)1) << (int)bitOffset) & Value) > 0)
                    {
                        // draw check
                        //drawingContext.DrawLine(hoverBorderPen, new Point(curX, y), new Point(curX + FLAG_DIM, y + FLAG_DIM));
                        //drawingContext.DrawLine(hoverBorderPen, new Point(curX, y + FLAG_DIM), new Point(curX + FLAG_DIM, y));
                        drawingContext.DrawRectangle(checkedBrush, null, new Rect(curX + 2, y + 2, FLAG_DIM - 4, FLAG_DIM - 4));
                    }

                    flags_[bitOffset].x = curX;
                    flags_[bitOffset].y = y;

                    curX += FLAG_DIM + 2;
                }
                maxX = Math.Max(maxX, curX);
                y += FLAG_DIM + 2;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            FlagInfo hitFlag = GetInfoAt(e.GetPosition(this));
            if (hitFlag != null)
            {
                ToolTip = GetToolTip(hitFlag.index);
                hoverFlagIndex = hitFlag.index;
                e.Handled = true;
                InvalidateVisual();
            }
            else
            {
                if (hoverFlagIndex != -1)
                    InvalidateVisual();
                hoverFlagIndex = -1;
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            FlagInfo hitFlag = GetInfoAt(e.GetPosition(this));
            if (hitFlag != null)
            {
                hoverFlagIndex = hitFlag.index;
                e.Handled = true;
                InvalidateVisual();
            }
            else
            {
                if (hoverFlagIndex != -1)
                    InvalidateVisual();
                hoverFlagIndex = -1;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (hoverFlagIndex != -1)
            {
                uint currentValue = Value;
                uint targetBit = ((uint)1) << hoverFlagIndex;

                if ((currentValue & targetBit) > 0) // already checked
                {
                    currentValue &= ~targetBit;
                    Value = currentValue;
                }
                else
                {
                    currentValue |= targetBit;
                    Value = currentValue;
                }
                InvalidateVisual();
            }
        }

        FlagInfo GetInfoAt(Point mousePoint)
        {
            for (int i = 0; i < flags_.Length; ++i)
            {
                Rect r = new Rect(flags_[i].x, flags_[i].y, FLAG_DIM, FLAG_DIM);
                if (r.Contains(mousePoint))
                    return flags_[i];
            }
            return null;
        }

        string GetToolTip(int flagIndex)
        {
            if (ToolMethod != null)
            {
                string ret = ToolMethod(flagIndex);
                if (!string.IsNullOrEmpty(ret))
                    return ret;
            }
            return string.Format("Bit {0} / 0x{1:x}", flagIndex + 1, (uint)1 << flagIndex);
        }
    }
}
