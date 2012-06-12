using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;

namespace MediaPlayerWithRemoteControl
{
  
    public class MediaPlayerViewModel : INotifyPropertyChanged
    {
        private  Song _selectedSong;
        public ObservableCollection<Song> Playlist { get; set;}
        public ObservableCollection<Album> Albums { get; set; }

        public Song SelectedSong
        {
            get { return _selectedSong; }
            set {
                if (_selectedSong != value) { _selectedSong = value; OnPropertyChanged("SelectedSong"); };
            }
        }

        public Album SelectedAlbum { get; set; }
        public MediaElement Player { get; set; }
        DispatcherTimer positionUpdateTimer;
        public int Position
        {
            get { return (int)Player.Position.TotalMilliseconds; }
            set
            {
                updatingTimer = true;
                Player.Position = new TimeSpan(0, 0, 0, 0, value);
                updatingTimer = false;
            }
        }

        public double Percentage
        {
            get { return Player.NaturalDuration.TimeSpan.TotalMilliseconds > 0 ? Player.Position.TotalMilliseconds / Player.NaturalDuration.TimeSpan.TotalMilliseconds * 100: 0; }
            set { if (Player.NaturalDuration.TimeSpan.TotalMilliseconds > 0) Player.Position = new TimeSpan(0, 0, 0, 0, (int) (value / 100 * Player.NaturalDuration.TimeSpan.TotalMilliseconds)); }
        }



        public int Length
        { get { return (int)Player.NaturalDuration.TimeSpan.TotalMilliseconds; } }

        public MediaPlayerViewModel(MediaElement mediaElement)
        {
            // normally we would initialie and keep Player internally, but it doesn't make a sound unless it's part of the control tree.
            Player = mediaElement;
            if (null == Player) Player = new MediaElement(); // no mediaelement supplied, so we can create one but it will have to be added to control tree manually by the view (we can't and won't manipulate the control tree in VM)

            Albums = new ObservableCollection<Album>();
            Playlist = new ObservableCollection<Song>();
            
            Player.LoadedBehavior = MediaState.Manual;
            Player.MediaEnded += new System.Windows.RoutedEventHandler(Player_MediaEnded);
            Player.MediaOpened += new System.Windows.RoutedEventHandler(Player_MediaOpened);

            GetAlbumsFromFolder(Properties.Settings.Default.Path).ForEach(album => Albums.Add(album));
            InitUpdateTimer();
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        // When the media opens, initialize the "Seek To" slider maximum value
        // to the total number of miliseconds in the length of the media clip.
        private void Player_MediaOpened(object sender, EventArgs e)
        {
            Player.Play();
            OnPropertyChanged("Length");
        }

        public bool PlayNextSong()
        {
            var index = Playlist.IndexOf(SelectedSong);
            if (index < Playlist.Count - 1)
            {
                SelectedSong = Playlist[index + 1];
                return true;
            }
            else return false;
        }

        public bool PlayAlbum(string albumTitle)
        {
            Album album = Albums.FirstOrDefault(a => a.Title == albumTitle);
            if (null == album) return false;
            SelectedAlbum = album;
            Playlist.Clear();
            foreach (Song song in album.Songs) Playlist.Add(song);
            if (Playlist.Count > 0) SelectedSong = Playlist.First(); else SelectedSong = null;
            Play();
            return true;
        }


        public bool PlaySong(string albumTitle, string songTitle)
        {
            Album album = Albums.FirstOrDefault(a=>a.Title == albumTitle);
            if (null==album) return false;
            Song song = album.Songs.FirstOrDefault(s=>s.Title== songTitle);
            if (null==song) return false;
            Playlist.Clear();
            foreach (Song sng in album.Songs) Playlist.Add(sng);
            SelectedSong = song;
            Play();
            return true;
        }

        void InitUpdateTimer()
        {
            positionUpdateTimer = new DispatcherTimer();
            positionUpdateTimer.Interval = TimeSpan.FromMilliseconds(250);
            positionUpdateTimer.Tick += (a, o) =>
            {
                if (!updatingTimer) OnPropertyChanged("Position"); // do not trigger updates while user is trying to update manually
                if (!updatingTimer) OnPropertyChanged("Percentage");
            };
            positionUpdateTimer.Start();
        }

        //public PlayerState GetState()
        //{
        //    try
        //    {
        //        PlayerState state = null;
        //        state = new PlayerState();
        //        var song = MainWindowReference.lstPlaylist.SelectedItem as Song;
        //        if (null!=song) {
        //            state.AlbumTitle = song.Album.Title;
        //            state.SongTitle = song.Title;
        //        }
        //        state.Position = MainWindowReference.MediaPlayer.Position;
        //        state.IsPlaying = state.Position>TimeSpan.Zero; //TODO add proper flag and change it when performing actions on MediaPlayer
        //        return state;
        //    }
        //    catch (Exception ex)
        //    { throw new FaultException(ex.Message); }
        //}

        public void Play()
        {
            Player.Play();
        }

        public void Pause()
        {
            Player.Pause();
        }


        public List<Album> GetAlbumsFromFolder(string folder)
        {
            var albums = new List<Album>();
            AddAlbumsFromFolder(folder, albums);
            albums = albums.OrderBy(a => a.Title).ToList();
            return albums;
        }

        // recursive function to dig through subfolders. we could use SearchOption.AllDirectories but we want to easily use folders as album names (no need to create a dictionary of albums for tracking)
        private void AddAlbumsFromFolder(string folder, List<Album> albums)
        {
            var songsInFolder = Directory.EnumerateFiles(folder, "*.mp3", SearchOption.TopDirectoryOnly).ToList();
            if (songsInFolder.Count > 0)
            {
                var album = new Album() { Title = Path.GetFileName(folder), Path = folder, Image = Path.Combine(folder, "folder.jpg") };
                foreach (string filename in songsInFolder)
                {
                    album.Songs.Add(new Song() { Title = Path.GetFileNameWithoutExtension(filename), FilePath = filename, Album = album });
                }
                albums.Add(album);
            }

            foreach (string subfolder in Directory.EnumerateDirectories(folder))
            {
                AddAlbumsFromFolder(subfolder, albums);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private bool updatingTimer;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public void EnqueueSong(Song s)
        {
            Playlist.Add(s);
        }
    }
}
