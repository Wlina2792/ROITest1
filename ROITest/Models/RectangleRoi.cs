using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ROITest.Models;

public partial class RectangleRoi : RoiShape
{
    public RectangleRoi() { }

    public RectangleRoi(double x, double y, double width, double height)
    {
        _x = x; _y = y; _width = width; _height = height;
    }

    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;

    public override Geometry GetGeometry()
        => new RectangleGeometry(new Rect(X, Y, Width, Height));

    public override bool HitTest(Point pt)
        => new Rect(X, Y, Width, Height).Contains(pt);

    public override bool HitTest(Rect rect)
    {
        var r = new Rect(X, Y, Width, Height);
        return rect.IntersectsWith(r) || rect.Contains(r);
    }

    public override Point[] GetHandles()
    {
        return new[]
        {
            new Point(X, Y),                         // 左上
            new Point(X + Width / 2, Y),             // 上中
            new Point(X + Width, Y),                 // 右上
            new Point(X + Width, Y + Height / 2),    // 右中
            new Point(X + Width, Y + Height),        // 右下
            new Point(X + Width / 2, Y + Height),    // 下中
            new Point(X, Y + Height),                // 左下
            new Point(X, Y + Height / 2),            // 左中
        };
    }

    public override RoiShape Clone()
    {
        return new RectangleRoi(X, Y, Width, Height)
        {
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
    }
}