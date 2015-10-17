#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using ArchivingLibrary;
#endregion

namespace AppConsole
{
    class Program
    {
        #region Fields

        #endregion        

        [STAThreadAttribute]
        static void Main(string[] args)
        {           
            // System.ConsoleColor                       

            Console.Title = "Архiватор BWT + RLE";
        
            ConsoleMenu menu = new ConsoleMenu("*Меню Архiватора*");
            menu.AddItems();
                       
            menu.Processing();          
        }
    }
}
