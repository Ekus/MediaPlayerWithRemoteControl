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
        MediaElement Player { get; set; }

        public MediaPlayerViewModel()
        {
            Albums = new ObservableCollection<Album>();
            Playlist = new ObservableCollection<Song>();
            Player = new MediaElement();
            Player.MediaEnded += new System.Windows.RoutedEventHandler(Player_MediaEnded);
            GetAlbumsFromFolder(Properties.Settings.Default.Path).ForEach(album => Albums.Add(album));
        }

        public bool PlayAlbum(string albumTitle)
        {
            Album album = Albums.FirstOrDefault(a=>a.Title==albumTitle);
            if (null==album) return false;
            SelectedAlbum = album;
            Playlist.Clear();
            foreach (Song song in album.Songs) Playlist.Add(song);
            if (Playlist.Count>0) SelectedSong = Playlist.First(); else SelectedSong = null;
            UpdatePlayer();
            return true;
        }

        void UpdatePlayer()
        {
            Player.Source = new Uri(SelectedSong.FilePath);
//            Player.SetBinding(MediaElement.SourceProperty, "SelectedSong.Path"); 
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        public bool PlayNextSong()
        {
            var index = Playlist.IndexOf(SelectedSong);
            if (index < Playlist.Count - 1)
            {
                SelectedSong = Playlist[index + 1];
                UpdatePlayer();
                return true;
            }
            else return false;
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
            UpdatePlayer();
            return true;
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
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
