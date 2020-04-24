# IronmanSaveBackup

## Chimera Squad Note
Chimera Squad saves your game much more frequently than XCOM 2 and WotC do. Because of this, I highly recommend limiting the max backups to a relatively managable number (20 should be more than enough). 

Saves in HQ are much smaller than saves made in the field, so you can use that as a better gauge for when to restore a save or not. They also appear to get bigger the longer the mission goes on, but maybe that's just me musing. 

Saves are triggered at different times making it a bit more resilient to the crashes we saw in XCOM 2/WotC but still, same engine means there's still a chacne.

## XEW/XEU/Vanilla XCOM2 Note
You might have a couple of backups from your non-ironman Campaigns for these 3 versions.

This is because WotC is the only version that makes it easy to identify Ironman from non-Ironman saves without looking at the save data directly. As a result, running this tool along side XEU/XEW/X2 Vanilla will not distinguish if a save is ironman or not, and will continue to make backups for the campaign you are currently playing. WotC will only backup ironman saves.


## Dependencies
1. .NET Framework 4.8 - https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-web-installer

## Instructions

### Configuration
1. Set the Save Location to the directory that stores your XEW/XEU/XCOM2/WotC Ironman Saves
2. Set the Backup Location to a directory of your choosing
3. Set Backups to Keep to to an appropriate value. A value of 0 keeps all backups. Oldest backups are deleted first if the limit is reached. This limits the backups to keep per campaign, not a combined total of all backups.
4. Choose one of the following methods:


#### Event Driven Saves (Recommended Version)
1. Click 'Enable Event Drive Saves' Checkbox
2. Click Start Backup
3. Play the game! 

Note: A backup will be automatically created each time XCOM2/WotC updates the save.

#### Manual Saves
1. Click Force Backup
2. Play the game!

#### Interval Saves
1. Set the Interval
2. Click Start Backup
3. Play the game!

Note: Backups will be created on each interval, so if you have a short interval, you may have duplicates

### Restoring Backups
1. Select the appropriate backup file
2. Click Restore backup
3. Play the game!

Note: To restore a deleted save, you may need to restart XCOM2/WotC before it shows up

## Compatibility
Currently, this is Windows only, supproting everything from Windows 7SP1 forward. OSX Sierra and Linux support are on the radar, but no definitive date as of yet.

This will work with XEU, XEW, WOTC, XCOM2, and Chimera Squad. It should work with any and all mods. This tool could be extended to work with any game that has Ironman Capability, but that's a future project. 
