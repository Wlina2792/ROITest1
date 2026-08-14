using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ROITest.ViewModels;

namespace ROITest.Converters;

/// <summary>
/// 枚举工具类型 ↔ 布尔值转换器
/// 用于 ToolBar 中 ToggleButton 的 IsChecked 双向绑定
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RoiToolType toolType && parameter is string paramStr)
        {
            if (Enum.TryParse<RoiToolType>(paramStr, out var paramType))
                return toolType == paramType;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string paramStr)
        {
            if (Enum.TryParse<RoiToolType>(paramStr, out var toolType))
                return toolType;
        }
        return Binding.DoNothing;
    }
}


