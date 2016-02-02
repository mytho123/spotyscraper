using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SpotyScraper.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherHelper.Initialize();

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.OnUnhandledException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.OnUnhandledException(e.ExceptionObject as Exception);
        }

        private void OnUnhandledException(Exception exception)
        {
            MessageBox.Show(
                $"An error occured:{Environment.NewLine}{exception.GetBaseException().GetType().Name}: {exception.GetBaseException().Message}",
                "Oops, an error occured",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}