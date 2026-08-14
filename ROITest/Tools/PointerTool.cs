using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 指针工具 —— 选中 / 移动 / 缩放 / 旋转 / 框选
/// </summary>
public class PointerTool : RoiTool
{
    private enum DragMode
    {
        None,
        Move,           // 移动形状
        Resize,         // 缩放手柄
        Rotate,         // 旋转手柄（旋转矩形）
        Marquee         // 框选
    }

    private DragMode _mode = DragMode.None;
    private Point _startPos;
    private RoiShape? _draggedShape;
    private int _handleIndex = -1;

    // 拖拽前的快照（用于撤销）
    private Dictionary<int, ShapeSnapshot>? _snapshots;

    private const double HandleHitRadius = 6.0;

    public override void OnMouseDown(Point pos, IEditorContext ctx)
    {
        _startPos = pos;
        _mode = DragMode.None;
        _draggedShape = null;
        _handleIndex = -1;
        _snapshots = null;

        // 1. 优先检测是否点击了选中形状的手柄
        foreach (var shape in ctx.Shapes.Where(s => s.IsSelected))
        {
            var handles = shape.GetHandles();
            for (int i = 0; i < handles.Length; i++)
            {
                if (Distance(pos, handles[i]) <= HandleHitRadius)
                {
                    _draggedShape = shape;
                    _handleIndex = i;
                    _mode = shape is RotatedRectangleRoi && i == handles.Length - 1
                        ? DragMode.Rotate
                        : DragMode.Resize;

                    CaptureSnapshots(ctx);
                    return;
                }
            }
        }

        // 2. 检测是否点击了某个形状
        for (int i = ctx.Shapes.Count - 1; i >= 0; i--)
        {
            var shape = ctx.Shapes[i];
            if (shape.HitTest(pos))
            {
                // 如果点击了未选中的形状，切换选中
                if (!shape.IsSelected)
                {
                    // 取消其他选中（除非按住 Shift）
                    if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(
                        System.Windows.Input.ModifierKeys.Shift))
                    {
                        foreach (var s in ctx.Shapes)
                            s.IsSelected = false;
                    }
                    shape.IsSelected = true;
                }

                _draggedShape = shape;
                _mode = DragMode.Move;
                CaptureSnapshots(ctx);
                ctx.RefreshCommandStates();
                ctx.RefreshOverlay();
                return;
            }
        }

        // 3. 点击空白区域 → 取消选中 + 开始框选
        if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(
            System.Windows.Input.ModifierKeys.Shift))
        {
            foreach (var s in ctx.Shapes)
                s.IsSelected = false;
            ctx.RefreshCommandStates();
        }

        _mode = DragMode.Marquee;
        ctx.RefreshOverlay();
    }

    public override void OnMouseMove(Point pos, IEditorContext ctx)
    {
        switch (_mode)
        {
            case DragMode.Move:
                DoMove(pos, ctx);
                break;

            case DragMode.Resize:
                DoResize(pos, ctx);
                break;

            case DragMode.Rotate:
                DoRotate(pos, ctx);
                break;

            case DragMode.Marquee:
                DoMarquee(pos, ctx);
                break;
        }
    }

    public override void OnMouseUp(Point pos, IEditorContext ctx)
    {
        if (_mode == DragMode.Marquee)
        {
            // 完成框选
            DoMarqueeFinalize(pos, ctx);
            ctx.SetSelectionRect(null);
        }
        else if (_mode != DragMode.None && _snapshots != null)
        {
            // 记录撤销
            CommitDrag(ctx);
        }

        _mode = DragMode.None;
        _draggedShape = null;
        _handleIndex = -1;
        _snapshots = null;
        ctx.RefreshOverlay();
    }

    // ─── 移动 ───

    private void DoMove(Point pos, IEditorContext ctx)
    {
        double dx = pos.X - _startPos.X;
        double dy = pos.Y - _startPos.Y;

        // 移动所有选中的形状
        foreach (var shape in ctx.Shapes.Where(s => s.IsSelected))
        {
            if (!_snapshots!.TryGetValue(shape.Id, out var snap)) continue;

            switch (shape)
            {
                case RectangleRoi rect:
                    rect.X = snap.X + dx;
                    rect.Y = snap.Y + dy;
                    break;

                case RotatedRectangleRoi rrect:
                    rrect.X = snap.X + dx;
                    rrect.Y = snap.Y + dy;
                    break;

                case CircleRoi circle:
                    circle.CenterX = snap.CenterX + dx;
                    circle.CenterY = snap.CenterY + dy;
                    break;

                case EllipseRoi ellipse:
                    ellipse.CenterX = snap.CenterX + dx;
                    ellipse.CenterY = snap.CenterY + dy;
                    break;

                case LineRoi line:
                    line.X1 = snap.X1 + dx;
                    line.Y1 = snap.Y1 + dy;
                    line.X2 = snap.X2 + dx;
                    line.Y2 = snap.Y2 + dy;
                    break;

                case PolygonRoi polygon:
                    for (int i = 0; i < polygon.Points.Count; i++)
                    {
                        var original = snap.PolygonPoints![i];
                        polygon.Points[i] = new Point(original.X + dx, original.Y + dy);
                    }
                    break;
            }
        }
    }

    // ─── 缩放 ───

    private void DoResize(Point pos, IEditorContext ctx)
    {
        if (_draggedShape == null || _handleIndex < 0) return;
        if (!_snapshots!.TryGetValue(_draggedShape.Id, out var snap)) return;

        switch (_draggedShape)
        {
            case RectangleRoi rect:
                ResizeRectangle(rect, snap, pos);
                break;

            case RotatedRectangleRoi rrect:
                ResizeRotatedRect(rrect, snap, pos);
                break;

            case CircleRoi circle:
                ResizeCircle(circle, snap, pos);
                break;

            case EllipseRoi ellipse:
                ResizeEllipse(ellipse, snap, pos);
                break;

            case LineRoi line:
                ResizeLine(line, snap, pos);
                break;
        }
    }

    private void ResizeRectangle(RectangleRoi rect, ShapeSnapshot snap, Point pos)
    {
        double left = snap.X;
        double top = snap.Y;
        double right = snap.X + snap.Width;
        double bottom = snap.Y + snap.Height;

        ApplyHandleConstraint(_handleIndex, ref left, ref top, ref right, ref bottom, pos);

        rect.X = left;
        rect.Y = top;
        rect.Width = Math.Max(1, right - left);
        rect.Height = Math.Max(1, bottom - top);
    }

    private void ResizeRotatedRect(RotatedRectangleRoi rrect, ShapeSnapshot snap, Point pos)
    {
        // 简化处理：在局部坐标系中缩放
        double cx = snap.X + snap.Width / 2;
        double cy = snap.Y + snap.Height / 2;
        double rad = snap.Angle * Math.PI / 180.0;
        double cos = Math.Cos(-rad);
        double sin = Math.Sin(-rad);

        // 将鼠标位置转换到局部坐标
        double lx = (pos.X - cx) * cos - (pos.Y - cy) * sin + snap.Width / 2;
        double ly = (pos.X - cx) * sin + (pos.Y - cy) * cos + snap.Height / 2;

        rrect.Width = Math.Max(10, lx * 2);
        rrect.Height = Math.Max(10, ly * 2);
    }

    private void ResizeCircle(CircleRoi circle, ShapeSnapshot snap, Point pos)
    {
        double dx = pos.X - snap.CenterX;
        double dy = pos.Y - snap.CenterY;
        circle.Radius = Math.Max(5, Math.Sqrt(dx * dx + dy * dy));
    }

    private void ResizeEllipse(EllipseRoi ellipse, ShapeSnapshot snap, Point pos)
    {
        double dx = Math.Abs(pos.X - snap.CenterX);
        double dy = Math.Abs(pos.Y - snap.CenterY);
        ellipse.RadiusX = Math.Max(5, dx);
        ellipse.RadiusY = Math.Max(5, dy);
    }

    private void ResizeLine(LineRoi line, ShapeSnapshot snap, Point pos)
    {
        if (_handleIndex == 0)
        {
            line.X1 = pos.X;
            line.Y1 = pos.Y;
        }
        else
        {
            line.X2 = pos.X;
            line.Y2 = pos.Y;
        }
    }

    // ─── 旋转 ───

    private void DoRotate(Point pos, IEditorContext ctx)
    {
        if (_draggedShape is not RotatedRectangleRoi rrect) return;
        if (!_snapshots!.TryGetValue(rrect.Id, out var snap)) return;

        double cx = snap.X + snap.Width / 2;
        double cy = snap.Y + snap.Height / 2;

        double angle = Math.Atan2(pos.Y - cy, pos.X - cx) * 180.0 / Math.PI;
        rrect.Angle = angle;
    }

    // ─── 框选 ───

    private void DoMarquee(Point pos, IEditorContext ctx)
    {
        double x = Math.Min(_startPos.X, pos.X);
        double y = Math.Min(_startPos.Y, pos.Y);
        double w = Math.Abs(pos.X - _startPos.X);
        double h = Math.Abs(pos.Y - _startPos.Y);

        ctx.SetSelectionRect(new Rect(x, y, w, h));
    }

    private void DoMarqueeFinalize(Point pos, IEditorContext ctx)
    {
        double x = Math.Min(_startPos.X, pos.X);
        double y = Math.Min(_startPos.Y, pos.Y);
        double w = Math.Abs(pos.X - _startPos.X);
        double h = Math.Abs(pos.Y - _startPos.Y);

        if (w < 3 && h < 3) return;

        var marqueeRect = new Rect(x, y, w, h);
        bool isShift = System.Windows.Input.Keyboard.Modifiers.HasFlag(
            System.Windows.Input.ModifierKeys.Shift);

        foreach (var shape in ctx.Shapes)
        {
            if (shape.HitTest(marqueeRect))
            {
                shape.IsSelected = true;
            }
            else if (!isShift)
            {
                shape.IsSelected = false;
            }
        }

        ctx.RefreshCommandStates();
    }

    // ─── 辅助方法 ───

    private void CaptureSnapshots(IEditorContext ctx)
    {
        _snapshots = new Dictionary<int, ShapeSnapshot>();
        foreach (var shape in ctx.Shapes.Where(s => s.IsSelected))
        {
            _snapshots[shape.Id] = ShapeSnapshot.Capture(shape);
        }
    }

    private void CommitDrag(IEditorContext ctx)
    {
        if (_snapshots == null || _snapshots.Count == 0) return;

        // 检查是否真的有变化
        bool hasChange = false;
        foreach (var kvp in _snapshots)
        {
            var shape = ctx.Shapes.FirstOrDefault(s => s.Id == kvp.Key);
            if (shape != null && !kvp.Value.Equals(shape))
            {
                hasChange = true;
                break;
            }
        }

        if (!hasChange) return;

        var snapshots = _snapshots;
        ctx.ViewModel.ExecuteAction(
            doAction: () => { },  // 变化已发生
            undoAction: () =>
            {
                foreach (var kvp in snapshots)
                {
                    var shape = ctx.Shapes.FirstOrDefault(s => s.Id == kvp.Key);
                    if (shape != null)
                        kvp.Value.Restore(shape);
                }
            }
        );
    }

    private static void ApplyHandleConstraint(int handleIndex,
        ref double left, ref double top, ref double right, ref double bottom, Point pos)
    {
        switch (handleIndex)
        {
            case 0: // 左上
                left = pos.X; top = pos.Y; break;
            case 1: // 上中
                top = pos.Y; break;
            case 2: // 右上
                right = pos.X; top = pos.Y; break;
            case 3: // 右中
                right = pos.X; break;
            case 4: // 右下
                right = pos.X; bottom = pos.Y; break;
            case 5: // 下中
                bottom = pos.Y; break;
            case 6: // 左下
                left = pos.X; bottom = pos.Y; break;
            case 7: // 左中
                left = pos.X; break;
        }
    }

    private static double Distance(Point a, Point b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

/// <summary>
/// 形状状态快照（用于撤销/恢复拖拽操作）
/// </summary>
internal class ShapeSnapshot
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Radius { get; set; }
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public double Angle { get; set; }
    public Point[]? PolygonPoints { get; set; }

    public static ShapeSnapshot Capture(RoiShape shape)
    {
        var snap = new ShapeSnapshot();

        switch (shape)
        {
            case RectangleRoi rect:
                snap.X = rect.X; snap.Y = rect.Y;
                snap.Width = rect.Width; snap.Height = rect.Height;
                break;

            case RotatedRectangleRoi rrect:
                snap.X = rrect.X; snap.Y = rrect.Y;
                snap.Width = rrect.Width; snap.Height = rrect.Height;
                snap.Angle = rrect.Angle;
                break;

            case CircleRoi circle:
                snap.CenterX = circle.CenterX; snap.CenterY = circle.CenterY;
                snap.Radius = circle.Radius;
                break;

            case EllipseRoi ellipse:
                snap.CenterX = ellipse.CenterX; snap.CenterY = ellipse.CenterY;
                snap.RadiusX = ellipse.RadiusX; snap.RadiusY = ellipse.RadiusY;
                break;

            case LineRoi line:
                snap.X1 = line.X1; snap.Y1 = line.Y1;
                snap.X2 = line.X2; snap.Y2 = line.Y2;
                break;

            case PolygonRoi polygon:
                snap.PolygonPoints = polygon.Points.ToArray();
                break;
        }

        return snap;
    }

    public void Restore(RoiShape shape)
    {
        switch (shape)
        {
            case RectangleRoi rect:
                rect.X = X; rect.Y = Y;
                rect.Width = Width; rect.Height = Height;
                break;

            case RotatedRectangleRoi rrect:
                rrect.X = X; rrect.Y = Y;
                rrect.Width = Width; rrect.Height = Height;
                rrect.Angle = Angle;
                break;

            case CircleRoi circle:
                circle.CenterX = CenterX; circle.CenterY = CenterY;
                circle.Radius = Radius;
                break;

            case EllipseRoi ellipse:
                ellipse.CenterX = CenterX; ellipse.CenterY = CenterY;
                ellipse.RadiusX = RadiusX; ellipse.RadiusY = RadiusY;
                break;

            case LineRoi line:
                line.X1 = X1; line.Y1 = Y1;
                line.X2 = X2; line.Y2 = Y2;
                break;

            case PolygonRoi polygon:
                if (PolygonPoints != null)
                {
                    polygon.Points.Clear();
                    foreach (var p in PolygonPoints)
                        polygon.Points.Add(p);
                }
                break;
        }
    }

    public bool Equals(RoiShape shape)
    {
        switch (shape)
        {
            case RectangleRoi rect:
                return rect.X == X && rect.Y == Y &&
                       rect.Width == Width && rect.Height == Height;

            case RotatedRectangleRoi rrect:
                return rrect.X == X && rrect.Y == Y &&
                       rrect.Width == Width && rrect.Height == Height &&
                       rrect.Angle == Angle;

            case CircleRoi circle:
                return circle.CenterX == CenterX && circle.CenterY == CenterY &&
                       circle.Radius == Radius;

            case EllipseRoi ellipse:
                return ellipse.CenterX == CenterX && ellipse.CenterY == CenterY &&
                       ellipse.RadiusX == RadiusX && ellipse.RadiusY == RadiusY;

            case LineRoi line:
                return line.X1 == X1 && line.Y1 == Y1 &&
                       line.X2 == X2 && line.Y2 == Y2;

            case PolygonRoi polygon:
                if (PolygonPoints == null || polygon.Points.Count != PolygonPoints.Length)
                    return false;
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    if (polygon.Points[i] != PolygonPoints[i])
                        return false;
                }
                return true;

            default:
                return true;
        }
    }
}