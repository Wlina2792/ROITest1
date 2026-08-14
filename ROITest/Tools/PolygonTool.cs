using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Shapes;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 多边形绘制工具 —— 点击添加顶点，双击或 Enter 完成，Esc 取消
/// </summary>
public class PolygonTool : RoiTool
{
    private PolygonRoi? _currentPolygon;
    private bool _isDrawing;

    // 预览线（从最后一个顶点到鼠标当前位置）
    private Line? _previewLine;

    public override void OnMouseDown(Point pos, IEditorContext ctx)
    {
        if (!_isDrawing)
        {
            // 第一次点击：创建新多边形
            _currentPolygon = new PolygonRoi();
            _currentPolygon.Points.Add(pos);
            _isDrawing = true;

            // 加入集合（暂不记录撤销）
            ctx.Shapes.Add(_currentPolygon);

            // 创建预览线
            _previewLine = new Line
            {
                Stroke = System.Windows.Media.Brushes.Yellow,
                StrokeThickness = 1,
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 4, 2 }
            };
            _previewLine.X1 = pos.X;
            _previewLine.Y1 = pos.Y;
            _previewLine.X2 = pos.X;
            _previewLine.Y2 = pos.Y;

            ctx.RefreshOverlay();
        }
        else
        {
            // 后续点击：添加顶点
            _currentPolygon!.Points.Add(pos);

            // 更新预览线起点
            if (_previewLine != null)
            {
                _previewLine.X1 = pos.X;
                _previewLine.Y1 = pos.Y;
            }
        }
    }

    public override void OnMouseMove(Point pos, IEditorContext ctx)
    {
        if (!_isDrawing || _previewLine == null) return;

        // 更新预览线终点（跟随鼠标）
        _previewLine.X2 = pos.X;
        _previewLine.Y2 = pos.Y;

        // 确保预览线在覆盖层中
        // （通过 RefreshOverlay 会清除，所以这里不直接操作 OverlayCanvas）
    }

    public override void OnDoubleClick(Point pos, IEditorContext ctx)
    {
        if (!_isDrawing) return;

        FinishPolygon(ctx);
    }

    public override void OnConfirm(IEditorContext ctx)
    {
        if (!_isDrawing) return;

        FinishPolygon(ctx);
    }

    public override void OnCancel(IEditorContext ctx)
    {
        if (!_isDrawing || _currentPolygon == null) return;

        // 移除未完成的多边形
        ctx.Shapes.Remove(_currentPolygon);
        ResetState();
        ctx.RefreshOverlay();
    }

    private void FinishPolygon(IEditorContext ctx)
    {
        if (_currentPolygon == null) return;

        // 至少需要 3 个顶点才能构成多边形
        if (_currentPolygon.Points.Count < 3)
        {
            ctx.Shapes.Remove(_currentPolygon);
            ResetState();
            ctx.RefreshOverlay();
            return;
        }

        // 标记为已闭合
        _currentPolygon.IsClosed = true;

        // 记录撤销
        var polygon = _currentPolygon;
        ctx.ViewModel.ExecuteAction(
            doAction: () => { },  // 形状已在集合中
            undoAction: () => ctx.Shapes.Remove(polygon)
        );

        ResetState();
        ctx.RefreshOverlay();
    }

    private void ResetState()
    {
        _currentPolygon = null;
        _isDrawing = false;
        _previewLine = null;
    }
}