using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace MovieApp.UI.Views.Components;

public sealed partial class StarSlider : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(StarSlider), new PropertyMetadata(0.0, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private bool isDragging = false;
    private const int MaxStars = 5;

    public StarSlider()
    {
        this.InitializeComponent();
        this.SizeChanged += (s, e) => UpdateMaskWidth();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StarSlider slider)
        {
            slider.UpdateMaskWidth();
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        isDragging = true;
        RootGrid.CapturePointer(e.Pointer);
        CalculateHalfStep(e);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (isDragging)
        {
            CalculateHalfStep(e);
        }
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        isDragging = false;
        RootGrid.ReleasePointerCapture(e.Pointer);
    }

    private void CalculateHalfStep(PointerRoutedEventArgs e)
    {
        if (RootGrid.ActualWidth == 0) return;

        var point = e.GetCurrentPoint(RootGrid).Position;
        double x = Math.Clamp(point.X, 0, RootGrid.ActualWidth);
        double starWidth = RootGrid.ActualWidth / MaxStars;
        double rawRating = x / starWidth;
        Value = Math.Round(rawRating * 2, MidpointRounding.AwayFromZero) / 2.0;
    }

    private void UpdateMaskWidth()
    {
        if (RootGrid.ActualWidth == 0) return;
        double starWidth = RootGrid.ActualWidth / MaxStars;
        FilledMask.Width = Value * starWidth;
    }
}
