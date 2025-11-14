using System;
using System.Windows.Forms;
using Act2_Mizal;

namespace Act2_Mizal
{
   internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}