using System.Windows;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using ROITest.ViewModels;

namespace ROITest;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 绑定 ViewModel 到编辑器控件
        if (DataContext is MainViewModel mainVm)
        {
            EditorControl.BindViewModel(mainVm.Editor);
        }

        // 窗口加载完成后聚焦到编辑器
        Loaded += (_, _) => EditorControl.Focus();
    }

    /// <summary>
    /// 加载底图按钮点击事件
    /// </summary>
    private void OnLoadImageClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择底图",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|所有文件|*.*",
            FilterIndex = 1
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze(); // 冻结以便跨线程访问

                EditorControl.SetImage(bitmap);

                if (DataContext is MainViewModel vm)
                {
                    vm.StatusText = $"已加载：{System.IO.Path.GetFileName(dialog.FileName)}" +
                                    $" ({bitmap.PixelWidth}×{bitmap.PixelHeight})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}