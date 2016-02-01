using GalaSoft.MvvmLight.Command;
using SpotyScraper.Model.Scrapers;
using SpotyScraper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotyScraper.ViewModel
{
    public class MainViewModel : BaseVM
    {
        public MainViewModel()
        {
            this.InitScrapers();

            this.ScrapCommand = new RelayCommand(ScrapCommand_Execute, ScrapCommand_CanExecute)
        }

        #region props

        public ObservableCollection<IScraperData> Scrapers { get; } = new ObservableCollection<IScraperData>();

        private IScraperData _selectedScraper;

        public IScraperData SelectedScraper
        {
            get { return _selectedScraper; }
            set
            {
                this.Set(ref _selectedScraper, value);
                this.ScrapCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ScrapCommand { get; }

        #endregion props

        private void InitScrapers()
        {
            this.Scrapers.AddRange(ScrapersManager.Instance.GetScrapersData());
            this.SelectedScraper = this.Scrapers.FirstOrDefault();
        }

        private bool ScrapCommand_CanExecute()
        {
            return this.SelectedScraper != null;
        }

        private void ScrapCommand_Execute()
        {
            var scraper = ScrapersManager.Instance.GetScraper(this.SelectedScraper);
            if (scraper == null)
                return;
        }
    }
}