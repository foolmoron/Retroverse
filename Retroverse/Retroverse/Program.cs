using System;

namespace Retroverse
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (RetroGame game = new RetroGame())
            {
                game.Run();
            }
        }
    }
#endif
}

