using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 旋转矩形绘制工具 —— 按下起点，拖动确定宽高，释放后角度为 0
/// 后续可通过 PointerTool 的旋转手柄调整角度
/// </summary>
public class RotatedRectangleTool : DrawingToolBase
{
    protected override RoiShape CreateShape(Point startPos)
    {
        return new RotatedRectangleRoi(startPos.X, startPos.Y, 0, 0, 0);
    }

    protected override void UpdateShape(RoiShape shape, Point start, Point current)
    {
        if (shape is not RotatedRectangleRoi rrect) return;

        double x = Math.Min(start.X, current.X);
        double y = Math.Min(start.Y, current.Y);
        double w = Math.Abs(current.X - start.X);
        double h = Math.Abs(current.Y - start.Y);

        rrect.X = x;
        rrect.Y = y;
        rrect.Width = w;
        rrect.Height = h;
    }
}