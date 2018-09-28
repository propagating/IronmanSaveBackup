using System.IO;
using System.Windows;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    internal class FolderOperations
    {
        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
            else
            {
                MessageOperations.UserMessage(Resources.FolderNotFound, MessageOperations.MessageTypeEnum.DoesNotExistError);
            }
        }

        private FolderOperations()
        {

        }
    }
}
