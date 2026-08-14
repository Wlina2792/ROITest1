using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ROITest.Models;
using ROITest.Tools;
using ROITest.ViewModels;

namespace ROITest.Views;

/// <summary>
/// ROI 编辑器控件 —— 负责渲染同步、鼠标事件转发
/// </summary>
public partial class RoiEditorControl : UserControl, IEditorContext
{
    private RoiEditorViewModel? _vm;
    private bool _isLeftButtonDown;

    // 手柄尺寸
    private const double HandleSize = 8;
    private const double HandleHalfSize = HandleSize / 2;

    // 框选矩形（指针工具拖动时显示）
    private Rectangle? _selectionRect;

    public RoiEditorControl()
    {
        InitializeComponent();

        // 鼠标事件绑定到 OverlayCanvas（最上层，接收所有交互）
        OverlayCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
        OverlayCanvas.MouseMove += OnMouseMove;
        OverlayCanvas.MouseLeftButtonUp += OnMouseLeftButtonUp;
        OverlayCanvas.AddHandler(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(OnMouseDoubleClick), true);
        // 键盘事件
        KeyDown += OnKeyDown;
        Focusable = true;
        Loaded += (_, _) => Focus();
    }

    // ─── 绑定 ViewModel ───

    /// <summary>
    /// 绑定编辑器视图模型（由父窗体在初始化时调用）
    /// </summary>
    public void BindViewModel(RoiEditorViewModel vm)
    {
        // 解绑旧的
        if (_vm != null)
        {
            _vm.Shapes.CollectionChanged -= OnShapesCollectionChanged;
            foreach (var shape in _vm.Shapes)
                shape.PropertyChanged -= OnShapePropertyChanged;
        }

        _vm = vm;

        // 绑定新的
        _vm.Shapes.CollectionChanged += OnShapesCollectionChanged;
        foreach (var shape in _vm.Shapes)
            shape.PropertyChanged += OnShapePropertyChanged;

        // 初始化已有形状
        ShapeCanvas.Children.Clear();
        foreach (var shape in _vm.Shapes)
            AddShapeVisual(shape);
    }

    // ─── 底图设置 ───

    /// <summary>设置底图</summary>
    public void SetImage(ImageSource? source)
    {
        BackgroundImage.Source = source;
    }

    // ─── 集合变更：增删形状时同步 Canvas ───

    private void OnShapesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (RoiShape shape in e.OldItems)
            {
                shape.PropertyChanged -= OnShapePropertyChanged;
                RemoveShapeVisual(shape);
            }
        }

        if (e.NewItems != null)
        {
            foreach (RoiShape shape in e.NewItems)
            {
                shape.PropertyChanged += OnShapePropertyChanged;
                AddShapeVisual(shape);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            ShapeCanvas.Children.Clear();
        }
    }

    // ─── 属性变更：形状属性改变时更新对应 Path ───

    private void OnShapePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not RoiShape shape) return;

        // 找到对应的 Path 并更新
        foreach (var child in ShapeCanvas.Children)
        {
            if (child is Path path && path.Tag == shape)
            {
                path.Data = shape.GetGeometry();
                path.Stroke = shape.IsSelected ? Brushes.Yellow : shape.Stroke;
                path.StrokeThickness = shape.IsSelected ? 2.0 : shape.StrokeThickness;
                break;
            }
        }

        // 选中状态变化时刷新手柄
        if (e.PropertyName == nameof(RoiShape.IsSelected))
        {
            UpdateOverlay();
            _vm?.RefreshCommandStates();
        }
    }

    // ─── 添加/移除形状的可视化元素 ───

    private void AddShapeVisual(RoiShape shape)
    {
        var path = new Path
        {
            Data = shape.GetGeometry(),
            Stroke = shape.IsSelected ? Brushes.Yellow : shape.Stroke,
            StrokeThickness = shape.IsSelected ? 2.0 : shape.StrokeThickness,
            Fill = shape.Fill,
            Tag = shape,
            SnapsToDevicePixels = true
        };
        ShapeCanvas.Children.Add(path);
    }

    private void RemoveShapeVisual(RoiShape shape)
    {
        for (int i = ShapeCanvas.Children.Count - 1; i >= 0; i--)
        {
            if (ShapeCanvas.Children[i] is Path path && path.Tag == shape)
            {
                ShapeCanvas.Children.RemoveAt(i);
                break;
            }
        }
    }

    // ─── 覆盖层更新（手柄 + 框选矩形） ───

    private void UpdateOverlay()
    {
        OverlayCanvas.Children.Clear();

        if (_vm == null) return;

        // 绘制选中形状的手柄
        foreach (var shape in _vm.Shapes.Where(s => s.IsSelected))
        {
            var handles = shape.GetHandles();
            foreach (var handle in handles)
            {
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = HandleSize,
                    Height = HandleSize,
                    Fill = Brushes.White,
                    Stroke = Brushes.DodgerBlue,
                    StrokeThickness = 1,
                    SnapsToDevicePixels = true
                };
                Canvas.SetLeft(rect, handle.X - HandleHalfSize);
                Canvas.SetTop(rect, handle.Y - HandleHalfSize);
                OverlayCanvas.Children.Add(rect);
            }
        }

        // 保留框选矩形（如果正在框选）
        if (_selectionRect != null)
        {
            OverlayCanvas.Children.Add(_selectionRect);
        }
    }

    // ─── 鼠标事件转发 ───

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null) return;

        _isLeftButtonDown = true;
        Focus();

        var pos = e.GetPosition(OverlayCanvas);
        var tool = _vm.GetActiveTool();
        tool.OnMouseDown(pos, this);

        // 捕获鼠标
        OverlayCanvas.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_vm == null) return;

        var pos = e.GetPosition(OverlayCanvas);
        var tool = _vm.GetActiveTool();

        if (_isLeftButtonDown)
        {
            tool.OnMouseMove(pos, this);
        }
        else
        {
            tool.OnMouseHover(pos, this);
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null) return;

        _isLeftButtonDown = false;
        var pos = e.GetPosition(OverlayCanvas);
        var tool = _vm.GetActiveTool();
        tool.OnMouseUp(pos, this);

        OverlayCanvas.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null) return;

        var pos = e.GetPosition(OverlayCanvas);
        var tool = _vm.GetActiveTool();
        tool.OnDoubleClick(pos, this);
        e.Handled = true;
    }

    // ─── 键盘事件 ───

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (_vm == null) return;

        switch (e.Key)
        {
            case Key.Delete:
                if (_vm.DeleteCommand.CanExecute(null))
                    _vm.DeleteCommand.Execute(null);
                break;

            case Key.Escape:
                _vm.ClearSelectionCommand.Execute(null);
                // 通知当前工具取消操作
                _vm.GetActiveTool().OnCancel(this);
                break;

            case Key.Enter:
                _vm.GetActiveTool().OnConfirm(this);
                break;

            case Key.A:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    _vm.SelectAllCommand.Execute(null);
                break;

            case Key.Z:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        if (_vm.RedoCommand.CanExecute(null))
                            _vm.RedoCommand.Execute(null);
                    }
                    else
                    {
                        if (_vm.UndoCommand.CanExecute(null))
                            _vm.UndoCommand.Execute(null);
                    }
                }
                break;

            case Key.Y:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (_vm.RedoCommand.CanExecute(null))
                        _vm.RedoCommand.Execute(null);
                }
                break;
        }
    }

    // ─── IEditorContext 接口实现 ───

    /// <summary>获取形状集合</summary>
    ObservableCollection<RoiShape> IEditorContext.Shapes => _vm!.Shapes;

    /// <summary>获取 ViewModel 引用</summary>
    RoiEditorViewModel IEditorContext.ViewModel => _vm!;

    /// <summary>刷新覆盖层</summary>
    void IEditorContext.RefreshOverlay()
    {
        UpdateOverlay();
    }

    /// <summary>设置框选矩形的显示</summary>
    void IEditorContext.SetSelectionRect(Rect? rect)
    {
        if (rect == null)
        {
            _selectionRect = null;
            UpdateOverlay();
            return;
        }

        var r = rect.Value;

        if (_selectionRect == null)
        {
            _selectionRect = new System.Windows.Shapes.Rectangle
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = new SolidColorBrush(Color.FromArgb(30, 30, 144, 255)),
                SnapsToDevicePixels = true
            };
        }

        Canvas.SetLeft(_selectionRect, r.X);
        Canvas.SetTop(_selectionRect, r.Y);
        _selectionRect.Width = r.Width;
        _selectionRect.Height = r.Height;

        // 确保框选矩形在覆盖层中
        if (!OverlayCanvas.Children.Contains(_selectionRect))
        {
            OverlayCanvas.Children.Add(_selectionRect);
        }
    }

    /// <summary>获取控件实际尺寸</summary>
    Size IEditorContext.ControlSize => new Size(ActualWidth, ActualHeight);


    void IEditorContext.RefreshCommandStates()
    {
        _vm?.RefreshCommandStates();
    }
}