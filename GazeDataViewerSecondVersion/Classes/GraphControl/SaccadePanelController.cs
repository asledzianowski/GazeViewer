using GazeDataViewer.Classes.Saccade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GazeDataViewer.Classes.GraphControl
{
    public class SaccadePanelController
    {
        //public static void ApplySaccadeControls(UniformGrid saccadeControlsPanel, List<EyeMove> saccades)
        //{
        //    saccadeControlsPanel.Children.Clear();

        //    foreach (var saccade in saccades)
        //    {
        //        var panel = GetSaccadeControl(saccade);
        //        saccadeControlsPanel.Children.Add(panel);
        //    }
        //}

        //public static StackPanel GetSaccadeControl(EyeMove saccade)
        //{
        //    var itemPanel = new StackPanel();
        //    itemPanel.Orientation = Orientation.Horizontal;
        //    itemPanel.HorizontalAlignment = HorizontalAlignment.Right;
        //    itemPanel.Margin = new Thickness(0, 10, 0, 0);
        //    itemPanel.Tag = saccade.Id.ToString();
        //    itemPanel.Name = "SaccPanel";

        //    var saccLabel = new Label();
        //    saccLabel.Content = $"#{saccade.Id}";

        //    var controlPadding = new Thickness(2, 5, 2, 5);

        //    var saccStartTB = new TextBox();
        //    saccStartTB.Text = saccade.EyeStartIndex.ToString();
        //    saccStartTB.Name = "TBEyeMoveStart";
        //    saccStartTB.Padding = controlPadding;
        //    saccStartTB.VerticalAlignment = VerticalAlignment.Center;
        //    saccStartTB.PreviewTextInput += NumberValidationTextBox;

        //    var saccEndTB = new TextBox();
        //    saccEndTB.Text = saccade.EyeEndIndex.ToString();
        //    saccEndTB.Name = "TBEyeMoveEnd";
        //    saccEndTB.Padding = controlPadding;
        //    saccEndTB.VerticalAlignment = VerticalAlignment.Center;
        //    saccEndTB.PreviewTextInput += NumberValidationTextBox;

        //    var removeButton = new Button();
        //    removeButton.Content = "X";
        //    //removeButton.F = FontWeights.Bold;
        //    removeButton.Tag = saccade.Id;
        //    removeButton.VerticalAlignment = VerticalAlignment.Center;
        //    removeButton.Padding = controlPadding;
        //    removeButton.Click += BtnRemoveEyeMove_Click;

        //    itemPanel.Children.Add(saccLabel);
        //    itemPanel.Children.Add(saccStartTB);
        //    itemPanel.Children.Add(saccEndTB);
        //    itemPanel.Children.Add(removeButton);

        //    return itemPanel;
        //}

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Equals("-"))
            {
                e.Handled = false;
            }
            else
            {
                var regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }
    }
}
