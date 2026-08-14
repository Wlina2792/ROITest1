using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ROITest.Models;

public partial class EllipseRoi : RoiShape
{
    public EllipseRoi() { }

    public EllipseRoi(double centerX, double centerY, double radiusX, double radiusY)
    {
        _centerX = centerX; _centerY = centerY;
        _radiusX = radiusX; _radiusY = radiusY;
    }

    [ObservableProperty] private double _centerX;
    [ObservableProperty] private double _centerY;
    [ObservableProperty] private double _radiusX;
    [ObservableProperty] private double _radiusY;

    public override Geometry GetGeometry()
        => new EllipseGeometry(new Point(CenterX, CenterY), RadiusX, RadiusY);

    public override bool HitTest(Point pt)
    {
        double dx = (pt.X - CenterX) / RadiusX;
        double dy = (pt.Y - CenterY) / RadiusY;
        return dx * dx + dy * dy <= 1.0;
    }

    public override bool HitTest(Rect rect)
    {
        return rect.Contains(new Point(CenterX, CenterY));
    }

    public override Point[] GetHandles()
    {
        return new[]
        {
            new Point(CenterX + RadiusX, CenterY),   // 右
            new Point(CenterX, CenterY - RadiusY),   // 上
            new Point(CenterX - RadiusX, CenterY),   // 左
            new Point(CenterX, CenterY + RadiusY),   // 下
        };
    }

    public override RoiShape Clone()
    {
        return new EllipseRoi(CenterX, CenterY, RadiusX, RadiusY)
        {
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
    }
}