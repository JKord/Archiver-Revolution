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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArchivingLibrary;
using System.Windows.Forms;
using System.IO;
using System.Threading;
#endregion

namespace BWT
{
    /// <Головне вікно>
    /// Логика взаимодействия для MainWindow.xaml
    /// </Головне вікно>
    public partial class MainWindow : Window
    {
        #region Fields

        Thread trdProcess; // Поток процесу
        OpenFileDialog fileDialog; // Діалогове вікно відкриття файлів

        String choice; // Вибір операції
        String FolderSave = "D:/"; // Шлях збереження файлів після виконання операції

        ProgressWindow progressW; // Вікно ходу виконання процесу

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Функція самого процесу, яка виконується в іншому потоці       
        private void ThreadTaskProcesing()
        {
            byte[] source = File.ReadAllBytes(fileDialog.FileName);

            switch (choice)
            {
                case "Encoder":
                    {
                        int offset = 0;
                        ArchivingLibrary.BWT.Encoder(ref source, ref offset);
                        List<byte> tmpSource = new List<byte>(source);
                        // Запис індексу оригінального рядку.   
                        byte[] tmpOffset = BitConverter.GetBytes((ushort)offset);
                        tmpSource.AddRange(tmpOffset);
                        source = tmpSource.ToArray();
                    } break;
                case "Decoder":
                    {
                        #region Зчитування і видалення індексу оригінального рядку

                        int size = source.Length;
                        List<byte> tmpSource = new List<byte>(source);
                        int offset = (int)BitConverter.ToUInt16(tmpSource.ToArray(), (size - 2));

                        tmpSource.RemoveAt(size - 2);
                        tmpSource.RemoveAt(size - 2);

                        source = tmpSource.ToArray();

                        #endregion

                        ArchivingLibrary.BWT.Decoder(ref source, offset);
                    } break;
                case "Compress":
                    {
                        ArchivingLibrary.BWT.Compress(ref source);
                    } break;
                case "Decompress":
                    {
                        ArchivingLibrary.BWT.Decompress(ref source);
                    } break;
            }

            File.WriteAllBytes(FolderSave + "/" + choice, source);

            trdProcess.Abort();
        }

        //Обробка натиску меню 
        #region Menu Item      

        private void FolderStore_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog FolderDialog = new FolderBrowserDialog();           
            FolderDialog.ShowDialog();

            FolderSave = FolderDialog.SelectedPath;
        }        

        private void Help_Click(object sender, RoutedEventArgs e)
        {           
            var help = new System.Diagnostics.Process();
            help.StartInfo.FileName = @"Archiver Revolution.chm";
            help.StartInfo.UseShellExecute = true;           
            help.Start();            
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        //Обробка дії натиску кнопок
        #region Button Click

        void start()
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Title = "Вибиріть файл для " + choice + ":";
            fileDialog.ShowDialog();

            if (fileDialog.FileName != "")
            {
                trdProcess = new Thread(new ThreadStart(this.ThreadTaskProcesing));
                trdProcess.IsBackground = true;
                trdProcess.Start();

                progressW = new ProgressWindow();
                progressW.Show();

            }
            // textBox1.Text = "" + ArchivingLibrary.BWT.pr;
        }

        private void Encoder_Click(object sender, RoutedEventArgs e)
        {
            choice = "Encoder";
            start();
        }

        private void Decoder_Click(object sender, RoutedEventArgs e)
        {
            choice = "Decoder";
            start();
        }

        private void Compress_Click(object sender, RoutedEventArgs e)
        {
            choice = "Compress";
            start();
        }

        private void Decompress_Click(object sender, RoutedEventArgs e)
        {
            choice = "Decompress";
            start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (progressW.Visibility == Visibility.Visible) progressW.Close();
            }
            catch (System.Exception ex)  { }          
        }

        #endregion
    }
}
