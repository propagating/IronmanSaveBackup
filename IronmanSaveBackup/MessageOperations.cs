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
        public static bool ConfirmChoice(MessageChoiceEnum type)
        {
            var choice = false;
            var message = "";
            var caption = "";
            switch (type)
            {
                //Delete backups
                case MessageChoiceEnum.DeleteChoice:
                    message =
                        @"Choosing 'Yes' will DELETE ALL BACKUPS in your backups folder. This is NOT reversible.";
                    caption = "Delete All Backups?";
                    break;
                    case MessageChoiceEnum.ReplaceChoice:
                        message =
                            @"Choosing 'Yes' will replace the existing Ironman Save for this campaign. Please ensure you have a backup of your current save prior to recovery.";
                        caption = "Restore Backup?";
                        break;
                    case MessageChoiceEnum.InvalidChoice:
                        message = "The choice you selected is invalid.";
                        caption = "Operation Cancelled";
                        break;
                default:
                    message = "Are you sure you want to close this box?";
                    break;
                        
            }

            var buttons = MessageBoxButtons.YesNoCancel;
            var result = MessageBox.Show(message, caption, buttons);

            if (result == DialogResult.Yes)
            {
                choice=true;
            }
            return choice;
        }

        public static void UserMessage(string message, MessageTypeEnum type = MessageTypeEnum.GenericError)
        {
            var caption = "";
            const MessageBoxButtons buttons = MessageBoxButtons.OK;
            switch (type)
            {
                //Delete backups
                case MessageTypeEnum.DoesNotExistError:
                    caption = "No File/Folder Selected";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.InvalidPathError:
                    caption = "Invalid Path Error";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.FileInUseError:
                    caption = "File In Use";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.BackupError:
                    caption = "Backup Error";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.RestoreError:
                    caption = "Backup Error";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.GenericError:
                    caption = "An Error Occurred";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.BackupSuccess:
                    caption = "Backup Operation Successful";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information);
                    break;
                case MessageTypeEnum.RestoreSuccess:
                    caption = "Restoration Successful";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information);
                    break;
                default:
                    caption = "Something Weird Happened";
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;

            }
          
        }

        public enum MessageTypeEnum
        {
            GenericError,
            DoesNotExistError,
            InvalidPathError,
            FileInUseError,
            BackupError,
            BackupSuccess,
            RestoreSuccess,
            RestoreError
        }

        public enum MessageChoiceEnum
        {
            DeleteChoice,
            ReplaceChoice,
            InvalidChoice
        }
    }

}
