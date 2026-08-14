using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 圆形绘制工具 —— 按下确定圆心，拖动确定半径
/// </summary>
public class CircleTool : DrawingToolBase
{
    protected override RoiShape CreateShape(Point startPos)
    {
        return new CircleRoi(startPos.X, startPos.Y, 0);
    }

    protected override void UpdateShape(RoiShape shape, Point start, Point current)
    {
        if (shape is not CircleRoi circle) return;

        double dx = current.X - start.X;
        double dy = current.Y - start.Y;
        circle.Radius = Math.Sqrt(dx * dx + dy * dy);
    }

    protected override bool IsValidSize(RoiShape shape)
    {
        return shape is CircleRoi c && c.Radius > 3;
    }
}