﻿//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Globalization;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using LiveCharts.Uwp.Points;
using LiveCharts.Uwp.Components;

namespace LiveCharts.Uwp
{
    /// <summary>
    /// The gauge chart is useful to display progress or completion.
    /// </summary>
    public class Gauge : UserControl
    {
        public Gauge()
        {
            Canvas = new Canvas();
            
            Content = Canvas;

            PieBack = new PieSlice();
            Pie = new PieSlice();
            MeasureTextBlock = new TextBlock();
            LeftLabel = new TextBlock();
            RightLabel = new TextBlock();

            Canvas.Children.Add(PieBack);
            Canvas.Children.Add(Pie);
            Canvas.Children.Add(MeasureTextBlock);
            Canvas.Children.Add(RightLabel);
            Canvas.Children.Add(LeftLabel);

            Canvas.SetZIndex(PieBack, 0);
            Canvas.SetZIndex(Pie, 1);

            PieBack.SetBinding(Shape.FillProperty,
                new Binding {Path = new PropertyPath("GaugeBackground"), Source = this});
            PieBack.SetBinding(Shape.StrokeThicknessProperty,
                new Binding {Path = new PropertyPath("StrokeThickness"), Source = this});
            PieBack.SetBinding(Shape.StrokeProperty,
                new Binding {Path = new PropertyPath("Stroke"), Source = this});

            Pie.SetBinding(Shape.StrokeThicknessProperty,
                new Binding { Path = new PropertyPath("StrokeThickness"), Source = this });
            Pie.Stroke = new SolidColorBrush(Colors.Transparent);

            this.SetIfNotSet(GaugeBackgroundProperty, new SolidColorBrush(Color.FromArgb(255, 21, 101, 191)) {Opacity = .1});
            this.SetIfNotSet(StrokeThicknessProperty, 0d);
            this.SetIfNotSet(StrokeProperty, new SolidColorBrush(Color.FromArgb(255, 222, 222, 222)));

            this.SetIfNotSet(FromColorProperty, Color.FromArgb(255, 100, 180, 245));
            this.SetIfNotSet(ToColorProperty, Color.FromArgb(255, 21, 101, 191));

            this.SetIfNotSet(MinHeightProperty, 50d);
            this.SetIfNotSet(MinWidthProperty, 80d);

            this.SetIfNotSet(AnimationsSpeedProperty, TimeSpan.FromMilliseconds(800));

            MeasureTextBlock.FontWeight = FontWeights.Bold;

            IsNew = true;

            SizeChanged += (sender, args) =>
            {
                IsChartInitialized = true;
                Update();
            };
        }

        #region Properties

        private Canvas Canvas { get; }
        private PieSlice PieBack { get; }
        private PieSlice Pie { get; }
        private TextBlock MeasureTextBlock { get; }
        private TextBlock LeftLabel { get; }
        private TextBlock RightLabel { get; }
        private bool IsNew { get; set; }
        private bool IsChartInitialized { get; set; }

        public static readonly DependencyProperty Uses360ModeProperty = DependencyProperty.Register(
            "Uses360Mode", typeof (bool), typeof (Gauge), new PropertyMetadata(default(bool), UpdateCallback));
        /// <summary>
        /// Gets or sets whether the gauge uses 360 mode, 360 mode will plot a full circle instead of a semi circle
        /// </summary>
        public bool Uses360Mode
        {
            get { return (bool) GetValue(Uses360ModeProperty); }
            set { SetValue(Uses360ModeProperty, value); }
        }

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From", typeof(double), typeof(Gauge), new PropertyMetadata(0d, UpdateCallback));
        /// <summary>
        /// Gets or sets the value where the gauge starts
        /// </summary>
        public double From
        {
            get { return (double)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To", typeof(double), typeof(Gauge), new PropertyMetadata(1d, UpdateCallback));
        /// <summary>
        /// Gets or sets the value where the gauge ends
        /// </summary>
        public double To
        {
            get { return (double)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof (double), typeof (Gauge), new PropertyMetadata(default(double), UpdateCallback));
        /// <summary>
        /// Gets or sets the current value of the gauge
        /// </summary>
        public double Value
        {
            get { return (double) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register(
            "InnerRadius", typeof (double), typeof (Gauge), new PropertyMetadata(double.NaN, UpdateCallback));
        /// <summary>
        /// Gets o sets inner radius
        /// </summary>
        public double InnerRadius
        {
            get { return (double) GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(Gauge), new PropertyMetadata(default(Brush)));
        /// <summary>
        /// Gets or sets stroke, the stroke is the brush used to draw the gauge border.
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }


        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(Gauge), new PropertyMetadata(default(double)));
        /// <summary>
        /// Gets or sets stroke brush thickness
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty ToColorProperty = DependencyProperty.Register(
            "ToColor", typeof(Color), typeof(Gauge), new PropertyMetadata(default(Color), UpdateCallback));
        /// <summary>
        /// Gets or sets the color when the current value equals to min value, any value between min and max will use an interpolated color.
        /// </summary>
        public Color ToColor
        {
            get { return (Color)GetValue(ToColorProperty); }
            set { SetValue(ToColorProperty, value); }
        }

        public static readonly DependencyProperty FromColorProperty = DependencyProperty.Register(
            "FromColor", typeof(Color), typeof(Gauge), new PropertyMetadata(default(Color), UpdateCallback));
        /// <summary>
        /// Gets or sets the color when the current value equals to max value, any value between min and max will use an interpolated color.
        /// </summary>
        public Color FromColor
        {
            get { return (Color)GetValue(FromColorProperty); }
            set { SetValue(FromColorProperty, value); }
        }

        public static readonly DependencyProperty GaugeBackgroundProperty = DependencyProperty.Register(
            "GaugeBackground", typeof (Brush), typeof (Gauge), new PropertyMetadata(default(Brush)));
        /// <summary>
        /// Gets or sets the gauge background
        /// </summary>
        public Brush GaugeBackground
        {
            get { return (Brush) GetValue(GaugeBackgroundProperty); }
            set { SetValue(GaugeBackgroundProperty, value); }
        }

        public static readonly DependencyProperty AnimationsSpeedProperty = DependencyProperty.Register(
            "AnimationsSpeed", typeof (TimeSpan), typeof (Gauge), new PropertyMetadata(default(TimeSpan)));
        /// <summary>
        /// G3ts or sets the gauge animations speed
        /// </summary>
        public TimeSpan AnimationsSpeed
        {
            get { return (TimeSpan) GetValue(AnimationsSpeedProperty); }
            set { SetValue(AnimationsSpeedProperty, value); }
        }

        public static readonly DependencyProperty LabelFormatterProperty = DependencyProperty.Register(
            "LabelFormatter", typeof (Func<double, string>), typeof (Gauge), new PropertyMetadata(default(Func<double, string>)));
        /// <summary>
        /// Gets or sets the label formatter, a label formatter takes a double value, and return a string, e.g. val => val.ToString("C");
        /// </summary>
        public Func<double, string> LabelFormatter
        {
            get { return (Func<double, string>) GetValue(LabelFormatterProperty); }
            set { SetValue(LabelFormatterProperty, value); }
        }

        public static readonly DependencyProperty HighFontSizeProperty = DependencyProperty.Register(
            "HighFontSize", typeof (double?), typeof (Gauge), new PropertyMetadata(null));
        /// <summary>
        /// Gets o sets the label size, if this value is null then it will be automatically calculated, default is null.
        /// </summary>
        public double? HighFontSize
        {
            get { return (double?) GetValue(HighFontSizeProperty); }
            set { SetValue(HighFontSizeProperty, value); }
        }

        #endregion

        private static void UpdateCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var gauge = (Gauge)dependencyObject;

            gauge.Update();
        }

        private void Update()
        {
            if (!IsChartInitialized) return;

            var ms = new Size(double.PositiveInfinity, double.PositiveInfinity);
            Measure(ms);
            Canvas.Width = Width;
            Canvas.Height = Height;

            Func<double, string> defFormatter = x => x.ToString(CultureInfo.InvariantCulture);

            var completed = (Value-From)/(To - From);

            var t = 0d;

            completed = completed > 1 ? 1 : (completed < 0 ? 0 : completed);
            var angle = Uses360Mode ? 360 : 180;

            if (!Uses360Mode)
            {
                LeftLabel.Text = (LabelFormatter ?? defFormatter)(From);
                RightLabel.Text = (LabelFormatter ?? defFormatter)(To);

                LeftLabel.Visibility = Visibility.Visible;
                RightLabel.Visibility = Visibility.Visible;

                t = LeftLabel.ActualHeight;
            }
            else
            {
                LeftLabel.Visibility = Visibility.Collapsed;//Visibility.Hidden;
                RightLabel.Visibility = Visibility.Collapsed;//Visibility.Hidden;
            }

            double r, top;

            if (Uses360Mode)
            {
                r = ActualWidth > ActualHeight ? ActualHeight: ActualWidth;
                r = r/2 - 2*t ;
                top = ActualHeight/2;
            }
            else
            {
                r = ActualWidth;
                
                if (ActualWidth > ActualHeight*2)
                {
                    r = ActualHeight*2;
                }

                r = r/2 - 2*t;

                top = ActualHeight/2 + r/2;
            }

            PieBack.Height = Canvas.ActualHeight;
            PieBack.Width = Canvas.ActualWidth;
            Pie.Height = Canvas.ActualHeight;
            Pie.Width = Canvas.ActualWidth;

            PieBack.Radius = r;
            PieBack.InnerRadius = double.IsNaN(InnerRadius) ? r*.6 : InnerRadius;
            PieBack.RotationAngle = 270;
            PieBack.WedgeAngle = angle;

            Pie.Radius = PieBack.Radius;
            Pie.InnerRadius = PieBack.InnerRadius;
            Pie.RotationAngle = PieBack.RotationAngle;

            Pie.YOffset = top - ActualHeight/2;
            PieBack.YOffset = top - ActualHeight / 2;

            Canvas.SetTop(LeftLabel, top);
            Canvas.SetTop(RightLabel, top);
            LeftLabel.Measure(ms);
            RightLabel.Measure(ms);
            Canvas.SetLeft(LeftLabel,
                Canvas.ActualWidth*.5 -
                (Pie.InnerRadius + (Pie.Radius - Pie.InnerRadius)*.5 + LeftLabel.DesiredSize.Width*.5));
            Canvas.SetLeft(RightLabel,
                Canvas.ActualWidth*.5 +
                (Pie.InnerRadius + (Pie.Radius - Pie.InnerRadius)*.5 - RightLabel.DesiredSize.Width * .5));

            MeasureTextBlock.FontSize = HighFontSize ?? Pie.InnerRadius*.4;
            MeasureTextBlock.Text = (LabelFormatter ?? defFormatter)(Value);

            MeasureTextBlock.Measure(ms);
            Canvas.SetTop(MeasureTextBlock, top - MeasureTextBlock.DesiredSize.Height*(Uses360Mode ? .5 : 1));
            Canvas.SetLeft(MeasureTextBlock, ActualWidth/2 - MeasureTextBlock.DesiredSize.Width/2);
            
            var interpolatedColor = new Color
            {
                R = LinearInterpolation(FromColor.R, ToColor.R),
                G = LinearInterpolation(FromColor.G, ToColor.G),
                B = LinearInterpolation(FromColor.B, ToColor.B),
                A = LinearInterpolation(FromColor.A, ToColor.A)
            };

            if (IsNew)
            {
                Pie.Fill = new SolidColorBrush(FromColor);
                Pie.WedgeAngle = 0;
            }

            Pie.BeginDoubleAnimation(nameof(PieSlice.WedgeAngle), completed * angle, AnimationsSpeed);
            ((SolidColorBrush)Pie.Fill).BeginColorAnimation(nameof(SolidColorBrush.Color), interpolatedColor, AnimationsSpeed);

            IsNew = false;
        }

        private byte LinearInterpolation(double from, double to)
        {
            var p1 = new Point(From, from);
            var p2 = new Point(To, to);

            var deltaX = p2.X - p1.X;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            var m = (p2.Y - p1.Y) / (deltaX == 0 ? double.MinValue : deltaX);

            var v = Value > To ? To : (Value < From ? From : Value);
            return (byte) (m*(v - p1.X) + p1.Y);
        }
    }
}