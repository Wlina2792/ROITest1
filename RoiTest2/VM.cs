using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RoiTest2;

public partial class VM:ObservableObject
{
    [ObservableProperty] private double x=0;
    [ObservableProperty] private double y=0;
    [ObservableProperty] private double w=200;
    [ObservableProperty] private double h=200;



}
