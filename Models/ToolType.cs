﻿using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models.Tools;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models
{
    public enum ToolType
    {
        Pencil,
        Brush,
        Eraser,
        Fill,
        ZoomPan,
        ColorPicker,
        Shape,
        RectSelect,
        Default
    }
}
