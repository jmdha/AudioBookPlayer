using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Audio_Book_Player
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class Player : Window
	{
		DispatcherTimer timer = new DispatcherTimer();
		double currentTime, totalTime;

		public Player()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{

			MediaPlayer.Play();
		}

		private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			if (MediaPlayer.NaturalDuration.HasTimeSpan)
			{
				totalTime = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
				TotalMin.Text = MediaPlayer.NaturalDuration.TimeSpan.Minutes.ToString();
				TotalSec.Text = MediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString();

				PositionSlider.Maximum = totalTime;

			}
			VolumeSlider.Value = MediaPlayer.Volume;
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Tick;
			timer.Start();
		}

		private void PositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Released)
				MediaPlayer.Play();
			MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(Math.Floor(PositionSlider.Value)));
			currentTime = PositionSlider.Value;
		}

		private void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
						MediaPlayer.Pause();
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			MediaPlayer.Volume = VolumeSlider.Value;
		}

		void Tick(object sender, EventArgs e)
		{
			currentTime = MediaPlayer.Position.TotalSeconds;
			PositionSlider.Value = currentTime;
			CurrentMin.Text = MediaPlayer.Position.Minutes.ToString();
			CurrentSec.Text = MediaPlayer.Position.Seconds.ToString();
		}

	}
}
