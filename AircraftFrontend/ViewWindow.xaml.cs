using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AircraftFrontend
{
    /// <summary>
    /// Interaction logic for ViewWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        private TcpListener? server;
        private Thread? thread;
        private Process? process;

        /// <summary>
        /// A function which innitiates the ViewWindow window.
        /// </summary>
        /// <param name="path"></param>
        public ViewWindow(string path)
        {
            InitializeComponent();
            MouseDown += MainWindow_MouseDown;
            this.Closing += Window_Closing;
            RunPythonScript(path);
        }

        /// <summary>
        /// A function which enabless window moving when the left button is being clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// A function which handles the maximize / demaximize the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// A function which minimizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        /// <summary>
        /// A function which closes the window and stops the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.server != null)
                this.server.Stop();

            Close();
        }

        /// <summary>
        /// A function which runs the python file which predicts all the objects in pictures and videos.
        /// </summary>
        /// <param name="path"></param>
        private void RunPythonScript(string path)
        {
            string outpath = Directory.GetCurrentDirectory();
            DirectoryInfo info = Directory.GetParent(outpath).Parent.Parent.Parent;
            outpath = info.FullName;
            string modelPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName + "\\model.py";

            ProcessStartInfo psi = new()
            {
                FileName = "py",
                Arguments = $"{modelPath} {path}",
                UseShellExecute = true,
                CreateNoWindow = false
            };



            this.process = new() { StartInfo = psi };
            this.process.Start();
            

            int port = 12345;
            this.server = new TcpListener(IPAddress.Any, port);
            this.server.Start();
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            path = "";

            this.thread = new(() =>
            {
                while (true)
                {
                    byte[] data = new byte[16384];
                    int bytesRead = 0;
                    StringBuilder messageBuilder = new();

                    while ((bytesRead = stream.Read(data, 0, data.Length)) > 0)
                    {
                        messageBuilder.Clear();
                        messageBuilder.Append(Encoding.ASCII.GetString(data, 0, bytesRead));
                        Dispatcher.Invoke(() => label.Content = messageBuilder.ToString());

                        if (messageBuilder.ToString().StartsWith("DONE."))
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

                try
                {
                    Dispatcher.Invoke(() => this.mediaElement.Source = new Uri(path));
                }
                catch (Exception ex)
                {
                    label.Content = "Invalid Path!";
                }

            });

            thread.Start();
        }

        /// <summary>
        /// A function which closes the server and the python file when the window gets closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.process != null)
                this.process.Close();

            if (this.server != null)
                this.server.Stop();
        }
    }
}
