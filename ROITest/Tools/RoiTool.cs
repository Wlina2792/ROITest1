using System.Windows;

namespace ROITest.Tools;

/// <summary>
/// 工具基类 —— 所有交互工具的抽象父类
/// </summary>
public abstract class RoiTool
{
    /// <summary>鼠标按下</summary>
    public virtual void OnMouseDown(Point pos, IEditorContext ctx) { }

    /// <summary>鼠标移动（按下状态）</summary>
    public virtual void OnMouseMove(Point pos, IEditorContext ctx) { }

    /// <summary>鼠标释放</summary>
    public virtual void OnMouseUp(Point pos, IEditorContext ctx) { }

    /// <summary>鼠标悬停（未按下的移动）</summary>
    public virtual void OnMouseHover(Point pos, IEditorContext ctx) { }

    /// <summary>鼠标双击</summary>
    public virtual void OnDoubleClick(Point pos, IEditorContext ctx) { }

    /// <summary>取消操作（Esc 键）</summary>
    public virtual void OnCancel(IEditorContext ctx) { }

    /// <summary>确认操作（Enter 键）</summary>
    public virtual void OnConfirm(IEditorContext ctx) { }
}