using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 椭圆绘制工具 —— 按下确定圆心，拖动确定 X/Y 半径
/// </summary>
public class EllipseTool : DrawingToolBase
{
    protected override RoiShape CreateShape(Point startPos)
    {
        return new EllipseRoi(startPos.X, startPos.Y, 0, 0);
    }

    protected override void UpdateShape(RoiShape shape, Point start, Point current)
    {
        if (shape is not EllipseRoi ellipse) return;

        ellipse.RadiusX = Math.Abs(current.X - start.X);
        ellipse.RadiusY = Math.Abs(current.Y - start.Y);
    }

    protected override bool IsValidSize(RoiShape shape)
    {
        return shape is EllipseRoi e && (e.RadiusX > 3 || e.RadiusY > 3);
    }
}