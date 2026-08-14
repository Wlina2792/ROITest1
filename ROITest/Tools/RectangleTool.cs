using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 矩形绘制工具
/// </summary>
public class RectangleTool : DrawingToolBase
{
    protected override RoiShape CreateShape(Point startPos)
    {
        return new RectangleRoi(startPos.X, startPos.Y, 0, 0);
    }

    protected override void UpdateShape(RoiShape shape, Point start, Point current)
    {
        if (shape is not RectangleRoi rect) return;

        double x = Math.Min(start.X, current.X);
        double y = Math.Min(start.Y, current.Y);
        double w = Math.Abs(current.X - start.X);
        double h = Math.Abs(current.Y - start.Y);

        // 按住 Shift 保持正方形
        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(
            System.Windows.Input.ModifierKeys.Shift))
        {
            double size = Math.Max(w, h);
            w = size;
            h = size;
        }

        rect.X = x;
        rect.Y = y;
        rect.Width = w;
        rect.Height = h;
    }
}