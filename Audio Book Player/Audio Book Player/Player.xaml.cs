using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Audio_Book_Player
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class Player : Window
	{
		DispatcherTimer timer = new DispatcherTimer();
		double currentTime, totalTime;
		public string currentFile;
		public string currentDirectory;
		private bool currentlyPaused = true;

		public Player()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			LoadConfig();
			MediaPlayer.Play();
		}

		void LoadConfig()
		{
			VolumeSlider.Value = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Volume"));
			MediaPlayer.Volume = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Volume"));
			currentTime = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Position"));
			MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(currentTime));
			UpdatePosition();
		}


		public static void AddOrUpdateAppSettings(string key, string value)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				if (settings[key] == null)
				{
					settings.Add(key, value);
				}
				else
				{
					settings[key].Value = value;
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error writing app settings");
			}
		}

		void RefreshUI()
		{
			totalTime = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
			TotalMin.Text = MediaPlayer.NaturalDuration.TimeSpan.Minutes.ToString();
			TotalSec.Text = MediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString();

			PositionSlider.Maximum = totalTime;

		}

		private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			if (MediaPlayer.NaturalDuration.HasTimeSpan)
			{
				RefreshUI();
			}
			
			UpdatePosition();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += Tick;
			timer.Start();
		}

		private void PositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Released && !currentlyPaused)
				MediaPlayer.Play();
			MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(Math.Floor(PositionSlider.Value)));
			currentTime = PositionSlider.Value;
			AddOrUpdateAppSettings("Position", currentTime.ToString());
		}

		private void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
						MediaPlayer.Pause();
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			MediaPlayer.Volume = VolumeSlider.Value;
			AddOrUpdateAppSettings("Volume", VolumeSlider.Value.ToString());
		}

		void UpdatePosition()
		{
			PositionSlider.Value = currentTime;
			CurrentMin.Text = MediaPlayer.Position.Minutes.ToString();
			CurrentSec.Text = MediaPlayer.Position.Seconds.ToString();
		}

		void Tick(object sender, EventArgs e)     
		{
			currentTime = MediaPlayer.Position.TotalSeconds;
			UpdatePosition();
			AddOrUpdateAppSettings("Position", currentTime.ToString());
			if (MediaPlayer.NaturalDuration.HasTimeSpan)
				if (currentTime == MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
					LoadNextFile();
		}

		void LoadNextFile()
		{
			string nextFile = GetNextFile();
			MediaPlayer.Source = new Uri(nextFile);
			Title.Text = System.IO.Path.GetFileNameWithoutExtension(nextFile);
			if (MediaPlayer.NaturalDuration.HasTimeSpan)
			{
				RefreshUI();
			}
			AddOrUpdateAppSettings("Position", "0");
			UpdatePosition();
			currentFile = nextFile;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MediaPlayer.Pause();
			currentlyPaused = true;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			MediaPlayer.Play();
			currentlyPaused = false;
		}

		string GetNextFile()
		{
			string[] files = Directory.GetFiles(currentDirectory);
			bool foundCurrentFile = false;
			foreach(string file in files)
			{
				if (file.Contains(".mp3"))
				{
					if (file == currentFile)
						foundCurrentFile = true;
					else if (file != currentFile && foundCurrentFile)
						return file;
						
				}
			}
			return files[0];
		}

	}
}
