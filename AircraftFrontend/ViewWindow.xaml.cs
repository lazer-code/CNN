using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private TcpListener? server;
        private Thread thread;
        private Process process;

        public ViewWindow(string path)
        {
            InitializeComponent();
            MouseDown += MainWindow_MouseDown;
            this.Closing += Window_Closing;
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.server.Stop();
            Close();
        }

        private void RunPythonScript(string path)
        {
            string outpath = Directory.GetCurrentDirectory();
            DirectoryInfo info = Directory.GetParent(outpath).Parent.Parent.Parent;
            outpath = info.FullName;
            string p = Directory.GetCurrentDirectory();

            ProcessStartInfo psi = new()
            {
                FileName = "python",
                Arguments = $"model.py",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            this.process = new() { StartInfo = psi };
            this.process.Start();
            

            int port = 12345;
            this.server = new TcpListener(IPAddress.Any, port);
            this.server.Start();
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            byte[] response = Encoding.ASCII.GetBytes(path);
            stream.Write(response, 0, response.Length);

            this.thread = new(() =>
            {
                while (true)
                {
                    byte[] data = new byte[16384];
                    int bytesRead = 0;
                    StringBuilder messageBuilder = new StringBuilder();

                    while ((bytesRead = stream.Read(data, 0, data.Length)) > 0)
                    {
                        messageBuilder.Clear();
                        messageBuilder.Append(Encoding.ASCII.GetString(data, 0, bytesRead));
                        Dispatcher.Invoke(() => label.Content = messageBuilder.ToString());

                        if (messageBuilder.ToString().Contains("DONE."))
                        {
                            Dispatcher.Invoke(() => label.Content = "");
                            string tmp = messageBuilder.ToString().Substring(5);
                            path = tmp;
                        }    
                    }

                    break;
                }

                client.Close();
                server.Stop();

                Dispatcher.Invoke(() => this.mediaElement.Source = new Uri(path));
            });

            thread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.process.Close();
            this.server.Stop();
        }
    }
}
