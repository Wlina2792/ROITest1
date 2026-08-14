using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ROITest.Models;

public partial class RotatedRectangleRoi : RoiShape
{
    public RotatedRectangleRoi() { }

    public RotatedRectangleRoi(double x, double y, double width, double height, double angle)
    {
        _x = x; _y = y; _width = width; _height = height; _angle = angle;
    }

    [ObservableProperty] private double _x;
    [ObservableProperty] private double _y;
    [ObservableProperty] private double _width;
    [ObservableProperty] private double _height;
    [ObservableProperty] private double _angle; // 旋转角度（度）

    /// <summary>获取旋转后的四个顶点</summary>
    public Point[] GetCorners()
    {
        double cx = X + Width / 2;
        double cy = Y + Height / 2;
        double rad = Angle * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        var localCorners = new[]
        {
            new Point(-Width / 2, -Height / 2),
            new Point( Width / 2, -Height / 2),
            new Point( Width / 2,  Height / 2),
            new Point(-Width / 2,  Height / 2),
        };

        return localCorners.Select(p =>
            new Point(cx + p.X * cos - p.Y * sin,
                      cy + p.X * sin + p.Y * cos)).ToArray();
    }

    public override Geometry GetGeometry()
    {
        var corners = GetCorners();
        var figure = new PathFigure { StartPoint = corners[0], IsClosed = true };
        figure.Segments.Add(new LineSegment(corners[1], true));
        figure.Segments.Add(new LineSegment(corners[2], true));
        figure.Segments.Add(new LineSegment(corners[3], true));
        var geo = new PathGeometry(new[] { figure });
        return geo;
    }

    public override bool HitTest(Point pt)
    {
        var geo = GetGeometry();
        return geo.FillContains(pt);
    }

    public override bool HitTest(Rect rect)
    {
        var geo = GetGeometry();
        return geo.FillContains(rect.GetCenter());
    }

    public override Point[] GetHandles()
    {
        return GetCorners();
    }

    public override RoiShape Clone()
    {
        return new RotatedRectangleRoi(X, Y, Width, Height, Angle)
        {
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
    }
}

internal static class RectExtensions
{
    public static Point GetCenter(this Rect r)
        => new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
}