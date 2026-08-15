using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RoiTest2;

public class RectRoiControl : Control
{
    private const double MinSize = 10;

    #region 字段

    private Thumb _moveTracker;
    private Thumb _leftTopTracker;
    private Thumb _centerTopTracker;
    private Thumb _rightTopTracker;
    private Thumb _leftCenterTracker;
    private Thumb _rightCenterTracker;
    private Thumb _leftBottomTracker;
    private Thumb _centerBottomTracker;
    private Thumb _rightBottomTracker;

    #endregion

    #region 依赖属性

    public static readonly DependencyProperty XProperty =
        DependencyProperty.Register(nameof(X), typeof(double), typeof(RectRoiControl),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRectChanged));

    public static readonly DependencyProperty YProperty =
        DependencyProperty.Register(nameof(Y), typeof(double), typeof(RectRoiControl),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRectChanged));

    public static readonly DependencyProperty WProperty =
        DependencyProperty.Register(nameof(W), typeof(double), typeof(RectRoiControl),
            new FrameworkPropertyMetadata(100.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRectChanged));

    public static readonly DependencyProperty HProperty =
        DependencyProperty.Register(nameof(H), typeof(double), typeof(RectRoiControl),
            new FrameworkPropertyMetadata(100.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRectChanged));

    public static readonly DependencyProperty MoveCommandProperty =
        DependencyProperty.Register(nameof(MoveCommand), typeof(ICommand), typeof(RectRoiControl));

    public static readonly DependencyProperty ResizeCommandProperty =
        DependencyProperty.Register(nameof(ResizeCommand), typeof(ICommand), typeof(RectRoiControl));

    #endregion

    #region 属性包装

    public double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public double W
    {
        get => (double)GetValue(WProperty);
        set => SetValue(WProperty, value);
    }

    public double H
    {
        get => (double)GetValue(HProperty);
        set => SetValue(HProperty, value);
    }

    public ICommand MoveCommand
    {
        get => (ICommand)GetValue(MoveCommandProperty);
        set => SetValue(MoveCommandProperty, value);
    }

    public ICommand ResizeCommand
    {
        get => (ICommand)GetValue(ResizeCommandProperty);
        set => SetValue(ResizeCommandProperty, value);
    }

    #endregion

    #region 构造与模板

    static RectRoiControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RectRoiControl),
            new FrameworkPropertyMetadata(typeof(RectRoiControl)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 先取消旧事件订阅，防止重复
        UnsubscribeEvents();

        // 获取模板子元素
        _moveTracker = GetTemplateChild("MoveTracker") as Thumb;
        _leftTopTracker = GetTemplateChild("LeftTopTracker") as Thumb;
        _centerTopTracker = GetTemplateChild("CenterTopTracker") as Thumb;
        _rightTopTracker = GetTemplateChild("RightTopTracker") as Thumb;
        _leftCenterTracker = GetTemplateChild("LeftCenterTracker") as Thumb;
        _rightCenterTracker = GetTemplateChild("RightCenterTracker") as Thumb;
        _leftBottomTracker = GetTemplateChild("LeftBottomTracker") as Thumb;
        _centerBottomTracker = GetTemplateChild("CenterBottomTracker") as Thumb;
        _rightBottomTracker = GetTemplateChild("RightBottomTracker") as Thumb;

        Debug.WriteLine("获取模版把手");
        // 订阅事件
        SubscribeEvents();

        // 初始定位所有手柄
        LocateAllTrackers();
    }

    #endregion

    #region 事件订阅/取消

    private void SubscribeEvents()
    {
        if (_moveTracker != null)
            _moveTracker.DragDelta += MoveTracker_DragDelta;

        if (_leftTopTracker != null)
            _leftTopTracker.DragDelta += LeftTopTracker_DragDelta;
        if (_centerTopTracker != null)
            _centerTopTracker.DragDelta += CenterTopTracker_DragDelta;
        if (_rightTopTracker != null)
            _rightTopTracker.DragDelta += RightTopTracker_DragDelta;
        if (_leftCenterTracker != null)
            _leftCenterTracker.DragDelta += LeftCenterTracker_DragDelta;
        if (_rightCenterTracker != null)
            _rightCenterTracker.DragDelta += RightCenterTracker_DragDelta;
        if (_leftBottomTracker != null)
            _leftBottomTracker.DragDelta += LeftBottomTracker_DragDelta;
        if (_centerBottomTracker != null)
            _centerBottomTracker.DragDelta += CenterBottomTracker_DragDelta;
        if (_rightBottomTracker != null)
            _rightBottomTracker.DragDelta += RightBottomTracker_DragDelta;
    }

    private void UnsubscribeEvents()
    {
        if (_moveTracker != null)
            _moveTracker.DragDelta -= MoveTracker_DragDelta;

        if (_leftTopTracker != null)
            _leftTopTracker.DragDelta -= LeftTopTracker_DragDelta;
        if (_centerTopTracker != null)
            _centerTopTracker.DragDelta -= CenterTopTracker_DragDelta;
        if (_rightTopTracker != null)
            _rightTopTracker.DragDelta -= RightTopTracker_DragDelta;
        if (_leftCenterTracker != null)
            _leftCenterTracker.DragDelta -= LeftCenterTracker_DragDelta;
        if (_rightCenterTracker != null)
            _rightCenterTracker.DragDelta -= RightCenterTracker_DragDelta;
        if (_leftBottomTracker != null)
            _leftBottomTracker.DragDelta -= LeftBottomTracker_DragDelta;
        if (_centerBottomTracker != null)
            _centerBottomTracker.DragDelta -= CenterBottomTracker_DragDelta;
        if (_rightBottomTracker != null)
            _rightBottomTracker.DragDelta -= RightBottomTracker_DragDelta;
    }

    #endregion

    #region 拖拽事件处理

    /// <summary>
    /// 整体移动：X 和 Y 同步变化
    /// </summary>
    private void MoveTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        SetCurrentValue(XProperty, X + e.HorizontalChange);
        SetCurrentValue(YProperty, Y + e.VerticalChange);
        LocateAllTrackers();
        MoveCommand?.Execute(new Point(X, Y));
    }

    /// <summary>
    /// 左上角：X↑ W↓  Y↑ H↓
    /// </summary>
    private void LeftTopTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"LeftTopTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W - e.HorizontalChange;
        var newH = H - e.VerticalChange;
        if (newW < MinSize || newH < MinSize) return;

        SetCurrentValue(XProperty, X + e.HorizontalChange);
        SetCurrentValue(YProperty, Y + e.VerticalChange);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 上中：Y↑ H↓
    /// </summary>
    private void CenterTopTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"CenterTopTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newH = H - e.VerticalChange;
        if (newH < MinSize) return;

        SetCurrentValue(YProperty, Y + e.VerticalChange);
        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 右上角：W↑  Y↑ H↓
    /// </summary>
    private void RightTopTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"RightTopTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W + e.HorizontalChange;
        var newH = H - e.VerticalChange;
        if (newW < MinSize || newH < MinSize) return;

        SetCurrentValue(YProperty, Y + e.VerticalChange);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 左中：X↑ W↓
    /// </summary>
    private void LeftCenterTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"LeftCenterTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W - e.HorizontalChange;
        if (newW < MinSize) return;

        SetCurrentValue(XProperty, X + e.HorizontalChange);
        SetCurrentValue(WProperty, newW);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 右中：W↑
    /// </summary>
    private void RightCenterTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"RightCenterTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W + e.HorizontalChange;
        if (newW < MinSize) return;

        SetCurrentValue(WProperty, newW);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 左下角：X↑ W↓  H↑
    /// </summary>
    private void LeftBottomTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"LeftBottomTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W - e.HorizontalChange;
        var newH = H + e.VerticalChange;
        if (newW < MinSize || newH < MinSize) return;

        SetCurrentValue(XProperty, X + e.HorizontalChange);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 下中：H↑
    /// </summary>
    private void CenterBottomTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"CenterBottomTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newH = H + e.VerticalChange;
        if (newH < MinSize) return;

        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    /// <summary>
    /// 右下角：W↑  H↑
    /// </summary>
    private void RightBottomTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        Debug.WriteLine($"RightBottomTracker_DragDelta: {e.HorizontalChange}, {e.VerticalChange}");
        var newW = W + e.HorizontalChange;
        var newH = H + e.VerticalChange;
        if (newW < MinSize || newH < MinSize) return;

        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);
        LocateAllTrackers();
        ResizeCommand?.Execute(new Rect(X, Y, W, H));
    }

    #endregion

    #region 手柄定位

    /// <summary>
    /// 属性变化时重新定位所有手柄
    /// </summary>
    private static void OnRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as RectRoiControl;
        control?.LocateAllTrackers();
    }

    /// <summary>
    /// 将8个手柄定位到矩形对应的锚点位置
    /// 手柄通过 Margin 偏移来定位（相对于 Grid 左上角）
    /// </summary>
    private void LocateAllTrackers()
    {
        if (_leftTopTracker == null) return; // 模板未应用时跳过

        double x = X, y = Y, w = W, h = H;

        // 四个角
        SetTrackerPosition(_leftTopTracker, x, y);
        SetTrackerPosition(_rightTopTracker, x + w, y);
        SetTrackerPosition(_leftBottomTracker, x, y + h);
        SetTrackerPosition(_rightBottomTracker, x + w, y + h);

        // 四个边中点
        SetTrackerPosition(_centerTopTracker, x + w / 2, y);
        SetTrackerPosition(_centerBottomTracker, x + w / 2, y + h);
        SetTrackerPosition(_leftCenterTracker, x, y + h / 2);
        SetTrackerPosition(_rightCenterTracker, x + w, y + h / 2);
    }

    /// <summary>
    /// 通过 Margin 将手柄定位到指定坐标
    /// 手柄宽高为10，减去一半使其中心对准锚点
    /// </summary>
    private void SetTrackerPosition(Thumb tracker, double anchorX, double anchorY)
    {
        if (tracker == null) return;

        double halfW = tracker.ActualWidth > 0 ? tracker.ActualWidth / 2 : 5;
        double halfH = tracker.ActualHeight > 0 ? tracker.ActualHeight / 2 : 5;

        tracker.Margin = new Thickness(anchorX - halfW, anchorY - halfH, 0, 0);
    }

    #endregion
}