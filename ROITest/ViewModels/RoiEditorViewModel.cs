using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ROITest.Models;
using ROITest.Services;
using ROITest.Tools;

namespace ROITest.ViewModels;

/// <summary>
/// 可用工具类型枚举
/// </summary>
public enum RoiToolType
{
    Pointer,
    Rectangle,
    RotatedRectangle,
    Circle,
    Ellipse,
    Line,
    Polygon
}

/// <summary>
/// ROI 编辑器视图模型
/// </summary>
public partial class RoiEditorViewModel : ObservableObject
{
    private readonly UndoRedoService _undoRedo = new();
    private readonly Dictionary<RoiToolType, RoiTool> _tools;
    private RoiTool _activeToolInstance;

    public RoiEditorViewModel()
    {
        Shapes = new ObservableCollection<RoiShape>();

        // 初始化所有工具实例（延迟绑定 Context）
        _tools = new Dictionary<RoiToolType, RoiTool>
        {
            [RoiToolType.Pointer] = new PointerTool(),
            [RoiToolType.Rectangle] = new RectangleTool(),
            [RoiToolType.RotatedRectangle] = new RotatedRectangleTool(),
            [RoiToolType.Circle] = new CircleTool(),
            [RoiToolType.Ellipse] = new EllipseTool(),
            [RoiToolType.Line] = new LineTool(),
            [RoiToolType.Polygon] = new PolygonTool(),
        };

        // 默认激活指针工具
        _activeToolInstance = _tools[RoiToolType.Pointer];
    }

    // ─── 形状集合 ───

    /// <summary>所有 ROI 形状</summary>
    public ObservableCollection<RoiShape> Shapes { get; }

    // ─── 当前工具 ───

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveToolName))]
    private RoiToolType _activeTool = RoiToolType.Pointer;

    /// <summary>当前工具名称（用于状态栏显示）</summary>
    public string ActiveToolName => ActiveTool.ToString();

    partial void OnActiveToolChanged(RoiToolType value)
    {
        if (_tools.TryGetValue(value, out var tool))
        {
            _activeToolInstance = tool;
        }
    }

    // ─── 选中形状 ───

    /// <summary>当前选中的形状（单选时）</summary>
    [ObservableProperty]
    private RoiShape? _selectedShape;

    // ─── 命令 ───

    /// <summary>撤销</summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _undoRedo.Undo();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private bool CanUndo() => _undoRedo.CanUndo;

    /// <summary>重做</summary>
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undoRedo.Redo();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    private bool CanRedo() => _undoRedo.CanRedo;

    /// <summary>删除选中形状</summary>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete()
    {
        var toDelete = Shapes.Where(s => s.IsSelected).ToList();
        if (toDelete.Count == 0) return;

        // 记录撤销：恢复被删除的形状
        var snapshots = toDelete.Select(s => (shape: s, index: Shapes.IndexOf(s))).ToList();

        ExecuteAction(
            doAction: () =>
            {
                foreach (var s in toDelete)
                    Shapes.Remove(s);
                SelectedShape = null;
            },
            undoAction: () =>
            {
                // 按原索引倒序插入，避免索引偏移
                foreach (var item in snapshots.OrderByDescending(s => s.index))
                    Shapes.Insert(item.index, item.shape);
            }
        );
    }

    private bool CanDelete() => Shapes.Any(s => s.IsSelected);

    /// <summary>全选</summary>
    [RelayCommand]
    private void SelectAll()
    {
        foreach (var shape in Shapes)
            shape.IsSelected = true;
        DeleteCommand.NotifyCanExecuteChanged();
    }

    /// <summary>取消全选</summary>
    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var shape in Shapes)
            shape.IsSelected = false;
        SelectedShape = null;
        DeleteCommand.NotifyCanExecuteChanged();
    }

    // ─── 供 Tool 层调用的公共方法 ───

    /// <summary>
    /// 执行一个可撤销的操作
    /// </summary>
    /// <param name="doAction">正向操作</param>
    /// <param name="undoAction">撤销操作</param>
    public void ExecuteAction(Action doAction, Action undoAction)
    {
        doAction();
        _undoRedo.Record(doAction, undoAction);
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 添加形状到集合并记录撤销
    /// </summary>
    public void AddShape(RoiShape shape)
    {
        ExecuteAction(
            doAction: () => Shapes.Add(shape),
            undoAction: () => Shapes.Remove(shape)
        );
    }

    /// <summary>
    /// 从集合中移除形状并记录撤销
    /// </summary>
    public void RemoveShape(RoiShape shape)
    {
        int index = Shapes.IndexOf(shape);
        if (index < 0) return;

        ExecuteAction(
            doAction: () => Shapes.Remove(shape),
            undoAction: () => Shapes.Insert(index, shape)
        );
    }

    /// <summary>
    /// 获取当前激活的工具实例（供 EditorControl 转发鼠标事件）
    /// </summary>
    public RoiTool GetActiveTool() => _activeToolInstance;

    /// <summary>
    /// 通知外部命令状态已变化（由 EditorControl 在选中状态改变时调用）
    /// </summary>
    public void RefreshCommandStates()
    {
        DeleteCommand.NotifyCanExecuteChanged();
    }
}