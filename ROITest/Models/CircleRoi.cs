using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace ROITest.Models;

public partial class CircleRoi : RoiShape
{
    public CircleRoi() { }

    public CircleRoi(double centerX, double centerY, double radius)
    {
        _centerX = centerX; _centerY = centerY; _radius = radius;
    }

    [ObservableProperty] private double _centerX;
    [ObservableProperty] private double _centerY;
    [ObservableProperty] private double _radius;

    public override Geometry GetGeometry()
        => new EllipseGeometry(new Point(CenterX, CenterY), Radius, Radius);

    public override bool HitTest(Point pt)
    {
        double dx = pt.X - CenterX;
        double dy = pt.Y - CenterY;
        return dx * dx + dy * dy <= Radius * Radius;
    }

    public override bool HitTest(Rect rect)
    {
        return rect.Contains(new Point(CenterX, CenterY));
    }

    public override Point[] GetHandles()
    {
        return new[]
        {
            new Point(CenterX + Radius, CenterY),  // 右
            new Point(CenterX, CenterY - Radius),  // 上
            new Point(CenterX - Radius, CenterY),  // 左
            new Point(CenterX, CenterY + Radius),  // 下
        };
    }

    public override RoiShape Clone()
    {
        return new CircleRoi(CenterX, CenterY, Radius)
        {
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
    }
}