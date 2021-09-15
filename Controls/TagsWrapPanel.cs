using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RehabKiosk.Controls
{
    public class TagsWrapPanel : Panel
    {

        protected override Size MeasureOverride(Size availableSize)
        {
            // Just take up all of the width
            Size finalSize = new Size { Width = availableSize.Width };
            double x = 0;
            double rowHeight = 0d;

            var children = InternalChildren;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i] as UIElement;

                // Tell the child control to determine the size needed
                child.Measure(availableSize);

                x += child.DesiredSize.Width;
                if (x > availableSize.Width)
                {
                    // this item will start the next row
                    x = child.DesiredSize.Width;

                    // adjust the height of the panel
                    finalSize.Height += rowHeight;
                    rowHeight = child.DesiredSize.Height;
                }
                else
                {
                    // Get the tallest item
                    rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
                }
            }

            // Add the final height
            finalSize.Height += rowHeight;
            return finalSize;
        }



        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect finalRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

            double rowHeight = 0;
            var children = InternalChildren;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i] as UIElement;
                if ((child.DesiredSize.Width + finalRect.X) > finalSize.Width)
                {
                    // next row!
                    finalRect.X = 0;
                    finalRect.Y += rowHeight;
                    rowHeight = 0;
                }
                // Place the item
                child.Arrange(new Rect(finalRect.X, finalRect.Y, child.DesiredSize.Width, child.DesiredSize.Height));

                // adjust the location for the next items
                finalRect.X += child.DesiredSize.Width;
                rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);
            }
            return finalSize;
        }

    }
}
