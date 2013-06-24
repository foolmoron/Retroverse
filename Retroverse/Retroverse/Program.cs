using System;
using System.Windows.Forms;

namespace Retroverse
{
#if WINDOWS
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (RetroGame game = new RetroGame())
            {
#if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += GenericExceptionHandler;
#endif
                game.Run();
            }
        }

        static void GenericExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            string exceptionName = exception.GetBaseException().GetType().ToString();
            string title = "Retroverse crashed unexpectedly!";
            string message = "ERROR: " + exceptionName + "\n" + exception.Message + "\n\n";
            message += exception.StackTrace.Replace(@"C:\Users\foolmoron\Documents\Git\Retroverse\Retroverse\Retroverse\", "");
            message += "\n\nPress [ctrl+C] to copy this error (even though you can't select the text), and send it to 'foolmoron@gmail.com' or 'foolmoron' on Desura";
            MessageBox.Show(message, title, MessageBoxButtons.OK);
            Environment.Exit(0);
        }
    }
#endif
}

