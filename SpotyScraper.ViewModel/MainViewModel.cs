﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using SpotyScraper.Model.Scrapers;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
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
            this.ScrapCommand = new RelayCommand(ScrapCommand_Execute, ScrapCommand_CanExecute);
            this.ResolveCommand = new RelayCommand(ResolveCommand_Execute, ResolveCommand_CanExecute);

            this.InitScrapers();
            this.InitStreamServices();
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

        private bool _isScraping;

        public bool IsScraping
        {
            get { return _isScraping; }
            set
            {
                this.Set(ref _isScraping, value);
                this.ScrapCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Track> ScrapedTracks { get; } = new ObservableCollection<Track>();

        public ObservableCollection<IStreamServiceData> StreamServices { get; } = new ObservableCollection<IStreamServiceData>();

        private IStreamServiceData _selectedStreamService;

        public IStreamServiceData SelectedStreamService
        {
            get { return _selectedStreamService; }
            set
            {
                this.Set(ref _selectedStreamService, value);
                this.ResolveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isResolving;

        public bool IsResolving
        {
            get { return _isResolving; }
            set
            {
                this.Set(ref _isResolving, value);
                this.ResolveCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion props

        #region Commands

        public RelayCommand ScrapCommand { get; }

        private bool ScrapCommand_CanExecute()
        {
            return this.SelectedScraper != null && !this.IsScraping;
        }

        private async void ScrapCommand_Execute()
        {
            var scraper = ScrapersManager.Instance.GetScraper(this.SelectedScraper);
            if (scraper == null)
                return;

            this.IsScraping = true;
            try
            {
                await this.ScrapAsync(scraper);
            }
            finally
            {
                this.IsScraping = false;
            }
        }

        private async Task ScrapAsync(IScraper scraper)
        {
            await Task.Run(() =>
            {
                foreach (var track in scraper.Scrap())
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => this.ScrapedTracks.Add(track));
                }
            });
        }

        public RelayCommand ResolveCommand { get; }

        private bool ResolveCommand_CanExecute()
        {
            return this.SelectedStreamService != null && !this.IsResolving;
        }

        private async void ResolveCommand_Execute()
        {
            var service = StreamServicesManager.Instance.GetService(this.SelectedStreamService);
            if (service == null)
                return;

            this.IsResolving = true;
            try
            {
                await service.ResolveAsync(this.ScrapedTracks);
            }
            finally
            {
                this.IsResolving = false;
            }
        }

        #endregion Commands

        private void InitScrapers()
        {
            this.Scrapers.AddRange(ScrapersManager.Instance.GetScrapersData());
            this.SelectedScraper = this.Scrapers.FirstOrDefault();
        }

        private void InitStreamServices()
        {
            this.StreamServices.AddRange(StreamServicesManager.Instance.GetServicesData());
            this.SelectedStreamService = this.StreamServices.FirstOrDefault();
        }
    }
}