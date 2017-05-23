﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7C8C027BDCDE641F23F0C7D8477DDB6E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DynamicDataDisplay.Markers;
using DynamicDataDisplay.Markers.MarkerGenerators;
using GazeDataViewer;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using Microsoft.Research.DynamicDataDisplay.Charts.Isolines;
using Microsoft.Research.DynamicDataDisplay.Charts.Maps;
using Microsoft.Research.DynamicDataDisplay.Charts.Maps.Network;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.Charts.NewLine;
using Microsoft.Research.DynamicDataDisplay.Charts.Selectors;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;
using Microsoft.Research.DynamicDataDisplay.Converters;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Maps.Charts;
using Microsoft.Research.DynamicDataDisplay.Maps.Charts.TiledRendering;
using Microsoft.Research.DynamicDataDisplay.Maps.Charts.VectorFields.Streamlines;
using Microsoft.Research.DynamicDataDisplay.Maps.DeepZoom;
using Microsoft.Research.DynamicDataDisplay.Maps.Servers;
using Microsoft.Research.DynamicDataDisplay.Maps.Servers.FileServers;
using Microsoft.Research.DynamicDataDisplay.Maps.Servers.Network;
using Microsoft.Research.DynamicDataDisplay.Markers.MarkerGenerators.Rendering;
using Microsoft.Research.DynamicDataDisplay.MarkupExtensions;
using Microsoft.Research.DynamicDataDisplay.Navigation;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.ViewportConstraints;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace GazeDataViewer {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 16 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer LayoutRoot;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ScreenContentGrid;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid HeaderGrid;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBDistanceFromScreen;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBFrequency;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBTimestampColumnIndex;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBEyeColumnIndex;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSpotColumnIndex;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LoadDataPathTextBox;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button LoadDataButton;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Research.DynamicDataDisplay.ChartPlotter amplitudePlotter;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Research.DynamicDataDisplay.Charts.Axes.HorizontalIntegerAxis timeAxis;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Research.DynamicDataDisplay.Charts.VerticalAxis valueAxis;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ControlGrid;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBTextOutput;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LogTextBox;
        
        #line default
        #line hidden
        
        
        #line 109 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBGraph;
        
        #line default
        #line hidden
        
        
        #line 122 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CBShowLabels;
        
        #line default
        #line hidden
        
        
        #line 125 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CBFixView;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBResults;
        
        #line default
        #line hidden
        
        
        #line 145 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSaccadeCalc;
        
        #line default
        #line hidden
        
        
        #line 147 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnClearResults;
        
        #line default
        #line hidden
        
        
        #line 149 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSaveResults;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnScreenShot;
        
        #line default
        #line hidden
        
        
        #line 161 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBEyeMoves;
        
        #line default
        #line hidden
        
        
        #line 174 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnAddEyeMove;
        
        #line default
        #line hidden
        
        
        #line 176 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnApply;
        
        #line default
        #line hidden
        
        
        #line 179 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.UniformGrid SaccadeControlsPanel;
        
        #line default
        #line hidden
        
        
        #line 198 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBFilters;
        
        #line default
        #line hidden
        
        
        #line 223 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CBFilterButterworth;
        
        #line default
        #line hidden
        
        
        #line 224 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComboFilterTypeButterworth;
        
        #line default
        #line hidden
        
        
        #line 228 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBButterWorthFrequency;
        
        #line default
        #line hidden
        
        
        #line 231 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBButterWorthSampleRate;
        
        #line default
        #line hidden
        
        
        #line 234 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBButterWorthResonance;
        
        #line default
        #line hidden
        
        
        #line 239 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CBFilterSavGolay;
        
        #line default
        #line hidden
        
        
        #line 242 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSavGoayPointsNum;
        
        #line default
        #line hidden
        
        
        #line 245 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSavGoayDerivOrder;
        
        #line default
        #line hidden
        
        
        #line 248 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSavGoayPolyOrder;
        
        #line default
        #line hidden
        
        
        #line 253 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GroupBox GBPredictions;
        
        #line default
        #line hidden
        
        
        #line 275 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBStartRec;
        
        #line default
        #line hidden
        
        
        #line 280 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBEndRec;
        
        #line default
        #line hidden
        
        
        #line 286 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBEyeShiftPeroid;
        
        #line default
        #line hidden
        
        
        #line 290 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSpotShiftPeroid;
        
        #line default
        #line hidden
        
        
        #line 296 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSaccadeStartShiftPeroid;
        
        #line default
        #line hidden
        
        
        #line 300 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSaccadeEndShiftPeroid;
        
        #line default
        #line hidden
        
        
        #line 304 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBEyeAmpProp;
        
        #line default
        #line hidden
        
        
        #line 308 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBSpotAmpProp;
        
        #line default
        #line hidden
        
        
        #line 312 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TBReduceEyeSpotAmpDiff;
        
        #line default
        #line hidden
        
        
        #line 314 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCalculate;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GazeDataViewer;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.LayoutRoot = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 2:
            this.ScreenContentGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.HeaderGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.TBDistanceFromScreen = ((System.Windows.Controls.TextBox)(target));
            
            #line 39 "..\..\MainWindow.xaml"
            this.TBDistanceFromScreen.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 5:
            this.TBFrequency = ((System.Windows.Controls.TextBox)(target));
            
            #line 43 "..\..\MainWindow.xaml"
            this.TBFrequency.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 6:
            this.TBTimestampColumnIndex = ((System.Windows.Controls.TextBox)(target));
            
            #line 47 "..\..\MainWindow.xaml"
            this.TBTimestampColumnIndex.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 7:
            this.TBEyeColumnIndex = ((System.Windows.Controls.TextBox)(target));
            
            #line 51 "..\..\MainWindow.xaml"
            this.TBEyeColumnIndex.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 8:
            this.TBSpotColumnIndex = ((System.Windows.Controls.TextBox)(target));
            
            #line 55 "..\..\MainWindow.xaml"
            this.TBSpotColumnIndex.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 9:
            this.LoadDataPathTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.LoadDataButton = ((System.Windows.Controls.Button)(target));
            
            #line 62 "..\..\MainWindow.xaml"
            this.LoadDataButton.Click += new System.Windows.RoutedEventHandler(this.LoadDataButton_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.amplitudePlotter = ((Microsoft.Research.DynamicDataDisplay.ChartPlotter)(target));
            return;
            case 12:
            this.timeAxis = ((Microsoft.Research.DynamicDataDisplay.Charts.Axes.HorizontalIntegerAxis)(target));
            return;
            case 13:
            this.valueAxis = ((Microsoft.Research.DynamicDataDisplay.Charts.VerticalAxis)(target));
            return;
            case 14:
            this.ControlGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 15:
            this.GBTextOutput = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 16:
            this.LogTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 17:
            this.GBGraph = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 18:
            this.CBShowLabels = ((System.Windows.Controls.CheckBox)(target));
            
            #line 123 "..\..\MainWindow.xaml"
            this.CBShowLabels.Click += new System.Windows.RoutedEventHandler(this.CBShowLabels_Click);
            
            #line default
            #line hidden
            return;
            case 19:
            this.CBFixView = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 20:
            this.GBResults = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 21:
            this.BtnSaccadeCalc = ((System.Windows.Controls.Button)(target));
            
            #line 146 "..\..\MainWindow.xaml"
            this.BtnSaccadeCalc.Click += new System.Windows.RoutedEventHandler(this.BtnSaccadeCalc_Click);
            
            #line default
            #line hidden
            return;
            case 22:
            this.BtnClearResults = ((System.Windows.Controls.Button)(target));
            
            #line 148 "..\..\MainWindow.xaml"
            this.BtnClearResults.Click += new System.Windows.RoutedEventHandler(this.BtnClearResults_Click);
            
            #line default
            #line hidden
            return;
            case 23:
            this.BtnSaveResults = ((System.Windows.Controls.Button)(target));
            
            #line 150 "..\..\MainWindow.xaml"
            this.BtnSaveResults.Click += new System.Windows.RoutedEventHandler(this.BtnSaveResults_Click);
            
            #line default
            #line hidden
            return;
            case 24:
            this.BtnScreenShot = ((System.Windows.Controls.Button)(target));
            
            #line 152 "..\..\MainWindow.xaml"
            this.BtnScreenShot.Click += new System.Windows.RoutedEventHandler(this.BtnSaveScreenShot_Click);
            
            #line default
            #line hidden
            return;
            case 25:
            this.GBEyeMoves = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 26:
            this.BtnAddEyeMove = ((System.Windows.Controls.Button)(target));
            
            #line 175 "..\..\MainWindow.xaml"
            this.BtnAddEyeMove.Click += new System.Windows.RoutedEventHandler(this.BtnAddEyeMove_Click);
            
            #line default
            #line hidden
            return;
            case 27:
            this.BtnApply = ((System.Windows.Controls.Button)(target));
            
            #line 177 "..\..\MainWindow.xaml"
            this.BtnApply.Click += new System.Windows.RoutedEventHandler(this.BtnApply_Click);
            
            #line default
            #line hidden
            return;
            case 28:
            this.SaccadeControlsPanel = ((System.Windows.Controls.Primitives.UniformGrid)(target));
            return;
            case 29:
            this.GBFilters = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 30:
            this.CBFilterButterworth = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 31:
            this.ComboFilterTypeButterworth = ((System.Windows.Controls.ComboBox)(target));
            
            #line 224 "..\..\MainWindow.xaml"
            this.ComboFilterTypeButterworth.Loaded += new System.Windows.RoutedEventHandler(this.ComboBox_Loaded);
            
            #line default
            #line hidden
            return;
            case 32:
            this.TBButterWorthFrequency = ((System.Windows.Controls.TextBox)(target));
            
            #line 227 "..\..\MainWindow.xaml"
            this.TBButterWorthFrequency.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.DecimalValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 33:
            this.TBButterWorthSampleRate = ((System.Windows.Controls.TextBox)(target));
            
            #line 230 "..\..\MainWindow.xaml"
            this.TBButterWorthSampleRate.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 34:
            this.TBButterWorthResonance = ((System.Windows.Controls.TextBox)(target));
            
            #line 233 "..\..\MainWindow.xaml"
            this.TBButterWorthResonance.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.DecimalValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 35:
            this.CBFilterSavGolay = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 36:
            this.TBSavGoayPointsNum = ((System.Windows.Controls.TextBox)(target));
            
            #line 241 "..\..\MainWindow.xaml"
            this.TBSavGoayPointsNum.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 37:
            this.TBSavGoayDerivOrder = ((System.Windows.Controls.TextBox)(target));
            
            #line 244 "..\..\MainWindow.xaml"
            this.TBSavGoayDerivOrder.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 38:
            this.TBSavGoayPolyOrder = ((System.Windows.Controls.TextBox)(target));
            
            #line 247 "..\..\MainWindow.xaml"
            this.TBSavGoayPolyOrder.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 39:
            this.GBPredictions = ((System.Windows.Controls.GroupBox)(target));
            return;
            case 40:
            this.TBStartRec = ((System.Windows.Controls.TextBox)(target));
            
            #line 274 "..\..\MainWindow.xaml"
            this.TBStartRec.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 41:
            this.TBEndRec = ((System.Windows.Controls.TextBox)(target));
            
            #line 279 "..\..\MainWindow.xaml"
            this.TBEndRec.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 42:
            this.TBEyeShiftPeroid = ((System.Windows.Controls.TextBox)(target));
            
            #line 285 "..\..\MainWindow.xaml"
            this.TBEyeShiftPeroid.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 43:
            this.TBSpotShiftPeroid = ((System.Windows.Controls.TextBox)(target));
            
            #line 289 "..\..\MainWindow.xaml"
            this.TBSpotShiftPeroid.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 44:
            this.TBSaccadeStartShiftPeroid = ((System.Windows.Controls.TextBox)(target));
            
            #line 295 "..\..\MainWindow.xaml"
            this.TBSaccadeStartShiftPeroid.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 45:
            this.TBSaccadeEndShiftPeroid = ((System.Windows.Controls.TextBox)(target));
            
            #line 299 "..\..\MainWindow.xaml"
            this.TBSaccadeEndShiftPeroid.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 46:
            this.TBEyeAmpProp = ((System.Windows.Controls.TextBox)(target));
            
            #line 303 "..\..\MainWindow.xaml"
            this.TBEyeAmpProp.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.DecimalValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 47:
            this.TBSpotAmpProp = ((System.Windows.Controls.TextBox)(target));
            
            #line 307 "..\..\MainWindow.xaml"
            this.TBSpotAmpProp.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.DecimalValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 48:
            this.TBReduceEyeSpotAmpDiff = ((System.Windows.Controls.TextBox)(target));
            
            #line 311 "..\..\MainWindow.xaml"
            this.TBReduceEyeSpotAmpDiff.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.NumberValidationTextBox);
            
            #line default
            #line hidden
            return;
            case 49:
            this.BtnCalculate = ((System.Windows.Controls.Button)(target));
            
            #line 315 "..\..\MainWindow.xaml"
            this.BtnCalculate.Click += new System.Windows.RoutedEventHandler(this.ButtonCalculate_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

