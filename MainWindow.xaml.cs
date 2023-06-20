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
		private InputStream ins = new();
		public MainWindow()
		{
			InitializeComponent();
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			TokenOutput.Clear();
			ProgramOutput.Clear();

			StringReader sr = new(ProgramCode.Text);
			TokenStream tokenizer = new(sr);
			var tokens = tokenizer.GetTokens().ToList();

			StringBuilder sb = new();
			foreach (var token in tokens)
			{
				sb.Append(token.ToString());
				sb.Append('\n');
			}
			TokenOutput.Text = sb.ToString();

			var parser = new Parser.Parser(tokens);
			var parsedTokens = parser.Parsed;

			try
			{
				var runner = new VirtualMachine.CodeRunner(parsedTokens, AddToOut, ins.GetNextManual);
				runner.Run();
			}
			catch (Exception ex)
			{
				AddToOut(ex.Message);
			}
		}

		private void AddToOut(string outp)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				ProgramOutput.Text += outp + '\n';
			});
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new();
			if (!(openFileDialog.ShowDialog() ?? false))
			{
				return;
			}
			ProgramCode.Text = new StreamReader(openFileDialog.FileName).ReadToEnd();
		}
	}

	internal class InputStream
	{
		private List<int> _inputs = new() { 10, 1, 20, 40};
		private int _manInputIndex = 0;
		public int GetNextManual()
		{
			_manInputIndex = _manInputIndex % _inputs.Count;
			return _inputs[_manInputIndex++];
		}
	}
}
