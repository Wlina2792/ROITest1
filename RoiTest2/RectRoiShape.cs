using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RoiTest2;

/// <summary>
/// 不旋转的矩形ROI图形，支持自定义位置、尺寸、描边、填充
/// </summary>
public class RectRoiShape : Shape
{
    #region 依赖属性

    // 左上角X坐标
    public static readonly DependencyProperty XProperty =
        DependencyProperty.Register(
            nameof(X),
            typeof(double),
            typeof(RectRoiShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    // 左上角Y坐标
    public static readonly DependencyProperty YProperty =
        DependencyProperty.Register(
            nameof(Y),
            typeof(double),
            typeof(RectRoiShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    // 矩形宽度
    public static readonly DependencyProperty WProperty =
        DependencyProperty.Register(
            nameof(W),
            typeof(double),
            typeof(RectRoiShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    // 矩形高度
    public static readonly DependencyProperty HProperty =
        DependencyProperty.Register(
            nameof(H),
            typeof(double),
            typeof(RectRoiShape),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

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

    #endregion


    /// <summary>
    /// 实现Shape的抽象属性，定义矩形的几何描述
    /// </summary>
    protected override Geometry DefiningGeometry
    {
        get
        {
            if (W <= 0 || H <= 0)
                return Geometry.Empty;

            return new RectangleGeometry(new Rect(X, Y, W, H));
        }
    }


    /// <summary>
    /// 重写渲染逻辑：绘制矩形
    /// </summary>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 跳过无效尺寸的绘制
        if (W <= 0 || H <= 0) return;

        var rect = new Rect(X, Y, W, H);

        // 复用Shape自带的Stroke/Fill属性，保持和WPF原生图形一致的样式能力
        var pen = Stroke != null ? new Pen(Stroke, StrokeThickness) : null;
        drawingContext.DrawRectangle(Fill, pen, rect);
    }
}
