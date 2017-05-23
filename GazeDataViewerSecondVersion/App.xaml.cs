using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace GazeDataViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string exceptionMessage = e.Exception.InnerException != null ? e.Exception.InnerException.Message : e.Exception.Message;

            MessageBox.Show("Exception : " + exceptionMessage, "Exception Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
