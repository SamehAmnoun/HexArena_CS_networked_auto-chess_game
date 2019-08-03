﻿using ASU2019_NetworkedGameWorkshop.controller;
using System;
using System.Windows.Forms;

namespace ASU2019_NetworkedGameWorkshop
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());
 
            //Application.Run(new ConnectForm());
        }
    }
}
