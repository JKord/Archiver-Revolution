#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace ArchivingLibrary
{
    /// <Реалізація алгоритму RLE (Кодування повторів)>
    /// пишуться двійки значення-кількість (по байту)
    /// нульвої кількості не буває тому кожен лічильник повторів
    /// при декодуванні буде рівним отриманому значенню лічильника + 1 (1 .... 256) 
     // якщо лічильник <= 128 - це кількість повторів певного значення (наступний байт)
     // якщо лічильник > 128 - це кількість пропуску - подальших прямо записаних байт
    /// (лічильник - 128). максимум прямо записаних байт може бути 256 - 128 (тобто 128)
    /// </Реалізація алгоритму RLE (Кодування повторів)>>
    public unsafe class RLE
    {

        [DllImport("ntdll.dll", EntryPoint = "memcpy")]
        internal static extern byte* CopyMemory(byte* destination, byte* source, int length);

        #region Encode       

        #region Функції повертаючі кодовані дані у вигляді масиву байт       

        public byte[] EncodeData(byte[] source)
        {
            return EncodeData(source, 0, source.Length);
        }

        public byte[] EncodeData(byte[] source, int count)
        {
            return EncodeData(source, 0, count);
        }

        public byte[] EncodeData(byte[] source, int start, int count)
        {
            byte[] buff = new byte[count * 2];
            byte[] tmp;
            if (start == 0)
                tmp = source;
            else
            {
                tmp = new byte[count];
                System.Buffer.BlockCopy(source, start, tmp, 0, count);
            }
            int l = Encode(tmp, count, buff);
            Array.Resize(ref buff, l);
            return buff;
        }

        #endregion
        
        /// <Кодування даних з вхідного масиву до вихідного>
        ///  Кодування до виділеного буферу
        /// </Кодування даних з вхідного масиву до вихідного>
        /// <param name="source">Вхідний масив</param>
        /// <param name="encodecount">Скільки байт кодувати</param>
        /// <param name="destination">Вихідний масив</param>
        /// <returns>Кількість записаних до вихідного масиву байт</returns>
        public int Encode(byte[] source, int encodecount, byte[] destination)
        {
            int p1, p2;
            fixed (byte* psource = source, pdest = destination)
            {
                // вказівник читання вхідних даних
                byte* ps = psource; // вказівник на перший байт даних
                byte* pd = pdest; // вказівник на перший байт даних
                int current = 0;
                int next = 0;
                int count; // кількість
                int copy = 0;
                // обробка вхідних даних
                while (current < encodecount)
                {
                    // пошук наступної позиції
                    next = GetNext(psource, encodecount, current, out count);
                    if (count == 0) // дані мають бути зкопійованими
                    {
                        copy = encodecount - current; // скільки копіювати
                        if (copy > 128) // більше максимальної кількості пропуску
                            copy = 128;
                        *pd++ = (byte)(127 + copy); // запис лічильника пропуска
                        // копіювання фрагменту
                        RLE.CopyMemory(pd, ps, copy);
                        pd += copy;
                        ps += copy;
                        current += copy;
                    }
                    else if (next != current) // певний фрагмент даних має бути записаним напряму до запису пари кількість-значення
                    {
                        copy = next - current;  // скільки копіювати
                        *pd++ = (byte)(127 + copy); // запис лічильника пропуска
                        // копіювання фрагмента
                        RLE.CopyMemory(pd, ps, copy);
                        pd += copy;
                        ps += copy;
                        current += copy;
                        // після чого має записатись пара лічильник-значення
                        *pd++ = (byte)(count - 1); // лічильник
                        *pd++ = *ps; // значення
                        ps += count; // зміщення вказівника на певну кількість байт
                        current += count; // зміщення позиції читання
                    }
                    else // пара лічильник-значення одразу пишеться
                    {
                        *pd++ = (byte)(count - 1); // лічильник
                        *pd++ = *ps; // значення
                        ps += count;
                        current += count;
                    }
                }
                p2 = *((int*)&pd); // кінцева позиція
                pd = pdest;
                p1 = *((int*)&pd); // початкова позиція
            }
            // повернення кількості кодованих байт
            return p2 - p1;
        }

        /// <GetNext>
        ///  Функція шукає позицію наступної послідовності не меньше трьох повторів
        ///  і повертає цю позицію, передаючи кількість за допомогою аргументу count
        ///  фрагмент даних до знайденої позиції (якщо він буде) має кодуватись
        ///  байтом лічильника пропуску ( + фрагмент прямо записаних байт) 
        /// </GetNext>
        /// <param name="src">Вказівник на перший байт даних</param>
        /// <param name="limit">Скільки байт кодувати</param>
        /// <param name="start">Позиція початку</param>
        /// <param name="count">Кількість повторів</param>
        /// <returns>Позицію наступної послідовності</returns>
        private int GetNext(byte* src, int limit, int start, out int count)
        {
            byte* test = src + start; // вказівник на перший байт даних початку перевірки
            byte testvalue = 0; // значення перевірки
            int testcount = 0; // кількість повторів

            // перегляд вхідних даних починаючи з вказаної позиції
            // доки не буде набрано максимальний відступ чи не досягнено кінець
            for (int i = 0; i < 128 && start + i < limit; i++)
            {
                testvalue = test[i]; // байт перевірки
                testcount = 1; // кількість
                // перевірка наступного байту
                while (testcount < 128 && start + i + testcount < limit - 1)
                {
                    if (test[i + testcount] != testvalue)
                        break; // не відповідає
                    else
                        testcount++; // нарощення лічильника
                }
                // перевірка виявленої кількості повторень
                if (testcount > 2) // знайдена позиція нормальної послідовності
                {
                    count = testcount; // кількість повторів
                    return start + i; // повернення позиції
                }
            }
            // не було знайдено послідовності щоб закодувати парою кількість-значення
            // знач починаючи з поточної позиції фрагмент має бути зкопійованим як є 
            count = 0;
            return start;
        } // FindNext

        #endregion

        #region Decode

        #region Функції повертаючі кодовані дані у вигляді масиву байт

        public byte[] DecodeData(byte[] source)
        {
            return DecodeData(source, 0, source.Length);
        }

        public byte[] DecodeData(byte[] source, int count)
        {
            return DecodeData(source, 0, count);
        }

        public byte[] DecodeData(byte[] source, int start, int count)
        {
            byte[] buff = new byte[1024 * 1024]; // мегабайт
            byte[] tmp;
            if (start == 0)
                tmp = source;
            else
            {
                tmp = new byte[count];
                System.Buffer.BlockCopy(source, start, tmp, 0, count);
            }
            int l = Decode(tmp, count, buff);
            Array.Resize(ref buff, l);
            return buff;
        }

        #endregion
                
        /// <Декодування даних з вхідного масиву до вихідного>
        ///  Декодування до виділеного буферу
        /// </Декодування даних з вхідного масиву до вихідного>
        /// <param name="source">Вхідний масив</param>
        /// <param name="decodecount">Скільки байт декодувати</param>
        /// <param name="destination">Вихідний масив</param>
        /// <returns>Кількість записаних до вихідного масиву байт</returns>
        public int Decode(byte[] source, int decodecount, byte[] destination)
        {
            // вихідні дані
            int count = 0; // кількість повторів
            int copycount = 0; // кількість байт прямого копіювання
            // режим лічильника чи значення
            // алгоритм індикує таким чином про набір лічильника чи зміну значення 
            bool counter = true;
            int n = 0, p1, p2;
            fixed (byte* psource = source, pdest = destination)
            {
                byte* ps = psource;
                byte* pd = pdest;
                for (; ; )
                {
                    if (counter) // лічильник 
                    {
                        count = ((int)*ps++) + 1; // кількість
                        if (count > 128) // прямо записаний фрагмент
                            copycount = count - 128; // кількість прямо записаних байт
                        n++; // прирощення лічильника прочитаного
                    }
                    else // значення
                    {
                        if (copycount > 0) // пряме копіювання фрагменту вхідних даних
                        {
                            RLE.CopyMemory(pd, ps, copycount);
                            pd += copycount;
                            ps += copycount;
                            n += copycount;
                            copycount = 0;
                        }
                        else // повторити значення певну кількість разів
                        {
                            for (int i = 0; i < count; i++)
                                *pd++ = *ps; // запис байта вхідних даних певну кількість раз
                            ps++; // зміщення
                            n++;
                        }
                    }
                    counter = !counter; // зміна режиму на протилежний
                    // дані оброблено
                    if (n >= decodecount) break;
                }
                p2 = *((int*)&pd); // кінцева позиція
                pd = pdest;
                p1 = *((int*)&pd); // початкова позиція
            }
            // повернення кількості декодованих байт
            return p2 - p1;
        } 

        #endregion

    } 

}
