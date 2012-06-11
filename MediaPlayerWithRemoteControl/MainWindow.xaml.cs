using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using SignalR.Client.Hubs;

namespace MediaPlayerWithRemoteControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public SynchronizationContext SyncContext = SynchronizationContext.Current;
		private RemoteControlHost remoteControlHost;
		public MainWindow()
		{
			InitializeComponent();
		}

		DispatcherTimer timelineUpdater;
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var viewModel = new MediaPlayerViewModel(); // not really using the VM yet, just a way to get albums
            lstAlbums.ItemsSource = viewModel.Albums;

			timelineUpdater = new DispatcherTimer();
			timelineUpdater.Interval = TimeSpan.FromMilliseconds(250);
			timelineUpdater.Tick += (a, o) =>
			{
                updatingTimer = true;
				timelineSlider.Value = MediaPlayer.Position.TotalMilliseconds;
                //if (timelineSlider.Value > 0 && chat != null)
                //{
                //    chat.Invoke("Position", (int)timelineSlider.Value);
                //}

			};
			timelineUpdater.Start();

			// avoid a namespace conflict with a running instance of Laharsub
//            Laharsub_Accessor.parameterDefaults["baseAddress"] = "http://" + Environment.MachineName + "/" + Guid.NewGuid().ToString();
		   // Laharsub.Server.Wcf.Laharsub.Start();
			//Laharsub.Server.Wcf.Laharsub.Stop();

		}

		private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			if (lstSongs.SelectedIndex < lstSongs.Items.Count - 1) lstSongs.SelectedIndex += 1; else lstSongs.SelectedIndex = 0;
			
		}


		// Change the volume of the media.
		private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			MediaPlayer.Volume = (double)volumeSlider.Value;
		}

		// When the media opens, initialize the "Seek To" slider maximum value
		// to the total number of miliseconds in the length of the media clip.
		private void Element_MediaOpened(object sender, EventArgs e)
		{
			MediaPlayer.Play();
			timelineSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
		}


		// Jump to different parts of the media (seek to). 
		private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
            if (chat != null)
            {
                chat.Invoke("Position", (int)timelineSlider.Value);
            }
            if (updatingTimer) //do not change position when change triggered by our own timer- could cause skipping.
            {
                updatingTimer = false;
                return;
            }
			int SliderValue = (int)timelineSlider.Value;

			// Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
			// Create a TimeSpan with miliseconds equal to the slider value.
			TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
			MediaPlayer.Position = ts;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null!=remoteControlHost) remoteControlHost.Dispose();
		}


        RemoteControl remoteControl;
        IHubProxy chat;
        HubConnection hubConnection;
        private bool updatingTimer;

		private void btnEnableService_Click(object sender, RoutedEventArgs e)
		{
		//	remoteControlHost = new RemoteControlHost(this);
			remoteControl = new RemoteControl(this);

            hubConnection = new HubConnection(Properties.Settings.Default.SignalRUrl); ///signalR/");
			chat = hubConnection.CreateProxy("chat");

			chat.On("PlayAlbum", albumTitle =>
			{
				remoteControl.PlayAlbum(albumTitle);
				//WriteLine("{0} client state index -> {1}", i, chat["index"]);
			});

			hubConnection.Start().Wait();

			//chat.Invoke("multipleCalls").ContinueWith(task =>
			//{
			//    Console.WriteLine(task.Exception);

			//}, TaskContinuationOptions.OnlyOnFaulted);

			//Task.Factory.StartNew(() =>
			//{
			//    Thread.Sleep(7000);
			//    hubConnection.Stop();
			//});
		

			btnEnableService.IsEnabled = false;
		}

		private void btnPlay_Click(object sender, RoutedEventArgs e)
		{
			// The Play method will begin the media if it is not currently active or 
			// resume media if it is paused. This has no effect if the media is
			// already running.
			MediaPlayer.Play();
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			// The Pause method pauses the media if it is currently running.
			// The Play method can be used to resume.
			MediaPlayer.Pause();
		}

        private void lstPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void lstAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Album selectedAlbum = lstAlbums.SelectedItem as Album;
            if (selectedAlbum!= null)
            {
                remoteControl.PlayAlbum(selectedAlbum.Title);
            }
        }

        private void lstSongs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Song selectedSong = lstSongs.SelectedItem as Song;
            if (selectedSong != null)
            {
                remoteControl.PlaySong(selectedSong.Album.Title, selectedSong.Title);
            }
        }
	}
}
