using System.Windows.Forms;
using IronmanSaveBackup.Enums;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    internal class MessageOperations
    {
        public static bool ConfirmChoice(MessageChoice type)
        {
            string message;
            var caption = "";
            MessageBoxIcon icon;
            const MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            const MessageBoxOptions options = MessageBoxOptions.ServiceNotification;
            const MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1;
            switch (type)
            {
                case MessageChoice.DeleteChoice:
                    message     = Resources.DeleteAllBackupWarning;
                    caption     = Resources.DeleteAllBackupCaption;
                    icon        = MessageBoxIcon.Exclamation;
                    break;

                case MessageChoice.ReplaceChoice:
                    message = Resources.ReplaceExistingWarning;
                    caption = Resources.ReplaceExistingCaption;
                    icon = MessageBoxIcon.Exclamation;
                    break;

                case MessageChoice.InvalidChoice:
                    message = Resources.InvalidChoiceWarning;
                    caption = Resources.InvalidChoiceCaption;
                    icon = MessageBoxIcon.Warning;
                    break;

                default:
                    message = Resources.CloseBoxWarning;
                    icon = MessageBoxIcon.Warning;
                    break;
            }

            var result = MessageBox.Show(message, caption, buttons, icon, defaultButton, options);
            return (result == DialogResult.Yes);
        }

        public static void UserMessage(string message, MessageType type = MessageType.GenericError)
        {
            string caption;
            const MessageBoxButtons buttons = MessageBoxButtons.OK;
            const MessageBoxOptions options = MessageBoxOptions.ServiceNotification;
            switch (type)
            {
                //Delete backups
                case MessageType.DoesNotExistError:
                    caption = Resources.FolderNotFoundCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.InvalidPathError:
                    caption = Resources.InvalidPathCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.FileInUseError:
                    caption = Resources.InUseCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.BackupError:
                    caption = Resources.BackupErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.RestoreError:
                    caption = Resources.RestoreErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.GenericError:
                    caption = Resources.GeneralErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.BackupSuccess:
                    caption = Resources.BackupSuccessCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, options);
                    break;
                case MessageType.RestoreSuccess:
                    caption = Resources.RestoreSuccessCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, options);
                    break;
                default:
                    caption = Resources.WeridCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,options);
                    break;

            }
          
        }

    }

}
