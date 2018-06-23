using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderWatcher
{
    class Program
    {
        /*
            This is a diagnostic program to grab all of the file system events that take place during an Iron Man Save
                  
            The events that take place during each save in XCOM 2 are as follows:
        
            1) delete old save 
            2) create temp#### save, increments by 1 each new save
            3) rename to the original name
            4) populate the new file with save data
        
            Also discovered a bit separately.. each save does not necessarily build on the previous save. 
            Even in a normal game, the length of each save is independent of the previous save. 
            I think this is due to the save not storing the actual actions taken during the last turn, only the state of each actor when the save takes place
            Why is this important? I'm hoping it can eventually provide insight into why the Ironman saves have the issues that they do. 
            
            Some other things to note: 
            A save event is kicked off when you press the 'Load' button while in Ironman, it does not kick off a save every turn
            A save event may occur if you see a pod, VIP, new enemy, tower to hack, or really any entity that you can interact with (even if you do not interact with/trigger them)
            This also leads to some odd cases where saves take place in the middle of the player's turn -- an area worth investigating for potential corruption
            Some save events can occur while your actor is in the middle of moving/dying/really doing anything -- another area worth investigating for corruption, combined with a few other quirks in the system could cause some unintended behavior
            A save event seems to occur each time an objective is completed
            Saves on the avenger appear to have the same steps as the saves during a mission, though they are triggered by some vents that I'm still not 100% on
        */
        static void Main(string[] args)
        {
            //TODO: Update to the path that your saves are located 
            var filePath = args[0];
            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = filePath;
                watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess |
                                       NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite;

                //TODO: Update to the naming scheme of your ironman saves. 
                watcher.Filter = "save_IRONMAN*";
                watcher.Changed += new FileSystemEventHandler(OnEvent);
                watcher.Created += new FileSystemEventHandler(OnEvent);
                watcher.Deleted += new FileSystemEventHandler(OnEvent);
                watcher.Renamed += new RenamedEventHandler(OnRename);

                watcher.EnableRaisingEvents = true;
                Console.WriteLine("Press 'q' to quit.");
                while (Console.ReadLine() != "q")
                {
                }
            }

        }

        static void OnEvent(object soruce, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} \n{e.ChangeType}\n");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine("----------------------------------------");

        }

        static void OnRename(object source, RenamedEventArgs e)
        {
            Console.WriteLine($"File: {e.OldFullPath} renamed \n {e.FullPath}\n");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine("----------------------------------------");
        }
    }
}
