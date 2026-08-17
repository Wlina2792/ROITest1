using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoiTest2;

public partial class VM:ObservableObject
{
    [ObservableProperty] private double x=20;
    [ObservableProperty] private double y=40;
    [ObservableProperty] private double w=100;
    [ObservableProperty] private double h=200;



}
