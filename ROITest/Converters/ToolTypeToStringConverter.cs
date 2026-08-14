using ROITest.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ROITest.Converters;

/// <summary>
/// 工具类型 → 字符串转换器（用于状态栏显示当前工具名）
/// </summary>
public class ToolTypeToStringConverter : IValueConverter
{
    private static readonly Dictionary<RoiToolType, string> DisplayNames = new()
    {
        [RoiToolType.Pointer] = "指针",
        [RoiToolType.Rectangle] = "矩形",
        [RoiToolType.RotatedRectangle] = "旋转矩形",
        [RoiToolType.Circle] = "圆形",
        [RoiToolType.Ellipse] = "椭圆",
        [RoiToolType.Line] = "线段",
        [RoiToolType.Polygon] = "多边形",
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RoiToolType toolType && DisplayNames.TryGetValue(toolType, out var name))
            return name;
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
