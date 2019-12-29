using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Audio_Book_Player
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class Player : Window
	{
		public static Random ran = new Random();
		DispatcherTimer timer = new DispatcherTimer();
		double currentTime { 
			get { return _currentTime; }
			set {
				_currentTime = value;
			} }
		double _currentTime;
		double volume;
		string currentBook = "none";
		string currentBookPath;
		public string currentFile;
		public string currentDirectory;
		public string[] filesInFolder;
		public int currentIndex;
		private bool currentlyPaused = true;

		public Player()
		{
			InitializeComponent();
		}
		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			LoadConfig();
			if (currentFile != "" && currentFile != null)
			{
				MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(currentTime));
				LoadFile(currentFile);
				Console.WriteLine(currentTime);
				LoadFiles();
				Console.WriteLine(currentTime);
				

			}
			
		}
		
		private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
		{
			TotalMin.Text = (MediaPlayer.NaturalDuration.TimeSpan.Minutes + (MediaPlayer.NaturalDuration.TimeSpan.Hours * 60)).ToString();
			TotalSec.Text = MediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString();
			PositionSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
			RefreshUI();
			if (!timer.IsEnabled)
			{
				timer.Interval = TimeSpan.FromSeconds(1);
				timer.Tick += Tick;
				timer.Start();
			}
		}
		private void PositionSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Released && !currentlyPaused)
				MediaPlayer.Play();
			MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(Math.Floor(PositionSlider.Value)));
			currentTime = PositionSlider.Value;
			RefreshPositionUI();
			Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, currentTime.ToString());
		}
		private void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
				MediaPlayer.Pause();
		}
		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			volume = VolumeSlider.Value;
			MediaPlayer.Volume = volume;
			Code.CommonFunctions.AddOrUpdateAppSettings("Volume", volume.ToString());
		}
		private void Start_Click(object sender, RoutedEventArgs e)
		{
			currentTime = 0;
			Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
			MediaPlayer.Position = new TimeSpan(0);
			RefreshPositionUI();
			MediaPlayer.Play();
			currentlyPaused = false;
			ContinuePauseText.Text = "Pause";
		}
		private void ContinuePause_Click(object sender, RoutedEventArgs e)
		{
			if (currentlyPaused)
			{
				MediaPlayer.Play();
				currentlyPaused = false;
				ContinuePauseText.Text = "Pause";
				if (RevertCheckBox.IsChecked == true)
					MediaPlayer.Position = MediaPlayer.Position.Subtract(new TimeSpan(0, 0, Int32.Parse(ConfigurationManager.AppSettings.Get("reverttime"))));
			}
			else
			{
				MediaPlayer.Pause();
				currentlyPaused = true;
				ContinuePauseText.Text = "Continue";
			}
		}

		private void SkipBack_Click(object sender, RoutedEventArgs e)
		{
			LoadFile(Code.CommonFunctions.GetFormerFile(filesInFolder, currentIndex));
			currentTime = 0;
			Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
		}

		private void SkipForward_Click(object sender, RoutedEventArgs e)
		{
			LoadFile(Code.CommonFunctions.GetNextFile(filesInFolder, currentIndex));
			currentTime = 0;
			Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
			if (openFileDialog.ShowDialog() == true)
			{
				filesInFolder = null;
				LoadFile(openFileDialog.FileName);
				LoadFiles();
			}
		}

		void Tick(object sender, EventArgs e)
		{
			currentTime = MediaPlayer.Position.TotalSeconds;
			RefreshPositionUI();
			if (MediaPlayer.NaturalDuration.HasTimeSpan)
				if (currentTime == MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
				{
					LoadFile(Code.CommonFunctions.GetNextFile(filesInFolder, currentIndex));
					currentTime = 0;
					Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
				}
			Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, currentTime.ToString());
		}

		void LoadConfig()
		{
			currentFile = "";
			currentTime = 0;
			volume = 1;

			string tempShuffle = ConfigurationManager.AppSettings.Get("shuffle");
			if (tempShuffle == "" || tempShuffle == null)
				Code.CommonFunctions.AddOrUpdateAppSettings("shuffle", "False");
			ShuffleCheckBox.IsChecked = Boolean.Parse(ConfigurationManager.AppSettings.Get("shuffle"));
			string tempRevert = ConfigurationManager.AppSettings.Get("revert");
			if (tempRevert == "" || tempRevert == null)
				Code.CommonFunctions.AddOrUpdateAppSettings("revert", "False");
			RevertCheckBox.IsChecked = Boolean.Parse(ConfigurationManager.AppSettings.Get("revert"));
			RevertTime.IsEnabled = Boolean.Parse(ConfigurationManager.AppSettings.Get("revert"));
			
			string revertTime = ConfigurationManager.AppSettings.Get("reverttime");
			if (revertTime == "" || revertTime == null)
				Code.CommonFunctions.AddOrUpdateAppSettings("reverttime", "30");
			RevertTime.Text = ConfigurationManager.AppSettings.Get("reverttime");
			currentBook = ConfigurationManager.AppSettings.Get("currentbook");
			if (currentBook == "" || currentBook == null)
			{
				currentBook = "none";
				return;
			} else
			{
				currentFile = ConfigurationManager.AppSettings.Get("path --- " + currentBook);
				currentTime = Convert.ToDouble(ConfigurationManager.AppSettings.Get("position --- " + currentBook));
				volume = Convert.ToDouble(ConfigurationManager.AppSettings.Get("Volume"));
			}
		}

		void LoadFile(string _file)
		{
			Debug.WriteLine("Trying to open: " + _file);
			MediaPlayer.Source = new Uri(_file);
			
			RefreshUI();
			currentDirectory = System.IO.Path.GetDirectoryName(_file);
			this.Title = System.IO.Path.GetFileName(currentDirectory);
			currentBook = this.Title;
			currentBookPath = currentDirectory;
			currentFile = _file;
			Code.CommonFunctions.AddOrUpdateAppSettings("currentbook", currentBook);
			Code.CommonFunctions.AddOrUpdateAppSettings("path --- " + currentBook, _file);
			if (filesInFolder != null)
			{
				currentIndex = Array.IndexOf(filesInFolder, currentFile);

				
			}

			ChapterList.SelectedItem = System.IO.Path.GetFileNameWithoutExtension(currentFile);
		}

		void LoadFiles()
		{
			ChapterList.Items.Clear();
			filesInFolder = Code.CommonFunctions.GetFilesInDirectory(currentDirectory);
			Array.Sort(filesInFolder, Code.CommonFunctions.CompareNatural);
			for (int i = 0; i < filesInFolder.Length; i++)
			{
				ChapterList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(filesInFolder[i]));
			}
			currentIndex = Array.IndexOf(filesInFolder, currentFile);
			ChapterList.SelectedItem = System.IO.Path.GetFileNameWithoutExtension(currentFile);

			BookList.Items.Clear();
			string[] tempBooks = ConfigurationManager.AppSettings.AllKeys;
			tempBooks = tempBooks.Where(c => c.Contains("path")).ToArray();
			List<string> books = new List<string>();
			for (int i = 0; i < tempBooks.Length; i++)
			{
				books.Add(tempBooks[i].Substring(9));
				BookList.Items.Add(tempBooks[i].Substring(9));
			}
			BookList.SelectedItem = currentBook;
		}

		public void RefreshUI()
		{
			VolumeSlider.Value = volume;
			MediaPlayer.Volume = volume;

			PositionSlider.Value = currentTime;
			//MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(currentTime));

			if (MediaPlayer.NaturalDuration.HasTimeSpan)
			{
				RefreshPositionUI();
			}
		}

		private void BookList_DropDownClosed(object sender, EventArgs e)
		{
			if (BookList.SelectedItem != null && BookList.SelectedItem.ToString() != currentBook)
			{
				Code.CommonFunctions.AddOrUpdateAppSettings("currentbook", BookList.SelectedItem.ToString());
				LoadConfig();
				
				LoadFile(currentFile);
				MediaPlayer.Position = new TimeSpan(0, 0, Convert.ToInt32(currentTime));
				LoadFiles();
			}
		}

		void RefreshPositionUI()
		{
			PositionSlider.Value = currentTime;
			CurrentMin.Text = (MediaPlayer.Position.Minutes + (MediaPlayer.Position.Hours * 60)).ToString();
			CurrentSec.Text = MediaPlayer.Position.Seconds.ToString();
		}

		private void ChapterList_DropDownClosed(object sender, EventArgs e)
		{
			
			if (ChapterList.SelectedItem != null)
			{
				string selectedFile = (currentDirectory + "\\" + ChapterList.SelectedItem.ToString() + ".mp3");
				if (currentFile != selectedFile)
				{
					LoadFile(selectedFile);
					currentTime = 0;
					Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
				}
			}
		}

		private void ShuffleCheckBox_Click(object sender, RoutedEventArgs e)
		{
			Code.CommonFunctions.AddOrUpdateAppSettings("shuffle", ShuffleCheckBox.IsChecked.ToString());
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			if (SettingsGrid.IsEnabled)
			{
				SettingsGrid.IsEnabled = false;
				MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
			}
			else
			{
				SettingsGrid.IsEnabled = true;
				MainGrid.ColumnDefinitions[1].Width = new GridLength();
			}
		}

		private void RevertCheckBox_Click(object sender, RoutedEventArgs e)
		{
			Code.CommonFunctions.AddOrUpdateAppSettings("revert", RevertCheckBox.IsChecked.ToString());
			if (RevertCheckBox.IsChecked == true)
			{
				RevertTime.IsEnabled = true;
			} else
			{
				RevertTime.IsEnabled = false;
			}
		}

		private void RevetTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void RevertTime_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Code.CommonFunctions.AddOrUpdateAppSettings("reverttime", RevertTime.Text);
		}
	}
}
