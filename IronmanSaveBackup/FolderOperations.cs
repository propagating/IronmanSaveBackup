using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronmanSaveBackup
{
    class FolderOperations
    {
        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start(path);

            }
            else
            {
                MessageOperations.UserMessage(MessageOperations.MessageTypeEnum.DoesNotExistError);
                //TODO: Create a dialog saying no folder has been selected, and/or no default folder has been set
            }
        }
    }
}
