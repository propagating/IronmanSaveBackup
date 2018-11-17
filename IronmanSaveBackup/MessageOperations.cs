using System.Windows.Forms;
using IronmanSaveBackup.Enums;
using IronmanSaveBackup.Properties;

namespace IronmanSaveBackup
{
    internal class MessageOperations
    {
        public static bool ConfirmChoice(MessageChoiceEnum type)
        {
            string message;
            var caption = "";
            switch (type)
            {
                case MessageChoiceEnum.DeleteChoice:
                    message     = Resources.DeleteAllBackupWarning;
                    caption     = Resources.DeleteAllBackupCaption;
                    break;

                case MessageChoiceEnum.ReplaceChoice:
                        message = Resources.ReplaceExistingWarning;
                        caption = Resources.ReplaceExistingCaption;
                        break;

                case MessageChoiceEnum.InvalidChoice:
                        message = Resources.InvalidChoiceWarning;
                        caption = Resources.InvalidChoiceCaption;
                        break;

                default:
                        message = Resources.CloseBoxWarning;
                        break;
            }

            const MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            var result = MessageBox.Show(message, caption, buttons);
            return (result == DialogResult.Yes);
        }

        public static void UserMessage(string message, MessageTypeEnum type = MessageTypeEnum.GenericError)
        {
            string caption;
            const MessageBoxButtons buttons = MessageBoxButtons.OK;
            switch (type)
            {
                //Delete backups
                case MessageTypeEnum.DoesNotExistError:
                    caption = Resources.FolderNotFoundCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.InvalidPathError:
                    caption = Resources.InvalidPathCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.FileInUseError:
                    caption = Resources.InUseCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.BackupError:
                    caption = Resources.BackupErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.RestoreError:
                    caption = Resources.RestoreErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.GenericError:
                    caption = Resources.GeneralErrorCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;
                case MessageTypeEnum.BackupSuccess:
                    caption = Resources.BackupSuccessCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information);
                    break;
                case MessageTypeEnum.RestoreSuccess:
                    caption = Resources.RestoreSuccessCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Information);
                    break;
                default:
                    caption = Resources.WeridCaption;
                    MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                    break;

            }
          
        }

    }

}
