using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IronmanSaveBackup
{
    class MessageOperations
    {
        public static bool ConfirmChoice(MessageTypeEnum type)
        {
            var choice = false;
            var message = "";
            var caption = "";
            switch (type)
            {
                //Delete backups
                case MessageTypeEnum.DeleteChoice:
                    message =
                        @"Choosing 'Yes' will DELETE ALL BACKUPS in your backups folder. This is NOT reversible.";
                    caption = "Delete All Backups?";
                    break;
                    case MessageTypeEnum.ReplaceChoice:
                        message =
                            @"Choosing 'Yes' will replace the existing Ironman Save for this campaign. Please ensure you have a backup of your current save prior to recovery.";
                        caption = "Restore Backup?";
                        break;
                default:
                    message = "Are you sure you want to close this box?";
                    break;
                        
            }

            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = MessageBox.Show(message, caption, buttons);

            if (result == DialogResult.Yes)
            {
                choice=true;
            }
            return choice;
        }

        public static void UserMessage(MessageTypeEnum type = MessageTypeEnum.GenericError)
        {
            var message = "";
            var caption = "";
            switch (type)
            {
                //Delete backups
                case MessageTypeEnum.DoesNotExistError:
                    message =
                        "The target file does not exist, the operation could not be completed.";
                    caption = "No File/Folder Selected";
                    break;
                case MessageTypeEnum.InvalidPathError:
                    message =
                        "The filepath selected is invalid, inaccessible, or does not exist.";
                    caption = "No File/Folder Selected";
                    break;
                case MessageTypeEnum.FileInUseError:
                    message =
                        "The Save File is in use by another process (probably XCOM2). The operation cannot be completed.";
                    caption = "File In Use Error";
                    break;
                case MessageTypeEnum.BackupError:
                    message =
                        "The Backup operation could not be completed.";
                    caption = "Backup Error";
                    break;
                case MessageTypeEnum.GenericError:
                    message =
                        "Uncaught exception occurred.";
                    caption = "Something Went Wrong";
                    break;
                case MessageTypeEnum.RestoreSuccess:
                    message =
                        "Backup succesfully restored.";
                    caption = "Backup Restoration Successful";
                    break;
                case MessageTypeEnum.BackupSuccess:
                    message =
                        "Backup succesfully created.";
                    caption = "Backup Creation Succesful";
                    break;
                case MessageTypeEnum.DeleteSuccess:
                    message =
                        "All backups succesfully deleted.";
                    caption = "Backup Deletion Successful";
                    break;
                default:
                    message = "You shouldn't be here. You should close this message box and submit an issue on Github.";
                    caption = "Something Weird Happened";
                    break;

            }

            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, caption, buttons);
        }

        public enum MessageTypeEnum
        {
            GenericError,
            DoesNotExistError,
            InvalidChoiceError,
            InvalidPathError,
            DeleteChoice,
            ReplaceChoice,
            FileInUseError,
            RestoreSuccess,
            BackupError,
            BackupSuccess,
            DeleteSuccess
        }
    }

}
