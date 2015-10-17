#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using ArchivingLibrary;
using System.Windows.Forms;
using System.IO;
using System.Threading;
#endregion

namespace AppConsole
{  
    class ConsoleMenu
    {
        #region Fields

        public String title;
        public List<String> strCommands;

        public int SelectIndex;
        public bool Visible = true;
        public bool isExit;
        String Direction;

        Object colorText;
        Object colorSelection;

        public event EventHandler PressItem;


        Thread trdProcess; // Поток процесу
        Thread timerProgress;
        int progressValue = 0;

        OpenFileDialog fileDialog; // Діалогове вікно відкриття файлів

        String choice; // Вибір операції
        String FolderSave = "D:/"; // Шлях збереження файлів після виконання операції

        #endregion

        #region Constructors

        public ConsoleMenu()
        {
            strCommands = new List<String>();
            SelectIndex = 0;

            colorText = ConsoleColor.White;
            colorSelection = ConsoleColor.Blue;

            PressItem += new EventHandler(OnPressItem); 
        }

        public ConsoleMenu(String _title)
        {
            strCommands = new List<String>();
            SelectIndex = 0;

            title = _title;
            colorText = ConsoleColor.White;
            colorSelection = ConsoleColor.Blue;

            PressItem += new EventHandler(OnPressItem); 
        }

        public ConsoleMenu(String _title, Object _colorText, Object _colorSelection)
        {
            strCommands = new List<String>();
            SelectIndex = 0;

            title = _title;
            colorText = _colorText;
            colorSelection = _colorSelection;

            PressItem += new EventHandler(OnPressItem);
        }

        public void AddItems()
        {
            strCommands.Add("Кодування");
            strCommands.Add("Декодування");
            strCommands.Add("-");
            strCommands.Add("Стиснути");
            strCommands.Add("Розтиснути");
            strCommands.Add("-");
            strCommands.Add("Папка збереження");
            strCommands.Add("Довiдка");
            strCommands.Add("Вихiд (Esc)");
        }

        #endregion

        #region Event
        
        public void OnPressItem(object sender, EventArgs e)
        {
            switch (SelectIndex)
            {
                case 0:
                    {
                        choice = "Encoder";
            start();
                    } break;
                case 1:
                    {
                        choice = "Decoder";
            start();
                    } break;
                case 3:               
                    {
                       choice = "Compress";
            start(); 
                    } break;
                case 4:
                    {
                         choice = "Decompress";
            start();
                    } break;
                case 6:
                    {
                        FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
                        FolderDialog.ShowDialog();

                        FolderSave = FolderDialog.SelectedPath;
                    } break;
                case 7:
                    {
                        var help = new System.Diagnostics.Process();
                        help.StartInfo.FileName = @"Archiver Revolution.chm";
                        help.StartInfo.UseShellExecute = true;
                        help.Start();      
                    } break;
                case 8:
                    {
                        isExit = true;
                    } break;
            }
        }

        #endregion

        #region Other

        void ProcessingBar()
        {
            Visible = false;
            while (progressValue <= ArchivingLibrary.BWT.progress)
            {
                Console.Beep(1000 - progressValue, 15);
                Console.WriteLine(progressValue);
                progressValue += 5;
            }
            progressValue = 0;
            Console.WriteLine(choice + " завершено.\nДля продовження натиснiть любу кнопку.");
            Console.ReadKey();            
            Visible = true;          
        }

        void start()
        {
            fileDialog = new OpenFileDialog();
            fileDialog.Title = "Вибиріть файл для " + choice + ":";
            fileDialog.ShowDialog();

            if (fileDialog.FileName != "")
            {
                try
                {
                    trdProcess = new Thread(new ThreadStart(this.ThreadTaskProcesing));
                    trdProcess.IsBackground = true;                   
                    trdProcess.Start();

                    timerProgress = new Thread(new ThreadStart(this.ProcessingBar));
                    timerProgress.IsBackground = true;
                   // timerProgress.Priority = ThreadPriority.Normal;
                    timerProgress.Start();
                }
                catch (System.Exception ex) { }                
            }
        }

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

            ArchivingLibrary.BWT.Clear();

            trdProcess.Abort();
            timerProgress.Abort();
        }

        public int maxStrList()
        {
            int max = 0;
            for (int i = 0; i < strCommands.Count; i++)
                if (strCommands[max].Length < strCommands[i].Length)
                    max = i;
            return strCommands[max].Length;
        }

        #endregion

        #region Draw

        public void DrawPart()
        {
            Console.Write("  ");
            for (int i = 0; i < maxStrList() + 4; i++)
                Console.Write("{0}", "-");
            Console.WriteLine();
        }

        public void DrawLines()
        {
            Console.Clear();

            int maxStr = maxStrList();

            for (int j = 0; j <= ((maxStr + 3) - title.Length); j++)
                Console.Write("{0}", " ");
            Console.WriteLine("{0}", title);           

            DrawPart();

            int i = 0, index = 1;          
            foreach (String str in strCommands)
            {
                if (i == SelectIndex)
                {
                    Console.BackgroundColor = (ConsoleColor)colorSelection;
                    Console.ForegroundColor = (ConsoleColor)colorText;                 
                }
                if (str != "-")
                {
                    Console.Write("| {0} ", index);
                    Console.Write("{0} ", str);
                    for (int j = 0; j < maxStr - strCommands[i].Length; j++)
                        Console.Write(" ");
                    Console.WriteLine(" |");
                    index++;
                }
                 else
                {
                    DrawPart();
                }

                Console.ResetColor();
                i++;
            }
            DrawPart();
        }

        #endregion

        #region Processing

        public void Processing()
        {
            while (1 == 1)
            {
                if (Visible)
                {
                    DrawLines();

                    ConsoleKeyInfo key = System.Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            {                                
                                SelectIndex--;
                                Direction = "Up";
                            } break;
                        case ConsoleKey.DownArrow:
                            {
                                SelectIndex++;
                                Direction = "Down";
                            } break;
                        case ConsoleKey.Enter:
                        case ConsoleKey.Spacebar:
                            {
                                Console.Beep(500, 100);
                                PressItem(this, new EventArgs());
                            } break;
                        case ConsoleKey.Escape:
                            {
                                return;
                            }
                    }

                    if (SelectIndex >= strCommands.Count) SelectIndex = 0;
                    if (SelectIndex < 0) SelectIndex = strCommands.Count - 1;
                    
                    if (strCommands[SelectIndex] == "-" && Direction == "Up") SelectIndex--;
                    if (strCommands[SelectIndex] == "-" && Direction == "Down") SelectIndex++;

                    if (isExit) return;                   
                }
            }
        }

        #endregion
    }
}
