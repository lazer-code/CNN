using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace AircraftFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// A function which innitiates the MainWindow window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            MouseDown += MainWindow_MouseDown;
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
        /// A function which closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// A function which handles local file detection option.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string filepath = dlg.FileName;
                ViewWindow window = new(filepath);
                window.ShowDialog();
            }
        }

        /// <summary>
        /// A function which handles YouTube video detection option.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string url = URLbox.Text;

            if (url == "")
            {
                MessageBox.Show("Please enter a URL.");
                return;
            }

            URLbox.Text = "Enter URL";
            ViewWindow window = new(url);
            window.Closed += (sender, e) => this.Show();
            this.Hide();
            window.ShowDialog();
        }

        /// <summary>
        /// A function which clears the URL in the input box when it gets focused.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void URLbox_GotFocus(object sender, RoutedEventArgs e) => this.URLbox.Text = "";
    }
}