using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Audio_Book_Player
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
			if (openFileDialog.ShowDialog() == true)
			{


				Player player = new Player();
				player.MediaPlayer.Source = new Uri(openFileDialog.FileName);
				player.currentFile = openFileDialog.FileName;
				player.currentDirectory = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
				string fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
				player.Title.Text = fileName;
				player.Show();
				this.Close();
			}
		}
	}
}
