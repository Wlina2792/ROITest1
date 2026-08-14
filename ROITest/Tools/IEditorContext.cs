using System.Collections.ObjectModel;
using System.Windows;
using ROITest.Models;
using ROITest.ViewModels;

namespace ROITest.Tools;

/// <summary>
/// 编辑器上下文接口 —— 解耦 Tool 层与 EditorControl
/// </summary>
public interface IEditorContext
{
    /// <summary>形状集合</summary>
    ObservableCollection<RoiShape> Shapes { get; }

    /// <summary>编辑器 ViewModel</summary>
    RoiEditorViewModel ViewModel { get; }

    /// <summary>刷新覆盖层（手柄、框选矩形）</summary>
    void RefreshOverlay();

    /// <summary>设置框选矩形（null 表示清除）</summary>
    void SetSelectionRect(Rect? rect);

    /// <summary>控件实际尺寸</summary>
    Size ControlSize { get; }
    /// <summary>刷新命令状态</summary>
    void RefreshCommandStates();
}