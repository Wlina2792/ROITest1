using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ROITest.Models;

/// <summary>
/// ROI 形状基类，所有具体形状继承此类
/// </summary>
public abstract partial class RoiShape : ObservableObject
{
    private static int _nextId;

    protected RoiShape()
    {
        Id = Interlocked.Increment(ref _nextId);
    }

    /// <summary>唯一标识</summary>
    public int Id { get; }

    /// <summary>是否被选中</summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>描边颜色</summary>
    [ObservableProperty]
    private Brush _stroke = Brushes.LimeGreen;

    /// <summary>描边厚度</summary>
    [ObservableProperty]
    private double _strokeThickness = 1.5;

    /// <summary>填充颜色（默认半透明）</summary>
    [ObservableProperty]
    private Brush _fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 0));

    // ─── 抽象方法：由子类实现 ───

    /// <summary>获取 WPF 几何对象，用于渲染</summary>
    public abstract Geometry GetGeometry();

    /// <summary>命中检测（点中）</summary>
    public abstract bool HitTest(Point pt);

    /// <summary>命中检测（矩形框选）</summary>
    public abstract bool HitTest(Rect rect);

    /// <summary>获取控制手柄位置（选中时显示）</summary>
    public abstract Point[] GetHandles();

    /// <summary>克隆副本（用于撤销重做）</summary>
    public abstract RoiShape Clone();
}