using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RoiTest2;

public class RectRoiControl : Control
{
    private const double MinSize = 4;
    private const double HalfThumbSize = 5;

    #region 字段

    private Thumb _moveTracker;
    private Thumb _leftTopTracker;
    private Thumb _rightTopTracker;
    private Thumb _leftBottomTracker;
    private Thumb _rightBottomTracker;

    #endregion

    #region 依赖属性

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(RectRoiControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRectChanged));
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

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

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

        UnsubscribeEvents();

        _moveTracker = GetTemplateChild("MoveTracker") as Thumb;
        _leftTopTracker = GetTemplateChild("LeftTopTracker") as Thumb;
        _rightTopTracker = GetTemplateChild("RightTopTracker") as Thumb;
        _leftBottomTracker = GetTemplateChild("LeftBottomTracker") as Thumb;
        _rightBottomTracker = GetTemplateChild("RightBottomTracker") as Thumb;

        SubscribeEvents();
        LocateAllTrackers();
    }

    #endregion

    #region 事件订阅 / 取消

    private void SubscribeEvents()
    {
        if (_moveTracker != null)
            _moveTracker.DragDelta += MoveTracker_DragDelta;

        if (_leftTopTracker != null)
            _leftTopTracker.DragDelta += LeftTopTracker_DragDelta;
        if (_rightTopTracker != null)
            _rightTopTracker.DragDelta += RightTopTracker_DragDelta;
        if (_leftBottomTracker != null)
            _leftBottomTracker.DragDelta += LeftBottomTracker_DragDelta;
        if (_rightBottomTracker != null)
            _rightBottomTracker.DragDelta += RightBottomTracker_DragDelta;
    }

    private void UnsubscribeEvents()
    {
        if (_moveTracker != null)
            _moveTracker.DragDelta -= MoveTracker_DragDelta;

        if (_leftTopTracker != null)
            _leftTopTracker.DragDelta -= LeftTopTracker_DragDelta;
        if (_rightTopTracker != null)
            _rightTopTracker.DragDelta -= RightTopTracker_DragDelta;
        if (_leftBottomTracker != null)
            _leftBottomTracker.DragDelta -= LeftBottomTracker_DragDelta;
        if (_rightBottomTracker != null)
            _rightBottomTracker.DragDelta -= RightBottomTracker_DragDelta;
    }

    #endregion

    #region 拖拽事件处理

    private void MoveTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newLeft = (double.IsNaN(Canvas.GetLeft(this)) ? 0 : Canvas.GetLeft(this)) + e.HorizontalChange;
        var newTop = (double.IsNaN(Canvas.GetTop(this)) ? 0 : Canvas.GetTop(this)) + e.VerticalChange;

        SetCurrentValue(Canvas.LeftProperty, newLeft);
        SetCurrentValue(Canvas.TopProperty, newTop);
        SetCurrentValue(XProperty, newLeft);
        SetCurrentValue(YProperty, newTop);

        MoveCommand?.Execute(new Point(newLeft, newTop));
    }

    private void LeftTopTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newW = Math.Max(W - e.HorizontalChange, MinSize);
        var newH = Math.Max(H - e.VerticalChange, MinSize);
        if (newH == MinSize|| newW == MinSize) return;

        var newLeft = (double.IsNaN(Canvas.GetLeft(this)) ? 0 : Canvas.GetLeft(this)) + e.HorizontalChange;
        var newTop = (double.IsNaN(Canvas.GetTop(this)) ? 0 : Canvas.GetTop(this)) + e.VerticalChange;

        SetCurrentValue(Canvas.LeftProperty, newLeft);
        SetCurrentValue(Canvas.TopProperty, newTop);
        SetCurrentValue(XProperty, newLeft);
        SetCurrentValue(YProperty, newTop);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);

        ResizeCommand?.Execute(new Rect(newLeft, newTop, newW, newH));
    }

    private void RightTopTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newW = Math.Max(W + e.HorizontalChange, MinSize);
        var newH = Math.Max(H - e.VerticalChange, MinSize);
        if (newH == MinSize) return;
        var newTop = (double.IsNaN(Canvas.GetTop(this)) ? 0 : Canvas.GetTop(this)) + e.VerticalChange;

       

        SetCurrentValue(Canvas.TopProperty, newTop);
        SetCurrentValue(YProperty, newTop);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);

        ResizeCommand?.Execute(new Rect(X, newTop, newW, newH));
    }

    private void LeftBottomTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newW = Math.Max(W - e.HorizontalChange, MinSize);
        var newH = Math.Max(H + e.VerticalChange, MinSize);

        if (newW == MinSize) return;
        var newLeft = (double.IsNaN(Canvas.GetLeft(this)) ? 0 : Canvas.GetLeft(this)) + e.HorizontalChange;

       
        SetCurrentValue(Canvas.LeftProperty, newLeft);
        SetCurrentValue(XProperty, newLeft);
        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);

        ResizeCommand?.Execute(new Rect(newLeft, Y, newW, newH));
    }

    private void RightBottomTracker_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newW = Math.Max(W + e.HorizontalChange, MinSize);
        var newH = Math.Max(H + e.VerticalChange, MinSize);
        

        SetCurrentValue(WProperty, newW);
        SetCurrentValue(HProperty, newH);

        ResizeCommand?.Execute(new Rect(X, Y, newW, newH));
    }

    #endregion

    #region 手柄定位

    private static void OnRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as RectRoiControl;
        control?.LocateAllTrackers();
        // X/Y 变化时同步设置 Canvas 位置
        if (e.Property == XProperty || e.Property == YProperty)
        {
            control?.SetCanvasPosition();
        }
    }

    private void SetCanvasPosition()
    {
        SetCurrentValue(Canvas.LeftProperty, X);
        SetCurrentValue(Canvas.TopProperty, Y);
    }

    private void LocateAllTrackers()
    {
        if (_leftTopTracker == null) return;

        double w = W, h = H;

        SetTrackerPosition(_leftTopTracker, 0, 0);
        SetTrackerPosition(_rightTopTracker, w, 0);
        SetTrackerPosition(_leftBottomTracker, 0, h);
        SetTrackerPosition(_rightBottomTracker, w, h);
    }

    private void SetTrackerPosition(Thumb tracker, double anchorX, double anchorY)
    {
        if (tracker == null) return;
        Canvas.SetLeft(tracker, anchorX - HalfThumbSize);
        Canvas.SetTop(tracker, anchorY - HalfThumbSize);
    }

    #endregion
}