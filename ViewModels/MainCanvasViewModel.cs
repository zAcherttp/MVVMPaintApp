﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using MVVMPaintApp.Models;
using MVVMPaintApp.Models.Tools;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using MVVMPaintApp.Views;
using System.Diagnostics;

namespace MVVMPaintApp.ViewModels
{
    public partial class MainCanvasViewModel : ObservableObject
    {
        private readonly IWindowManager windowManager;
        public readonly IDialogService dialogService;
        private readonly ViewModelLocator viewModelLocator;

        [ObservableProperty]
        private bool showGridBorders = false;

        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private string windowTitle = "Home";

        [ObservableProperty]
        private DrawingCanvasViewModel drawingCanvasViewModel;

        [ObservableProperty]
        private ColorPaletteViewModel colorPaletteViewModel;

        [ObservableProperty]
        private LayerViewModel layerViewModel;

        [ObservableProperty]
        private ToolboxViewModel toolboxViewModel;

        [ObservableProperty]
        private int windowWidth = 1600;

        [ObservableProperty]
        private int windowHeight = 900;

        [RelayCommand]
        public void SetProject(Project project)
        {
            ProjectManager.LoadProject(project);
            WindowTitle = "Home - " + project.Name;

            DrawingCanvasViewModel.SetProjectManager(ProjectManager);
            ColorPaletteViewModel.SetProjectColors(project.ColorsList);
            LayerViewModel.SetProjectManager(ProjectManager);
            ToolboxViewModel.SetProjectManager(ProjectManager);

            ProjectManager.FitToWindowCommand.Execute(null);
        }

        public MainCanvasViewModel(
            IWindowManager windowManager,
            IDialogService dialogService,
            ViewModelLocator viewModelLocator,
            ProjectManager projectManager,
            DrawingCanvasViewModel drawingCanvasViewModel,
            ColorPaletteViewModel colorPaletteViewModel,
            LayerViewModel layerViewModel,
            ToolboxViewModel toolboxViewModel)
        {
            this.windowManager = windowManager;
            this.dialogService = dialogService;
            this.viewModelLocator = viewModelLocator;
            DrawingCanvasViewModel = drawingCanvasViewModel;
            ColorPaletteViewModel = colorPaletteViewModel;
            LayerViewModel = layerViewModel;
            ToolboxViewModel = toolboxViewModel;
            ProjectManager = projectManager;
        }

        [RelayCommand]
        private void SaveAsPNG()
        {
            ProjectManager.SaveProjectAs(ProjectManager.CurrentProject, ImageFileType.PNG);
        }

        [RelayCommand]
        private void SaveAsJPEG()
        {
            ProjectManager.SaveProjectAs(ProjectManager.CurrentProject, ImageFileType.JPEG);
        }

        [RelayCommand]
        private void SaveAsBMP()
        {
            ProjectManager.SaveProjectAs(ProjectManager.CurrentProject, ImageFileType.BMP);
        }

        [RelayCommand]
        private void Save()
        {
            ProjectManager.SaveProject();
        }

        [RelayCommand]
        private void Exit()
        {
            windowManager.CloseWindow(this);
        }

        [RelayCommand]
        private void Open()
        {
            windowManager.ShowWindow(viewModelLocator.DashboardViewModel);
            windowManager.CloseWindow(this);
        }

        [RelayCommand]
        private void New()
        {
            windowManager.ShowWindow(viewModelLocator.NewFileViewModel);
            windowManager.CloseWindow(this);
        }

        [RelayCommand]
        private void Undo()
        {
            ProjectManager.UndoRedoManager.Undo();
        }

        [RelayCommand]
        private void Redo()
        {
            ProjectManager.UndoRedoManager.Redo();
        }

        //<MenuItem Header = "Copy" Command="{Binding CopyCommand}"/>
        //<MenuItem Header = "Paste" Command="{Binding PasteCommand}"/>
        //< MenuItem Header="Zoom In" Command="{Binding ZoomInCommand}"/>
        //<MenuItem Header = "Zoom Out" Command="{Binding ZoomOutCommand}"/>
        //<MenuItem Header = "Fit to Window" Command="{Binding FitToWindowCommand}"/>

        //[RelayCommand]
        //private void Cut()
        //{
        //    ProjectManager.Cut();
        //}

        [RelayCommand]
        private void Copy()
        {
            if (DrawingCanvasViewModel.SelectedTool is RectSelect rectSelect && rectSelect.SelectedContent != null)
            {
                var wb = rectSelect.SelectedContent;

                BitmapSource bitmapSource = BitmapSource.Create(
                    wb.PixelWidth, wb.PixelHeight,
                    wb.DpiX, wb.DpiY,
                    PixelFormats.Bgra32,
                    null,
                    wb.BackBuffer,
                    wb.BackBufferStride * wb.PixelHeight,
                    wb.BackBufferStride
                );

                bitmapSource.Freeze();

                Clipboard.SetImage(bitmapSource);
            }
        }

        //[RelayCommand]
        //private void Paste()
        //{
        //    ProjectManager.Paste();
        //}

        [RelayCommand]
        private void ZoomIn()
        {
            ProjectManager.ZoomIn();
        }

        [RelayCommand]
        private void ZoomOut()
        {
            ProjectManager.ZoomOut();
        }

        [RelayCommand]
        private async Task ShowResizeDialog()
        {
            var vm = new ResizeViewModel(ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height);
            var (result, dialogVm) = dialogService.ShowDialog(vm);

            if (result)
            {
                Debug.WriteLine("Width: " + dialogVm.Width + " Height: " + dialogVm.Height);
                Debug.WriteLine("IsPixels: " + dialogVm.IsPixels);
                // Use dialogVm.Width and dialogVm.Height to resize the canvas
            }
        }

        [RelayCommand]
        private void ShowAboutDialog()
        {
            var vm = new AboutViewModel();
            dialogService.ShowDialog(vm);
        }
    }
}
