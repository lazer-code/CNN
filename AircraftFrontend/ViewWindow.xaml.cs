using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AircraftFrontend
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        public ViewWindow(string path)
        {
            InitializeComponent();
            MouseDown += MainWindow_MouseDown;

            RunPythonScript(path);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ButtonMaximize.Content = "\uE922";
            }

            else
            {
                WindowState = WindowState.Maximized;
                ButtonMaximize.Content = "\uE923";
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void RunPythonScript(string path)
        {
            string outpath = Directory.GetCurrentDirectory();
            DirectoryInfo info = Directory.GetParent(outpath).Parent.Parent.Parent;
            outpath = info.FullName;

            ProcessStartInfo psi = new()
            {
                FileName = "python",
                Arguments = $"{outpath}\\model.py {path}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new() { StartInfo = psi };
            process.Start();

            Thread thread = new(() =>
            {
                string outputFile = "output.txt";

                while (!process.HasExited)
                {
                    if (File.Exists(outputFile))
                    {
                        string outputText = File.ReadAllText(outputFile);
                        Dispatcher.Invoke(() => label.Content = outputText);
                    }
                    Thread.Sleep(500);
                }

                Dispatcher.Invoke(() => LoadOutput(outpath));
            });

            thread.Start();
        }

        private void LoadOutput(string outpath)
        {
            string outputDir = "ModelOutput";

            var latestFile = Directory.GetFiles(outputDir)
                                        .Select(f => new FileInfo(f))
                                        .OrderByDescending(f => f.CreationTime)
                                        .FirstOrDefault();

            mediaElement.Source = new Uri(latestFile.FullName);
            File.Delete("output.txt");
        }
    }
}
