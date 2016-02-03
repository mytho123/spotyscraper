using SpotyScraper.Model.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpotyScraper.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);
        }

        private void RowDetails_DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // HACK : workaround selected item binding not working
            var dataGrid = sender as DataGrid;
            var track = dataGrid?.DataContext as Track;
            if (track == null)
                return;

            dataGrid.SelectedItem = track.SelectedMatch;
        }

        private void RowDetails_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // HACK : workaround selected item binding not working
            var dataGrid = sender as DataGrid;
            var track = dataGrid?.DataContext as Track;
            if (track == null)
                return;

            if (dataGrid.SelectedItem is KeyValuePair<ITrackMatch, double>)
                track.SelectedMatch = (KeyValuePair<ITrackMatch, double>)dataGrid.SelectedItem;
        }
    }
}