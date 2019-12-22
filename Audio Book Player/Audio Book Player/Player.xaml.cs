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

namespace Audio_Book_Player
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class Player : Window
	{
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
		private bool initialUISet = true;

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
			string selectedFile = (currentDirectory + "\\" + ChapterList.SelectedItem.ToString() + ".mp3");
			if (ChapterList.SelectedItem != null && currentFile != selectedFile)
			{
				LoadFile(selectedFile);
				currentTime = 0;
				Code.CommonFunctions.AddOrUpdateAppSettings("position --- " + currentBook, "0");
			}
		}
	}
}
