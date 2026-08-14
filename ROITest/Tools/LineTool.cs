using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 线段绘制工具 —— 按下确定起点，拖动确定终点
/// </summary>
public class LineTool : DrawingToolBase
{
    protected override RoiShape CreateShape(Point startPos)
    {
        return new LineRoi(startPos.X, startPos.Y, startPos.X, startPos.Y);
    }

    protected override void UpdateShape(RoiShape shape, Point start, Point current)
    {
        if (shape is not LineRoi line) return;

        line.X2 = current.X;
        line.Y2 = current.Y;

        // 按住 Shift 约束为 0°/45°/90°/135°
        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(
            System.Windows.Input.ModifierKeys.Shift))
        {
            double dx = current.X - start.X;
            double dy = current.Y - start.Y;
            double angle = Math.Atan2(dy, dx);
            double len = Math.Sqrt(dx * dx + dy * dy);

            // 量化到最近的 45°
            double snapped = Math.Round(angle / (Math.PI / 4)) * (Math.PI / 4);
            line.X2 = start.X + len * Math.Cos(snapped);
            line.Y2 = start.Y + len * Math.Sin(snapped);
        }
    }

    protected override bool IsValidSize(RoiShape shape)
    {
        if (shape is not LineRoi line) return false;
        double dx = line.X2 - line.X1;
        double dy = line.Y2 - line.Y1;
        return Math.Sqrt(dx * dx + dy * dy) > 3;
    }
}