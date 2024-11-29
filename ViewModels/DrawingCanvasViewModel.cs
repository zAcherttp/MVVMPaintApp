﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.ViewModels
{
    public partial class DrawingCanvasViewModel : ObservableObject
    {
        private const double ZOOM_STEP_PERCENTAGE = 0.1;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private WriteableBitmap canvasRenderTarget;

        [ObservableProperty]
        public int canvasWidth;

        [ObservableProperty]
        public int canvasHeight;

        // Canvas properties
        [ObservableProperty]
        private Easing panOffsetX = new(0.0);

        [ObservableProperty]
        private Easing panOffsetY = new (0.0);

        [ObservableProperty]
        private Easing zoomFactor = new(1.0);

        // Mode properties
        // to be changed to Tools class using enum tool types
        [ObservableProperty]
        private bool isZoomMode;

        [ObservableProperty]
        private bool isPanMode;

        [ObservableProperty]
        private bool isCtrlPressed;


        // Debugging properties
        [ObservableProperty]
        private string modeText = "";

        [ObservableProperty]
        private string mouseInfo = "0, 0";
        

        public DrawingCanvasViewModel()//Project project)
        {
            if(currentProject == null)
            {
                currentProject = new Project();
            }
            canvasRenderTarget = new WriteableBitmap(currentProject.Width, currentProject.Height, 96, 96, PixelFormats.Pbgra32, null);

            RenderProject();
        }

        public DrawingCanvasViewModel(Project project)//Project project)
        {
            currentProject = project;

            canvasRenderTarget = new WriteableBitmap(currentProject.Width, currentProject.Height, 96, 96, PixelFormats.Pbgra32, null);

            RenderProject();
        }


        public void RenderProject()
        {
            if (CurrentProject == null || CanvasRenderTarget == null) return;

            using var context = CanvasRenderTarget.GetBitmapContext();
            // Clear the canvas
            CanvasRenderTarget.Clear(CurrentProject.Background);

            // Render each layer
            foreach (var layer in CurrentProject.Layers)
            {
                if (layer.IsVisible)
                {
                    Rect rect = new(0, 0, layer.Content.PixelWidth, layer.Content.PixelHeight);
                    CanvasRenderTarget.Blit(rect, layer.Content, rect,
                        WriteableBitmapExtensions.BlendMode.Alpha);
                }
            }
        }


        partial void OnIsZoomModeChanged(bool value)
        {
            UpdateModeText();
        }

        partial void OnIsPanModeChanged(bool value)
        {
            UpdateModeText();
        }

        private void UpdateModeText()
        {
            ModeText = IsZoomMode ? "ZOOM" : (IsPanMode ? "PAN" : "");
        }

        [RelayCommand]
        private void ToggleZoomMode()
        {
            IsZoomMode ^= true;
            IsPanMode = false;
        }

        [RelayCommand]
        private void TogglePanMode()
        {
            IsPanMode ^= true;
            IsZoomMode = false;
        }

        [RelayCommand]
        private async Task Reset()
        {
            var tasks = new[]
            {
                PanOffsetX.EaseDeltaAsync(-PanOffsetX.Value, Easing.EasingType.EaseInOutCubic , 300),
                PanOffsetY.EaseDeltaAsync(-PanOffsetY.Value, Easing.EasingType.EaseInOutCubic, 300),
                ZoomFactor.EaseDeltaAsync(-ZoomFactor.Value + 1f, Easing.EasingType.EaseInOutCubic, 300)
            };
            await Task.WhenAll(tasks);
        }

        [RelayCommand]
        private async Task FitToWindow()
        {
            double newZoomFactor = Math.Min((double)CanvasRenderTarget.PixelWidth / CurrentProject.Width,
                (double)CanvasRenderTarget.PixelHeight / CurrentProject.Height);
            await ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 300);
        }

        public void UpdateMouseInfo(Point position, bool isPressed)
        {
            MouseInfo = $"{position.X:F0}, {position.Y:F0}" + (isPressed ? " [DOWN]" : "");
        }

        public void HandleCtrlKeyPress(bool isPressed)
        {
            IsCtrlPressed = isPressed;
        }

        public async Task HandleMouseWheel(MouseWheelEventArgs e)
        {
            await HandleMouseZoom(e);
        }

        public async Task HandleMouseZoom(MouseWheelEventArgs e)
        {
            if (!IsZoomMode) return;

            double delta = e.Delta / 120.0;
            double newZoomFactor = Math.Clamp(ZoomFactor.Value + (delta * ZOOM_STEP_PERCENTAGE), 0.1, 10); 
            await ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 30);
        }

        public void HandleMousePan(Point startPoint, Point currentPoint)
        {
            if (!IsPanMode) return;

            double deltaX = currentPoint.X - startPoint.X;
            double deltaY = currentPoint.Y - startPoint.Y;

            PanOffsetX.Value += deltaX;
            PanOffsetY.Value += deltaY;
        }
    }
}