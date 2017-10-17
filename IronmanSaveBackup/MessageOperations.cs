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
                        @"Choosing 'Yes' will DELETE ALL BACKUPS in your selected backups folder.";
                    caption = "Delete All Backups?";
                    break;
                    case MessageTypeEnum.ReplaceChoice:
                        message =
                            @"Choosing 'Yes' will replace the existing Ironman Save for this campaign. It cannot be recovered.";
                        caption = "Restore Backup?";
                        break;
                default:
                    message = "Are you sure you want to close this box?";
                    break;
                        
            }

            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);

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
                        "The item you selected does not exist and/or there is no value set.";
                    caption = "No File/Folder Selected";
                    break;
                case MessageTypeEnum.InvalidPathError:
                    message =
                        "The item you selected does not exist and/or there is no value set.";
                    caption = "No File/Folder Selected";
                    break;
                case MessageTypeEnum.GenericError:
                    message =
                        "Uncaught exception occurred.";
                    caption = "Something Went Wrong";
                    break;
                default:
                    message = "Are you sure you want to close this message box?";
                    caption = "Something Happened";
                    break;

            }

            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, caption, buttons);
        }

        public enum MessageTypeEnum
        {
            GenericError = 0,
            DoesNotExistError = 1,
            InvalidChoiceError = 2,
            InvalidPathError = 3,
            DeleteChoice = 10,
            ReplaceChoice = 11,

        }
    }

}
