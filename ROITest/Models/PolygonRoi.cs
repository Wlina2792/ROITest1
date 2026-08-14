using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace ROITest.Models;

public partial class PolygonRoi : RoiShape
{
    public PolygonRoi()
    {
        Points = new ObservableCollection<Point>();
    }

    public PolygonRoi(IEnumerable<Point> points) : this()
    {
        foreach (var p in points)
            Points.Add(p);
    }

    /// <summary>顶点集合</summary>
    public ObservableCollection<Point> Points { get; }

    /// <summary>是否已闭合（双击或 Enter 完成绘制）</summary>
    [ObservableProperty]
    private bool _isClosed;

    public override Geometry GetGeometry()
    {
        if (Points.Count < 2)
            return Geometry.Empty;

        var figure = new PathFigure { StartPoint = Points[0], IsClosed = IsClosed };
        for (int i = 1; i < Points.Count; i++)
        {
            figure.Segments.Add(new LineSegment(Points[i], true));
        }
        return new PathGeometry(new[] { figure });
    }

    public override bool HitTest(Point pt)
    {
        if (Points.Count < 3 || !IsClosed)
            return false;

        var geo = GetGeometry();
        return geo.FillContains(pt);
    }

    public override bool HitTest(Rect rect)
    {
        if (Points.Count == 0) return false;
        // 任一顶点在框选区域内即视为命中
        return Points.Any(p => rect.Contains(p));
    }

    public override Point[] GetHandles()
    {
        return Points.ToArray();
    }

    public override RoiShape Clone()
    {
        var clone = new PolygonRoi(Points)
        {
            IsClosed = IsClosed,
            IsSelected = IsSelected,
            Stroke = Stroke,
            StrokeThickness = StrokeThickness,
            Fill = Fill
        };
        return clone;
    }
}