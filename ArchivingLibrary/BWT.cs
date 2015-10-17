#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#endregion

namespace ArchivingLibrary
{
    /// <Реалізація алгоритму BWT (Перетворення Барроуза - Уилера)>
    ///  Змінює порядок символів у вхідний рядку таким чином, що повторюються 
    ///  підрядка утворюють на виході йдуть підряд послідовності однакових символів. 
    ///  Таким чином, поєднання BWT і RLE виконує завдання стиснення винятком повторюваних 
    ///  підрядків, тобто завдання, аналогічну алгоритмам LZ.
    /// </Реалізація алгоритму BWT (Перетворення Барроуза - Уилера>
    public static class BWT
    {
        #region Fields

        static byte[] workdata; // Масив даних
        static int[] ipoints; // Масив ключів сортування

        public static Double progress; //Прогерес виконання алгоритму      

        #endregion

        public static void Clear()
        {
            workdata = null;
            ipoints = null; 
        }

        #region Sort RecursiveBucketSort & RecursiveQuickSort

        /// <Рекурсивне сортування комірками>
        ///  Сортування, яке працює шляхом поділу масиву в число сегментів. 
        ///  Вони потім сортуються індивідуально.
        /// </Рекурсивне сортування комірками>
        /// <param name="istart">Початок масиву</param>
        /// <param name="iend">Кінець масиву</param>
        /// <param name="depth">Глибина рекурсії</param>
        /// <param name="step">Крок</param>
        private static void RecursiveBucketSort(int istart, int iend, short depth, short step)
        {

            int q = 0, npos;
            short num;
            byte[] values;
            int[] counter, sortpos, startpoint;
            Hashtable tmp = new Hashtable();

            if (iend - istart < 100)
            {
                RecursiveQuickSort(istart, iend, step);
                return;
            }

            // Виконати грубого сортування масиву
            counter = new int[256];
            for (int i = istart; i <= iend; i++)
            {
                tmp.Add(i, ipoints[i]);
                q = workdata[ipoints[i] + step];
                counter[q]++;
            }

            try
            {
                if (counter[q] == iend - istart + 1) // Тільки 1 символ знайдено
                {
                    if (step == depth)
                        RecursiveQuickSort(istart, iend, (short)(step + 1));
                    else
                        RecursiveBucketSort(istart, iend, depth, (short)(step + 1));
                    return;
                }
                else
                {
                    values = new byte[256];
                    sortpos = new int[255];
                    startpoint = new int[256];
                    npos = istart;
                    num = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        if (counter[i] > 0)
                        {
                            startpoint[i] = npos;
                            sortpos[i] = npos;
                            npos += counter[i];
                            values[num] = (byte)i;
                            num++;
                        }
                    }
                    //Останнє місце, де знаходиться показник порядку.
                    for (int i = istart; i <= iend; i++)
                    {
                        q = workdata[(int)tmp[i] + step];
                        ipoints[sortpos[q]] = (int)tmp[i];
                        sortpos[q]++;
                    }
                    step++;

                    // Початок рекурсії
                    for (int i = 0; i < num; i++)
                    {
                        q = values[i];
                        RecursiveBucketSort(startpoint[q], startpoint[q] + counter[q] - 1, depth, step);
                    }
                }

            }
            catch (System.Exception ex) { }
        }

        /// <Рекурсивне швидке сортування>
        /// Переставляє елементи масиву таким чином, 
        /// щоб його можна було розділити на дві частини і 
        /// кожний елемент з першої частини був не більший за будь-який елемент з другої. 
        /// </Рекурсивне швидке сортування>
        /// <param name="istart">Початок масиву</param>
        /// <param name="iend">Кінець масиву</param>
        /// <param name="step">Крок</param>
        private static void RecursiveQuickSort(int istart, int iend, short step)
        {
            int y, x, l = istart, r = iend, t, newstep = 100000, cstep;
            bool d = false;
            // Перетворення в графічний вигляд масиву
            if (l >= r) return;
            while (l < r)
            {
                cstep = 0;
                y = ipoints[l] + step;
                x = ipoints[r] + step;
                while (workdata[y] == workdata[x])
                {
                    y++;
                    x++;
                    cstep++;
                }
                if (cstep < newstep) newstep = cstep;
                if (workdata[x] < workdata[y])
                {
                    t = ipoints[l];
                    ipoints[l] = ipoints[r];
                    ipoints[r] = t;
                    d = !d;
                }
                if (d)
                    r--;
                else
                    l++;
            }
            step += (short)newstep;
            RecursiveQuickSort(istart, l - 1, step);
            RecursiveQuickSort(r + 1, iend, step);
        }

        #endregion

        #region Coding

        /// <Кодування>
        /// Виконаня кодування вхідних даних тобто групування елементів.
        /// </Кодування>
        /// <param name="source">Масив даних</param>
        /// <param name="offset">Індекс оригінального рядку</param>
        public static void Encoder(ref byte[] source, ref int offset)
        {
            progress = 10;
            ipoints = new int[source.Length];
            workdata = new byte[source.Length * 2];

            for (int i = 0; i < source.Length; i++)
            {
                workdata[i] = source[i];
                ipoints[i] = i;
            }

            progress += 20;

            for (int i = 0; i < source.Length; i++)
            {
                workdata[source.Length + i] = source[i];
            }

            progress += 45;

            //Сортування               
            RecursiveBucketSort(0, source.Length - 1, 7, 0);

            progress += 20;

            for (int i = 0; i < source.Length; i++)
            {
                if (ipoints[i] == 1) offset = i;
                if (ipoints[i] == 0)
                    source[i] = workdata[source.Length - 1];
                else
                    source[i] = workdata[ipoints[i] - 1];
            }

            progress += 5;
        }

        /// <Декодування>
        /// Виконаня декодування вхідних даних тобто перетворення масиву до кодувального виду.
        /// </Декодування>
        /// <param name="source">Масив даних</param>
        /// <param name="offset">Індекс оригінального рядку</param>
        public static void Decoder(ref byte[] source, int offset)
        {
            progress = 10;
            int npos = 0;
            int[] transformvector = new int[source.Length];
            int[] position = new int[256];
            int[] counter = new int[256];
            byte[] decoded = new byte[source.Length];

            // Використання швидкого метод сортування масиву дає можливість не робити це лексикографічно
            for (int i = 0; i < source.Length; i++)
                counter[source[i]]++;

            progress += 20;

            // Місце елементів у масиві
            for (int i = 0; i < 256; i++)
            {
                position[i] = npos;
                npos += counter[i];
            }

            progress += 30;

            // Оригінальний і впорядкований масив тепер можна побудувати перетворення таблиць
            for (int i = 0; i < source.Length; i++)
            {
                transformvector[position[source[i]]] = i;
                position[source[i]]++;
            }

            progress += 35;

            // З використанням перетворення таблиць і коду розташування елементу, коли можна відновити вихідні дані
            for (int i = 0; i < source.Length; i++)
            {
                decoded[i] = source[offset];
                offset = transformvector[offset];
            }
            source = decoded;

            progress += 5;
        }

        #endregion

        #region Compress & Decompress

        /// <Архівування>
        /// Виконання архівування за допомогою алгоритму RLE (Кодування повторів).
        /// </Архівування>
        /// <param name="source">Масив даних</param>
        public static void Compress(ref byte[] source)
        {
            int offset = 0;
            Encoder(ref source, ref offset);

            int size = source.Length;
            List<byte> tmpSource = new List<byte>();
            List<byte> tmpCount = new List<byte>();
            /* 
             int count = 0;

             for (int i = 0; i < size - 1; i++)
             {
                 if (!source[i].Equals(source[i + 1]))
                 {
                     tmpSource.Add(source[i]);
                     tmpCount.Add((byte)count);
                     count = 0;
                 }
                 count++;
             }          
             tmpSource.Add(source[size - 1]);  
             tmpCount.Add(Convert.ToByte(count));

             tmpSource.AddRange(tmpCount);*/

            RLE rleEncode = new RLE();
            source = rleEncode.EncodeData(source);

            tmpSource.AddRange(source);
            // Запис індексу оригінального рядку.   
            byte[] tmpOffset = BitConverter.GetBytes((ushort)offset);
            tmpSource.AddRange(tmpOffset);

            source = tmpSource.ToArray();
        }

        /// <Розпакування>
        /// Виконання розпакування за допомогою алгоритму RLE (Кодування повторів).
        /// </Розпакування>
        /// <param name="source">Масив даних</param>
        public static void Decompress(ref byte[] source)
        {
            #region Зчитування і видалення індексу оригінального рядку

            int size = source.Length;
            List<byte> tmpSource = new List<byte>(source);
            int offset = (int)BitConverter.ToUInt16(tmpSource.ToArray(), (size - 2));

            tmpSource.RemoveAt(size - 2);
            tmpSource.RemoveAt(size - 2);

            source = tmpSource.ToArray();

            #endregion

            tmpSource.Clear();
            size -= 2;

            /*  for (int i = 0; i < size / 2; i++)
              {
                  int j = 0;                
                  do
                  {
                      tmpSource.Add(source[i]);
                      j++;
                  } while (j < (source[i + size / 2]));
              }
              source = tmpSource.ToArray();   */

            RLE rleDecode = new RLE();
            source = rleDecode.DecodeData(source);

            Decoder(ref source, offset);
        }

        #endregion
    }
    

    #region  test

    class test
    {
        /*
        byte[] EncodeMP(byte[] buf_in)
        {
            int size_m = buf_in.Length;
            int[] indices = new int[size_m];

            #region Спосіб 1
            /* 
            for (int i = 0; i < size_m; i++)
                indices[i] = i;

            byte[][] matr = new byte[size_m][];
            for (int i = 0; i < size_m; i++)
                matr[i] = new byte[size_m];

            matr[0] = (byte[])buf_in.Clone();

            for (int i = 1; i < size_m; i++)
            {
                byte tmp = matr[i - 1][0];
                matr[i] = (byte[])matr[i - 1].Clone();

                for (int j = 0; j < size_m - 1; j++)
                    matr[i][j] = matr[i][j + 1];
                matr[i][size_m - 1] = tmp;

            }

             ///////
             print_matr(matr, size_m, indices, richTextBox1);
             ////////////

             for (int i = 0; i < size_m; ++i)
             {
                 for (int j = size_m - 1; j > i; --j)
                 {
                     if (matr[j][0] < matr[j - 1][0] )
                     {
                         byte[] temp = matr[j];
                         matr[j] = matr[j - 1];
                         matr[j - 1] = temp;
                     }                   
                 }
             }

             //запис в рядок
          /*   string[] str = new string[matr.Length];
             for (int i = 0; i < matr.Length; i++)
             {
                 str[i] = "";
                 for (int j = 0; j < matr.Length; j++)
                     str[i] += (char)matr[i][j];
             }

             //сортування
             Array.Sort(str);

             //запис в матрицю
             for (int i = 0; i < matr.Length; i++)
                 for (int j = 0; j < matr.Length; j++)
                 {
                     matr[i][j] = (byte)str[i][j];
                 }        */

        /*                    
          ///////////////////
          richTextBox1.Text += '\n';
          print_matr(matr, size_m, indices,richTextBox1);
          ///////////////////
                        
          int primary_index = -1;
          bool isEquals = false;
          for (int i = 0; i < size_m; i++)
          {
              for (int j = 0; j < size_m; j++)
                 if (matr[i][j] == buf_in[j])
                     isEquals = true;
                  else break;
              if (isEquals) primary_index = i;
              isEquals = false;
          }
            

          byte[] bufResult = new byte[size_m + 1];
            
          bufResult[0] = (byte) primary_index;
          for (int i = 1; i < size_m + 1; i++)
           bufResult[i] = matr[i - 1][size_m - 1];
         */
        /*   #endregion


           #region Спосіб 2
           /*
           // Array.Reverse(indices); //quick
               
          //  Quicksort.Sort(indices,size_m,-1);

            for (int j = 0; j < size_m; j++)
                richTextBox1.Text += Convert.ToChar(buf_in[j]) + ",";
            richTextBox1.Text += "\n";
            for (int j = 0; j < size_m; j++)
                richTextBox1.Text += indices[j] + ",";

            int primary_index = -1;
            byte[] bufResult = new byte[size_m + 1];

            for (int i = 1; i < size_m; i++)
                bufResult[i] = buf_in[(indices[i - 1] + size_m - 1) % size_m];
            for (int i = 1; i < size_m; i++)
            {
                if (indices[i] == 1)
                {
                    primary_index = i;
                    break;
                }
            }

            bufResult[0] = (byte) primary_index;

            richTextBox1.Text += "\n";
            for (int j = 0; j < size_m + 1; j++)
                richTextBox1.Text += (bufResult[j]) + ",";
           */
        /*  #endregion


          #region Спосіб 3
          /*
          byte sumvol = buf_in[0];

          byte[] buf_out = (byte[])buf_in.Clone();

          byte tmp = buf_out[0];

          for (int j = 0; j < size_m - 1; j++)
              buf_out[j] = buf_out[j + 1];
          buf_out[size_m - 1] = tmp;

          ///////////
          for (int j = 0; j < size_m; j++)
              richTextBox1.Text += Convert.ToChar(buf_in[j]) + ",";
          richTextBox1.Text += '\n';
          for (int j = 0; j < size_m; j++)
              richTextBox1.Text += Convert.ToChar(buf_out[j]) + ",";
          ///////

          Array.Sort(buf_in, buf_out);

          ///////////
          richTextBox1.Text += '\n';
          for (int j = 0; j < size_m; j++)
              richTextBox1.Text += Convert.ToChar(buf_in[j]) + ",";
          richTextBox1.Text += '\n';
          for (int j = 0; j < size_m; j++)
              richTextBox1.Text += Convert.ToChar(buf_out[j]) + ",";
          ///////

          int primary_index = -1;

          for (int j = 0; j < size_m; j++)
              if (buf_in[j] == sumvol)
              {
                  primary_index = j;
                  break;
              }


          byte[] bufResult = new byte[size_m + 1];

          bufResult[0] = (byte)primary_index;

          for (int j = 1; j < size_m + 1; j++)
              bufResult[j] = buf_out[j - 1];
          */
        /*  #endregion

       /*   return bufResult;
      }

     /* byte[] DecodeMP(byte[] buf_in)
      {
         /* int size_m = buf_in.Length;

          int primary_index = buf_in[0];

          textBox2.Text += " primary_index=" + primary_index;

          byte[][] matr = new byte[size_m][];
          for (int i = 0; i < size_m; i++)
              matr[i] = new byte[size_m];


          //////////////
          print_matr(matr, size_m, null, richTextBox2);
          /////////////

          byte[] sortBuf = (byte[])buf_in.Clone();

          Array.Sort(sortBuf);
          //  Array.Sort(sortBuf, buf_in);

          for (int i = 0; i < size_m; i++)
              matr[i][1] = buf_in[i];

          for (int i = 0; i < size_m; i++)
              matr[i][0] = sortBuf[i];

          ///////////////////////////////////////
          richTextBox2.Text += '\n';
          print_matr(matr, size_m, null, richTextBox2);
          ////////////////////////////////*/

        /* int[][] numericMatr = new int[size_m][];
         for (int i = 0; i < size_m; i++)
             numericMatr[i] = new int[2];
         for (int i = 0; i < size_m; i++)
             numericMatr[i][0] = i;

         for (int i = 0; i < size_m; i++)
         {
             for (int j = 0; j < size_m; j++)
                 if (matr[i][0] == matr[j][1] && i != j)
                 {
                     bool isExit = true;
                     numericMatr[i][1] = j;
                     for (int ii = 0; ii < i; ii++)
                         if (numericMatr[ii][1] == j) isExit = false;
                     if (isExit) break;
                 }
         }

         //вивод мас 2-о мір numericMatr[ ][ ]
         richTextBox2.Text += '\n';
         for (int i = 0; i < size_m; i++)
         {
             for (int j = 0; j < 2; j++)
                 richTextBox2.Text += numericMatr[i][j];
             richTextBox2.Text += '\n';
         }
         ///////////////////////////////////////*/

        //граф
        /* int searchNumber = primary_index + 1;
         int[] graf = new int[size_m];
         for (int i = 0; i < size_m; i++)
         {
             for (int j = 0; j < size_m; j++)
                 if (numericMatr[j][0] == searchNumber)
                 {
                     graf[i] = searchNumber;
                     searchNumber = numericMatr[j][1];
                     break;
                 }
         }
         ////////////////////////////////////////
         richTextBox2.Text += "Граф:\n";
         for (int j = 0; j < size_m; j++)
             richTextBox2.Text += graf[j];
         richTextBox2.Text += "\n";
         for (int j = 0; j < size_m; j++)
             richTextBox2.Text += Convert.ToChar(matr[graf[j]][0]);
         /////////////////////////////////////*/

        /* byte[] bufResult = new byte[size_m];
         for (int j = 0; j < size_m; j++)
             bufResult[j] = matr[graf[j]][0];

         // Array.Reverse(bufResult);             

         List<byte> tmpResult = bufResult.ToList();
         tmpResult.RemoveAt(size_m - 1);
         */
        /*return tmpResult.ToArray();
    }
}*/
    }

    #endregion

}
