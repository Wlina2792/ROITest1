using System.Windows;
using ROITest.Models;

namespace ROITest.Tools;

/// <summary>
/// 绘制工具通用基类 —— 按下创建 → 拖动调整 → 抬起完成
/// </summary>
public abstract class DrawingToolBase : RoiTool
{
    private Point _startPos;
    private RoiShape? _currentShape;
    private bool _isDrawing;

    public override void OnMouseDown(Point pos, IEditorContext ctx)
    {
        _startPos = pos;
        _currentShape = CreateShape(pos);
        _isDrawing = true;

        // 先加入集合（不通过 AddShape，因为还没完成绘制，不应记录撤销）
        ctx.Shapes.Add(_currentShape);
        ctx.RefreshOverlay();
    }

    public override void OnMouseMove(Point pos, IEditorContext ctx)
    {
        if (!_isDrawing || _currentShape == null) return;

        UpdateShape(_currentShape, _startPos, pos);
    }

    public override void OnMouseUp(Point pos, IEditorContext ctx)
    {
        if (!_isDrawing || _currentShape == null) return;

        _isDrawing = false;

        // 检查尺寸是否过小（误点击），过小则移除
        if (!IsValidSize(_currentShape))
        {
            ctx.Shapes.Remove(_currentShape);
            _currentShape = null;
            ctx.RefreshOverlay();
            return;
        }

        // 记录撤销操作（将已添加的形状包装进撤销栈）
        var shape = _currentShape;
        ctx.ViewModel.ExecuteAction(
            doAction: () => { },  // 形状已在集合中，无需再添加
            undoAction: () => ctx.Shapes.Remove(shape)
        );

        _currentShape = null;
        ctx.RefreshOverlay();
    }

    public override void OnCancel(IEditorContext ctx)
    {
        if (_isDrawing && _currentShape != null)
        {
            ctx.Shapes.Remove(_currentShape);
            _currentShape = null;
            _isDrawing = false;
            ctx.RefreshOverlay();
        }
    }

    // ─── 子类实现 ───

    /// <summary>在指定位置创建新的形状实例</summary>
    protected abstract RoiShape CreateShape(Point startPos);

    /// <summary>根据起点和当前点更新形状尺寸</summary>
    protected abstract void UpdateShape(RoiShape shape, Point start, Point current);

    /// <summary>判断形状尺寸是否有效（防止误点击创建零尺寸形状）</summary>
    protected virtual bool IsValidSize(RoiShape shape)
    {
        var geo = shape.GetGeometry();
        var bounds = geo.Bounds;
        return bounds.Width > 3 || bounds.Height > 3;
    }
}