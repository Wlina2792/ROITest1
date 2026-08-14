using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace ROITest.Models;

public partial class LineRoi : RoiShape
{
    public LineRoi() { }

    public LineRoi(double x1, double y1, double x2, double y2)
    {
        _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
    }

    [ObservableProperty] private double _x1;
    [ObservableProperty] private double _y1;
    [ObservableProperty] private double _x2;
    [ObservableProperty] private double _y2;

    /// <summary>命中容差（像素）</summary>
    private const double HitTolerance = 5.0;

    public override Geometry GetGeometry()
    {
        var geo = new LineGeometry(new Point(X1, Y1), new Point(X2, Y2));
        return geo;
    }

    public override bool HitTest(Point pt)
    {
        // 点到线段的距离
        double dx = X2 - X1;
        double dy = Y2 - Y1;
        double lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-9) return Distance(pt, new Point(X1, Y1)) <= HitTolerance;

        double t = Math.Max(0, Math.Min(1,
            ((pt.X - X1) * dx + (pt.Y - Y1) * dy) / lenSq));

        double projX = X1 + t * dx;
        double projY = Y1 + t * dy;
        return Distance(pt, new Point(projX, projY)) <= HitTolerance;
    }

    public override bool HitTest(Rect rect)
    {
        return rect.Contains(new Point(X1, Y1)) || rect.Contains(new Point(X2, Y2));
    }

    public override Point[] GetHandles()
    {
        return new[]
        {
            new Point(X1, Y1),  // 起点
            new Point(X2, Y2),  // 终点
        };
    }

    public override RoiShape Clone()
    {
        return new LineRoi(X1, Y1, X2, Y2)
        {
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
    }

    private static double Distance(Point a, Point b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}