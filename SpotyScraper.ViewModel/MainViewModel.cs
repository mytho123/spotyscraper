using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using SpotyScraper.Model.Scrapers;
using SpotyScraper.Model.StreamServices;
using SpotyScraper.Model.Tracks;
using SpotyScraper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            this.ResolveCommand = new RelayCommand<object>(ResolveCommand_Execute, ResolveCommand_CanExecute);
            this.CreatePlaylistCommand = new RelayCommand(CreatePlaylistCommand_Execute, CreatePlaylistCommand_CanExecute);

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
                var oldValue = _selectedScraper;
                this.Set(ref _selectedScraper, value);
                this.ScrapCommand.RaiseCanExecuteChanged();
                this.UpdatePlaylistName(oldValue, value);
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

        private double _scrapProgress;

        public double ScrapProgress
        {
            get { return _scrapProgress; }
            set { this.Set(ref _scrapProgress, value); }
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

        private Track _selectedTrack;

        public Track SelectedTrack
        {
            get { return _selectedTrack; }
            set { this.Set(ref _selectedTrack, value); }
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

        private double _resolveProgress;

        public double ResolveProgress
        {
            get { return _resolveProgress; }
            set { this.Set(ref _resolveProgress, value); }
        }

        private string _playlistName;

        public string PlaylistName
        {
            get { return _playlistName; }
            set { this.Set(ref _playlistName, value); }
        }

        private double _minimumScore = .1;

        public double MinimumScore
        {
            get { return _minimumScore; }
            set { this.Set(ref _minimumScore, value); }
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
                var sw = Stopwatch.StartNew();
                await this.ScrapAsync(scraper);
                Debug.WriteLine($"Scraping took {sw.ElapsedMilliseconds}ms");
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
                var progress = new Progress<double>(x =>
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => this.ScrapProgress = x);
                });

                var tracks = scraper.Scrap(progress)
                    .Distinct(new TrackComparer());
                foreach (var track in tracks)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => this.ScrapedTracks.Add(track));
                }
            });
        }

        public RelayCommand<object> ResolveCommand { get; }

        private bool ResolveCommand_CanExecute(object arg)
        {
            return this.SelectedStreamService != null && !this.IsResolving;
        }

        private async void ResolveCommand_Execute(object arg)
        {
            var service = StreamServicesManager.Instance.GetService(this.SelectedStreamService);
            if (service == null)
                return;

            var track = arg as Track;
            var tracks = track != null ? new Track[] { track } : this.ScrapedTracks.ToArray();

            var progress = new Progress<double>(x =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => this.ResolveProgress = x);
            });

            Track.MinimumScoreToMatch = this.MinimumScore;

            this.IsResolving = true;
            try
            {
                var sw = Stopwatch.StartNew();
                await service.ResolveAsync(tracks, progress);
                Debug.WriteLine($"Resolving took {sw.ElapsedMilliseconds}ms");
            }
            finally
            {
                this.IsResolving = false;
            }
        }

        public RelayCommand CreatePlaylistCommand { get; }

        private bool CreatePlaylistCommand_CanExecute()
        {
            return true;
        }

        private async void CreatePlaylistCommand_Execute()
        {
            var service = StreamServicesManager.Instance.GetService(this.SelectedStreamService);
            if (service == null)
                return;

            var tracks = this.ScrapedTracks
                .Select(x => x.SelectedMatch.Key)
                .Where(x => x != null);
            await service.CreatePlaylist(this.PlaylistName, tracks);
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

        private void UpdatePlaylistName(IScraperData oldValue, IScraperData newValue)
        {
            // if user has modified playlist name, don't overwrite it
            if (oldValue != null)
            {
                var oldName = this.GetPlaylistName(oldValue);
                if (this.PlaylistName != oldName)
                    return;
            }

            this.PlaylistName = this.GetPlaylistName(newValue);
        }

        private string GetPlaylistName(IScraperData scraper)
        {
            return $"SpotyScraper - {scraper?.Name}";
        }
    }
}