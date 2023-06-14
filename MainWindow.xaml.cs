using Microsoft.Win32;
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
using TinyBasic.Tokenizer;

namespace TinyBasic
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

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new();
			if (!(openFileDialog.ShowDialog()??false)) 
			{
				return;
			}
			ProgramCode.Text = new StreamReader(openFileDialog.FileName).ReadToEnd();

			TokenStream tokenizer = new(openFileDialog.FileName);
			var tokens = tokenizer.GetTokens().ToList();

			StringBuilder sb = new();
			foreach (var token in tokens)
			{
				sb.Append(token.ToString());
				sb.Append('\n');
			}
			TokenOutput.Text = sb.ToString();


		}
	}
}
