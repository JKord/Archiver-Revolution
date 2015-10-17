#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArchivingLibrary;
using System.Windows.Threading;
#endregion

namespace BWT
{
    /// <summary>
    /// Логика взаимодействия для ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        #region Fields

        public DispatcherTimer timerProgress;

        delegate Double Progress();

        Progress progressDele;

        #endregion

        public ProgressWindow()
        {
            InitializeComponent();           

           timerProgress = new DispatcherTimer(DispatcherPriority.Loaded);
           timerProgress.Interval = TimeSpan.FromMilliseconds(1);
           timerProgress.Tick += new EventHandler(delegate(object s, EventArgs a)
           {
               BarProgressProcesing();
           });
           timerProgress.Start();           
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }      

        private void button1_Click(object sender, RoutedEventArgs e)
        {           
            this.Close();
        }

        void BarProgressProcesing()
        {
            if (progressBar.Value <= ArchivingLibrary.BWT.progress)
               progressBar.Value += 4;
            this.Title = "Progress - " + progressBar.Value;
            if (progressBar.Value >= progressBar.Maximum)
            {
                timerProgress.Stop();
                button1.Visibility = Visibility.Visible;
            }
        }       
    }
}
