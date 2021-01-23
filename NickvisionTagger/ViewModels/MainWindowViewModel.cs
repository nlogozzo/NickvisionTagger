/****
* "MainWindowViewModel.cs" - The ViewModel for MainWindow
* Copyright (C) 2021 Nicholas Logozzo
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, see <http://www.gnu.org/licenses/>.
****/

using ModernWpf.Controls;
using Nickvision.MVVM;
using Nickvision.MVVM.Commands;
using Nickvision.MVVM.Services;
using Nickvision.Update;
using NickvisionTagger.Extensions;
using NickvisionTagger.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace NickvisionTagger.ViewModels
{
    /// <summary>
    /// The ViewModel for MainWindow
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private IContentDialogService _contentDialogService;
        private INotificationService _notificationService;
        private IIODialogService _ioDialogService;
        private IComboBoxDialogService _comboBoxDialogService;
        private MusicFolder _musicFolder;
        private string _tagFilename;
        private string _tagTitle;
        private string _tagArtist;
        private string _tagAlbum;
        private uint _tagYear;
        private uint _tagTrack;
        private string _tagAlbumArtist;
        private string _tagGenre;
        private string _tagComment;
        private string _tagDuration;
        private string _tagFileSize;
        private bool _tagFilenameEnabled;
        private bool _tagYearEnabled;
        private bool _tagTrackEnabled;
        private List<MusicFile> _selectedFiles;

        public ObservableCollection<MusicFile> AllMusicFiles => new ObservableCollection<MusicFile>(_musicFolder.Files);
        public DelegateCommand<object> OpenMusicFolderCommand { get; private set; }
        public DelegateCommand<object> CloseMusicFolderCommand { get; private set; }
        public DelegateCommand<object> ReloadMusicFolderCommand { get; private set; }
        public DelegateCommand<object> ExitCommand { get; private set; }
        public DelegateAsyncCommand<object> SaveTagCommand { get; private set; }
        public DelegateAsyncCommand<object> RemoveTagCommand { get; private set; }
        public DelegateAsyncCommand<object> FilenameToTagCommand { get; private set; }
        public DelegateAsyncCommand<object> TagToFilenameCommand { get; private set; }
        public DelegateCommand<IList> SelectedFilesCommand { get; private set; }
        public DelegateAsyncCommand<object> CheckForUpdatesCommand { get; private set; }
        public DelegateCommand<object> ReportABugCommand { get; private set; }
        public DelegateAsyncCommand<object> ChangelogCommand { get; private set; }
        public DelegateAsyncCommand<object> AboutCommand { get; private set; }

        /// <summary>
        /// Constructs the viewmodel
        /// </summary>
        /// <param name="messageBoxService">The IMessageBoxService</param>
        /// <param name="notificationService">The INotificationService</param>
        /// <param name="ioDialogService">The IIODialogService</param>
        public MainWindowViewModel(IContentDialogService contentDialogService, INotificationService notificationService, IIODialogService ioDialogService, IComboBoxDialogService comboBoxDialogService)
        {
            Title = "Nickvision Tagger";
            ControlzEx.Theming.ThemeManager.Current.ChangeThemeColorScheme(Application.Current, "Orange");
            _contentDialogService = contentDialogService;
            _notificationService = notificationService;
            _ioDialogService = ioDialogService;
            _comboBoxDialogService = comboBoxDialogService;
            _musicFolder = new MusicFolder("", IncludeSubfolders);
            _selectedFiles = null;
            OpenMusicFolderCommand = new DelegateCommand<object>(OpenMusicFolder);
            CloseMusicFolderCommand = new DelegateCommand<object>(CloseMusicFolder);
            ReloadMusicFolderCommand = new DelegateCommand<object>(ReloadMusicFolder);
            ExitCommand = new DelegateCommand<object>(Exit);
            SaveTagCommand = new DelegateAsyncCommand<object>(SaveTag);
            RemoveTagCommand = new DelegateAsyncCommand<object>(RemoveTag);
            FilenameToTagCommand = new DelegateAsyncCommand<object>(FilenameToTag);
            TagToFilenameCommand = new DelegateAsyncCommand<object>(TagToFilename);
            SelectedFilesCommand = new DelegateCommand<IList>(SelectedFiles);
            CheckForUpdatesCommand = new DelegateAsyncCommand<object>(CheckForUpdates);
            ReportABugCommand = new DelegateCommand<object>(ReportABug);
            ChangelogCommand = new DelegateAsyncCommand<object>(Changelog);
            AboutCommand = new DelegateAsyncCommand<object>(About);
            TagDuration = "Duration\n00:00:00";
            TagFileSize = "File Size:\n0 MB";
            TagFilenameEnabled = true;
            TagYearEnabled = true;
            TagTrackEnabled = true;
            LoadConfig();
        }

        public bool IncludeSubfolders
        {
            get => _musicFolder == null ? true : _musicFolder.IncludeSubfolders;

            set
            {
                _musicFolder.IncludeSubfolders = value;
                _musicFolder.ReloadFiles();
                OnPropertyChanged();
                OnPropertyChanged("AllMusicFiles");
                UpdateConfig();
            }
        }

        public string CurrentMusicFolder
        {
            get => _musicFolder.FolderPath;

            set
            {
                _musicFolder.FolderPath = value == "No Folder Open" ? "" : value;
                _musicFolder.ReloadFiles();
                OnPropertyChanged();
                OnPropertyChanged("AllMusicFiles");
                UpdateConfig();
            }
        }

        public string TagFilename
        {
            get => _tagFilename;

            set => SetProperty(ref _tagFilename, value);
        }

        public string TagTitle
        {
            get => _tagTitle;

            set => SetProperty(ref _tagTitle, value);
        }

        public string TagArtist
        {
            get => _tagArtist;

            set => SetProperty(ref _tagArtist, value);
        }

        public string TagAlbum
        {
            get => _tagAlbum;

            set => SetProperty(ref _tagAlbum, value);
        }

        public double TagYear
        {
            get => _tagYear;

            set => SetProperty(ref _tagYear, (uint)value);
        }

        public double TagTrack
        {
            get => _tagTrack;

            set => SetProperty(ref _tagTrack, (uint)value);
        }

        public string TagAlbumArtist
        {
            get => _tagAlbumArtist;

            set => SetProperty(ref _tagAlbumArtist, value);
        }

        public string TagGenre
        {
            get => _tagGenre;

            set => SetProperty(ref _tagGenre, value);
        }

        public string TagComment
        {
            get => _tagComment;

            set => SetProperty(ref _tagComment, value);
        }

        public string TagDuration
        {
            get => _tagDuration;

            set => SetProperty(ref _tagDuration, value);
        }

        public string TagFileSize
        {
            get => _tagFileSize;

            set => SetProperty(ref _tagFileSize, value);
        }

        public bool TagFilenameEnabled
        {
            get => _tagFilenameEnabled;

            set => SetProperty(ref _tagFilenameEnabled, value);
        }

        public bool TagYearEnabled
        {
            get => _tagYearEnabled;

            set => SetProperty(ref _tagYearEnabled, value);
        }

        public bool TagTrackEnabled
        {
            get => _tagTrackEnabled;

            set => SetProperty(ref _tagTrackEnabled, value);
        }

        public bool IsLightTheme
        {
            get => ModernWpf.ThemeManager.Current.ApplicationTheme == ModernWpf.ApplicationTheme.Light;

            set
            {
                if (value == true)
                {
                    ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Light;
                    ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Light");
                    IsDarkTheme = false;
                    UpdateConfig();
                }
                OnPropertyChanged();
            }
        }

        public bool IsDarkTheme
        {
            get => ModernWpf.ThemeManager.Current.ApplicationTheme == ModernWpf.ApplicationTheme.Dark;

            set
            {
                if (value == true)
                {
                    ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
                    ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, "Dark");
                    IsLightTheme = false;
                    UpdateConfig();
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads the configuration file and applies its preferences
        /// </summary>
        private void LoadConfig()
        {
            var config = Config.LoadConfig();
            if (config.IsLightTheme)
            {
                IsLightTheme = true;
            }
            else
            {
                IsDarkTheme = true;
            }
            CurrentMusicFolder = Directory.Exists(config.PreviousMusicFolderPath) ? config.PreviousMusicFolderPath : "No Folder Open";
            IncludeSubfolders = config.IncludeSubfolders;
        }

        /// <summary>
        /// Updates the configuration file based on the current application's settings
        /// </summary>
        private void UpdateConfig() => Config.SaveConfig(new Config(IsLightTheme, CurrentMusicFolder, IncludeSubfolders));

        /// <summary>
        /// Asks the user to select a folder that contains the music files to tag
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenMusicFolder(object parameter)
        {
            var musicFolder = _ioDialogService.OpenFolderDialog("Open Music Folder");
            if(musicFolder != null)
            {
                CurrentMusicFolder = musicFolder;
            }
        }

        /// <summary>
        /// Closes the music folder, if one is open
        /// </summary>
        /// <param name="parameter"></param>
        public void CloseMusicFolder(object parameter)
        {
            CurrentMusicFolder = "No Folder Open";
        }

        /// <summary>
        /// Reloads the music folder, if one is open
        /// </summary>
        /// <param name="parameter"></param>
        public void ReloadMusicFolder(object parameter)
        {
            _musicFolder.ReloadFiles();
            OnPropertyChanged("AllMusicFiles");
        }

        /// <summary>
        /// Closes the program
        /// </summary>
        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Saves the tags of the selected music files
        /// </summary>
        /// <param name="parameter"></param>
        public async Task SaveTag(object parameter)
        {
            if (_selectedFiles != null)
            {
                foreach (var musicFile in _selectedFiles)
                {
                    if (string.IsNullOrEmpty(TagFilename))
                    {
                        continue;
                    }
                    if (musicFile.Filename != TagFilename && TagFilenameEnabled)
                    {
                        musicFile.Filename = TagFilename;
                    }
                    musicFile.Title = TagTitle == "<keep>" ? musicFile.Title : TagTitle;
                    musicFile.Artist = TagArtist == "<keep>" ? musicFile.Artist : TagArtist;
                    musicFile.Album = TagAlbum == "<keep>" ? musicFile.Album : TagAlbum;
                    musicFile.Year = !TagYearEnabled ? musicFile.Year : (uint)TagYear;
                    musicFile.Track = !TagTrackEnabled ? musicFile.Track : (uint)TagTrack;
                    musicFile.AlbumArtist = TagAlbumArtist == "<keep>" ? musicFile.AlbumArtist : TagAlbumArtist;
                    musicFile.Genre = TagGenre == "<keep>" ? musicFile.Genre : TagGenre;
                    musicFile.Comment = TagComment == "<keep>" ? musicFile.Comment : TagComment;
                    await Task.Run(() => musicFile.Save());
                }
                ReloadMusicFolder(null);
            }
        }

        /// <summary>
        /// Removes the tags of the selected music files
        /// </summary>
        /// <param name="parameter"></param>
        public async Task RemoveTag(object parameter)
        {
            if (_selectedFiles != null && _selectedFiles.Count != 0)
            {
                var result = await _contentDialogService.ShowAsync($"Are you sure you want to remove {_selectedFiles.Count} tag(s)?", "Remove Tag(s)?", "No", "Yes");
                if (result == ContentDialogResult.Primary)
                {
                    foreach (var musicFile in _selectedFiles)
                    {
                        await Task.Run(() => musicFile.RemoveTag());
                    }
                    ReloadMusicFolder(null);
                }
            }
        }

        public async Task FilenameToTag(object parameter)
        {
            if(_selectedFiles != null && _selectedFiles.Count != 0)
            {
                var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%title%" };
                var result = await _comboBoxDialogService.ShowAsync("Select a format string", "Filename To Tag", formatStrings);
                if(result.SelectedItem != null)
                {
                    foreach (var musicFile in _selectedFiles)
                    {
                        await Task.Run(() => musicFile.FilenameToTag(result.SelectedItem));
                    }
                    ReloadMusicFolder(null);
                }
            }
        }

        public async Task TagToFilename(object parameter)
        {
            if (_selectedFiles != null && _selectedFiles.Count != 0)
            {
                var formatStrings = new List<string>() { "%artist%- %title%", "%title%- %artist%", "%title%" };
                var result = await _comboBoxDialogService.ShowAsync("Select a format string", "Tag To Filename", formatStrings);
                if (result.SelectedItem != null)
                {
                    foreach (var musicFile in _selectedFiles)
                    {
                        await Task.Run(() => musicFile.TagToFilename(result.SelectedItem));
                    }
                    ReloadMusicFolder(null);
                }
            }
        }

        /// <summary>
        /// Updates the properties editor based on the selected music files
        /// </summary>
        /// <param name="parameter"></param>
        public void SelectedFiles(IList selectedItems)
        {
            _selectedFiles = selectedItems.Cast<MusicFile>().ToList();
            TagFilenameEnabled = true;
            TagYearEnabled = true;
            TagTrackEnabled = true;
            if (_selectedFiles.Count == 0)
            {
                TagFilename = "";
                TagTitle = "";
                TagArtist = "";
                TagAlbum = "";
                TagYear = 0;
                TagTrack = 0;
                TagAlbumArtist = "";
                TagGenre = "";
                TagComment = "";
                TagDuration = "Duration\n00:00:00";
                TagFileSize = "File Size:\n0 MB";
            }
            else if (_selectedFiles.Count == 1)
            {
                var musicFile = _selectedFiles[0];
                TagFilename = musicFile.Filename;
                TagTitle = musicFile.Title;
                TagArtist = musicFile.Artist;
                TagAlbum = musicFile.Album;
                TagYear = musicFile.Year;
                TagTrack = musicFile.Track;
                TagAlbumArtist = musicFile.AlbumArtist;
                TagGenre = musicFile.Genre;
                TagComment = musicFile.Comment;
                TagDuration = $"Duration:\n{musicFile.DurationAsString}";
                TagFileSize = $"File Size:\n{musicFile.FileSizeAsString}";
            }
            else
            {
                var firstMusicFile = _selectedFiles[0];
                bool haveSameTitle = true;
                bool haveSameArtist = true;
                bool haveSameAlbum = true;
                bool haveSameYear = true;
                bool haveSameTrack = true;
                bool haveSameAlbumArtist = true;
                bool haveSameGenre = true;
                bool haveSameComment = true;
                var totalDuration = new TimeSpan();
                long totalFileSize = 0;
                foreach (var musicFile in _selectedFiles)
                {
                    if (firstMusicFile.Title != musicFile.Title)
                    {
                        haveSameTitle = false;
                    }
                    if (firstMusicFile.Artist != musicFile.Artist)
                    {
                        haveSameArtist = false;
                    }
                    if (firstMusicFile.Album != musicFile.Album)
                    {
                        haveSameAlbum = false;
                    }
                    if (firstMusicFile.Year != musicFile.Year)
                    {
                        haveSameYear = false;
                    }
                    if (firstMusicFile.Track != musicFile.Track)
                    {
                        haveSameTrack = false;
                    }
                    if (firstMusicFile.AlbumArtist != musicFile.AlbumArtist)
                    {
                        haveSameAlbumArtist = false;
                    }
                    if (firstMusicFile.Genre != musicFile.Genre)
                    {
                        haveSameGenre = false;
                    }
                    if (firstMusicFile.Comment != musicFile.Comment)
                    {
                        haveSameComment = false;
                    }
                    totalDuration += musicFile.Duration;
                    totalFileSize += musicFile.FileSize;
                }
                var totalDurationAsString = totalDuration.DurationToString();
                var totalFileSizeAsString = totalFileSize.FileSizeToString();
                TagFilenameEnabled = false;
                TagFilename = "<keep>";
                TagTitle = haveSameTitle ? firstMusicFile.Title : "<keep>";
                TagArtist = haveSameArtist ? firstMusicFile.Artist : "<keep>";
                TagAlbum = haveSameAlbum ? firstMusicFile.Album : "<keep>";
                TagYearEnabled = haveSameYear;
                TagYear = haveSameYear ? firstMusicFile.Year : 0;
                TagTrackEnabled = haveSameTrack;
                TagTrack = haveSameTrack ? firstMusicFile.Track : 0;
                TagAlbumArtist = haveSameAlbumArtist ? firstMusicFile.AlbumArtist : "<keep>";
                TagGenre = haveSameGenre ? firstMusicFile.Genre : "<keep>";
                TagComment = haveSameComment ? firstMusicFile.Comment : "<keep>";
                TagDuration = $"Duration:\n{totalDurationAsString}";
                TagFileSize = $"File Size:\n{totalFileSizeAsString}";
            }
        }

        /// <summary>
        /// Checks for updates and if one is available, will prompt the user to automatically update the app
        /// </summary>
        private async Task CheckForUpdates(object parameter)
        {
            var updater = new Updater("https://raw.githubusercontent.com/nlogozzo/NickvisionTagger/main/UpdateConfig.json", Assembly.GetExecutingAssembly().GetName().Version);
            await updater.CheckForUpdateAsync();
            if (updater.UpdateAvaliable)
            {
                var result = await _contentDialogService.ShowAsync($"An update is available V{updater.LatestVersion}. Would you like to update?\nIf yes, Nickvision Tagger will attempt to download the installer and will close the application to update. Please make sure all work is saved", "Update Available", "No", "Yes");
                if (result == ContentDialogResult.Primary)
                {
                    var updateSuccessful = await updater.Update();
                    if (!updateSuccessful)
                    {
                        await _notificationService.Send("Update failed. Please try again later", "Update Failed", SystemIcons.Application);
                    }
                }
            }
            else
            {
                await _notificationService.Send("No update is available at this time", "No Update Available", SystemIcons.Application);
            }
        }

        /// <summary>
        /// Opens a browser and navigates to this program's new issue page on GitHub
        /// </summary>
        private void ReportABug(object parameter) => Process.Start(new ProcessStartInfo("cmd", $"/c start {"https://github.com/nlogozzo/NickvisionTagger/issues/new"}") { CreateNoWindow = true });

        /// <summary>
        /// Displays information about this program
        /// </summary>
        private async Task Changelog(object parameter) => await _contentDialogService.ShowAsync("- Reordered format string list in Filename To Tag and Tag To Filename", "What's New?", "OK");

        /// <summary>
        /// Handles when the window closes
        /// </summary>
        private async Task About(object parameter) => await _contentDialogService.ShowAsync($"Nickvision Tagger V{Assembly.GetExecutingAssembly().GetName().Version}", "About Nickvision Tagger", "OK");
    }
}
