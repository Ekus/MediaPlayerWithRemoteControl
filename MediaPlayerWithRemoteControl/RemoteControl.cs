using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MediaPlayerWithRemoteControl
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        InstanceContextMode = InstanceContextMode.Single, 
        UseSynchronizationContext = false)]
    public class RemoteControl : IRemoteControl
    {
        private MainWindow MainWindowReference;

        public RemoteControl(MainWindow window)
        {
            MainWindowReference = window;
        }

        public void PlayAlbum(string albumTitle)
        {
            try
            {

                MainWindowReference.SyncContext.Send(state =>
                {
                    foreach (Album album in MainWindowReference.lstAlbums.Items)
                    {
                        if (album.Title == albumTitle)
                        {
                            MainWindowReference.lstAlbums.SelectedItem = album;
                            if (MainWindowReference.lstSongs.Items.Count > 0) MainWindowReference.lstSongs.SelectedIndex = 0;
                            break;
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
        }

        public void PlaySong(string albumTitle, string songTitle)
        {
            try
            {
                MainWindowReference.SyncContext.Send(state =>
                {
                    foreach (Album album in MainWindowReference.lstAlbums.Items)
                    {
                        if (album.Title == albumTitle)
                        {
                            MainWindowReference.lstAlbums.SelectedItem = album;
                            foreach (Song song in MainWindowReference.lstSongs.Items)
                            {
                                 if (song.Title == songTitle) MainWindowReference.lstSongs.SelectedItem = song;
                                break;
                            }
                        }
                    }
                }, null);
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
        }

        public PlayerState GetState()
        {
            try
            {
                PlayerState state = null;
                state = new PlayerState();
                var song = MainWindowReference.lstSongs.SelectedItem as Song;
                if (null!=song) {
                    state.AlbumTitle = song.Album.Title;
                    state.SongTitle = song.Title;
                }
                state.Position = MainWindowReference.MediaPlayer.Position;
                state.IsPlaying = state.Position>TimeSpan.Zero; //TODO add proper flag and change it when performing actions on MediaPlayer
                return state;
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
        }


        public Album[] GetAlbums()
        {
            Album[] result =null;
            try
            {

                MainWindowReference.SyncContext.Send(state =>
                {
                    result = (MainWindowReference.lstAlbums.Items.Cast<Album>()).ToArray();
                }, null);
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
            return result;
            
        }

        public void Play()
        {
            try
            {
                MainWindowReference.SyncContext.Send(state =>
                {
                    MainWindowReference.MediaPlayer.Play();
                }, null);
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
           
        }

        public void Pause()
        {
            try
            {

                MainWindowReference.SyncContext.Send(state =>
                {
                    if (MainWindowReference.MediaPlayer.CanPause) MainWindowReference.MediaPlayer.Pause();
                }, null);
            }
            catch (Exception ex)
            { throw new FaultException(ex.Message); }
            
        }
    }
}
