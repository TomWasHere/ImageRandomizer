using System;
using System.Collections.Generic;
using System.IO;
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

namespace ImageRandomizer
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
			// Create OpenFileDialog 
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension 
			dlg.DefaultExt = ".png";
			dlg.Filter = "Image files (*.jpg,*.gif,*.png,*.jpeg,*.bmp)|*.jpg;*.gif;*.png;*.jpeg;*.bmp|All files (*.*)|*.*";

			// Display OpenFileDialog by calling ShowDialog method 
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox 
			if (result == true)
			{
				// Open document 
				string filename = dlg.FileName;
				textboxFileName.Text = filename;

				ShowImage(filename);
			}
		}

		private void ShowImage(string filename)
		{
			Image.Source = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));
		}

		private System.Drawing.Bitmap Scramble(string filename, int pixelSize, IProgress<Tuple<int,int>> progress)
		{
			DateTime startTime = DateTime.Now;
			System.Drawing.Bitmap originalImage = new System.Drawing.Bitmap(filename);
			int width = originalImage.Width;
			int height = originalImage.Height;

			System.Drawing.Bitmap newImage = new System.Drawing.Bitmap(width, height);

			// Generate list of lists
			var listOfLists = new List<List<Tuple<int, int>>>(width);

			for (int i = 0; i <= width - pixelSize; i+= pixelSize)
			{

					var list = new List<Tuple<int, int>>(height);
					for (int j = 0; j <= height - pixelSize; j+=pixelSize)
					{
						list.Add(new Tuple<int, int>(i, j));
					}
					listOfLists.Add(list);
			}

			var rng = new Random();
			for (int w = 0; w <= width - pixelSize; w+=pixelSize)
			{
				for (int h = 0; h <= height - pixelSize; h+=pixelSize)
				{
					// Get a random pixel
					int a, b;
					a = rng.Next(listOfLists.Count);
					b = rng.Next(listOfLists[a].Count);

					int x, y;
					x = listOfLists[a][b].Item1;
					y = listOfLists[a][b].Item2;

					// Remove it from the list
					listOfLists[a].RemoveAt(b);
					if (listOfLists[a].Count == 0)
					{
						listOfLists.RemoveAt(a);
					}

					for (int i = 0; i < pixelSize; i++)
					{
						for (int j = 0; j < pixelSize; j++)
						{
							// Find the pixel value
							System.Drawing.Color pixel = originalImage.GetPixel(w + i, h + j);

							// Set it in the new image
							newImage.SetPixel(x+ i, y+j, pixel);
						}
					}


				}
				if (progress != null)
				{
					progress.Report(new Tuple<int,int>(w+pixelSize, width));
				}
			}

			if (progress != null)
			{
				progress.Report(new Tuple<int, int>(100, 100));
			}

			return newImage;
		}

		private ImageSource ImageConverter(System.Drawing.Image inputImage)
		{
			MemoryStream ms = new MemoryStream();
			inputImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			ms.Position = 0;
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();

			return bi;
		}


		private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ButtonScramble.Content = "Scrambling!!!";
            string filename = textboxFileName.Text;
			int pixelSize = (int)SliderPixel.Value;
			ProgressBar.Visibility = Visibility.Visible;

			var progressIndicator = new Progress<Tuple<int,int>>(ReportProgress);

            var image = await Task.Run(() =>
            {
				return Scramble(filename, pixelSize, progressIndicator);
            });


            Image.Source = ImageConverter(image);

            ButtonScramble.Content = "Scramble!";
			ProgressBar.Visibility = Visibility.Hidden;
        }

		private void ReportProgress(Tuple<int,int> value)
		{
			ProgressBar.Value = value.Item1;
			ProgressBar.Maximum = value.Item2;
		}

	}
}
