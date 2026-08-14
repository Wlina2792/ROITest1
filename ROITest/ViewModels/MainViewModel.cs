using CommunityToolkit.Mvvm.ComponentModel;

namespace ROITest.ViewModels;

/// <summary>
/// 主窗体视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Editor = new RoiEditorViewModel();
    }

    /// <summary>ROI 编辑器视图模型</summary>
    [ObservableProperty]
    private RoiEditorViewModel _editor;

    /// <summary>状态栏文本</summary>
    [ObservableProperty]
    private string _statusText = "就绪";
}