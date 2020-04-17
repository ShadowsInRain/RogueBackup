using System;
using System.IO;

namespace RogueBackup
{
    class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            MainImpl(args);
#else
            WriteLineDelegate log = Console.WriteLine;
            try
            {
                MainImpl(args);
            }
            catch (BoringException e)
            {
                // for exceptions that may happen before core loop
                log($"Error: {e.Message}");
                log("Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                log();
                log("*** Crash log ***");
                log();
                log(e.ToString());
                log();
                log("Program has crashed. Press any key to exit.");
                Console.ReadKey();
                throw;
            }
#endif
        }

        static void MainImpl(string[] args)
        {
            var userIO = new ConsoleUserIO();
            var config = new Config();
            CommandLine.Parse(args, config);
            var service = new Service(config);
            var mainMenu = new MainMenu(userIO, config, service);

            mainMenu.Welcome();
            var interpreter = mainMenu as Repl;
            while (interpreter != null)
            {
                try
                {
                    interpreter = interpreter.Step();
                }
                catch (Exception e) when (e is IOException)
                {
                    userIO.WriteLine($"System error: {e.Message}");
                }
                catch (BoringException e)
                {
                    userIO.WriteLine($"Error: {e.Message}");
                }
            }
        }
    }
}
