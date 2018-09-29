# IronmanSaveBackup

## Dependencies
1. .NET Framework 4.6.1 - https://www.microsoft.com/en-us/download/details.aspx?id=49981

## Instructions

### Configuration
1. Set the Save Location to the directory that stores your XCOM2/WotC Ironman Saves
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
This will work on any XOCM2/WotC Ironman Save. It will not work on EU/EW, but I'm exploring a way to backup those as well. It works with any and all mods. This tool could be extneded to work with any game that has Ironman Capability, but that's a future project.

## History
This project is designed to be a tool for creating automatic backups, and restoring those backups, of Ironman save games.

It's initial purpose is the protection of XCOM 2 save games (working with any version of the game), as Ironman tends to cause unrecoverable crashes and heartache for many players.

The tool is built to work with any game that has ironman saves, though that functionality will not be fully tested and integrated for the time being. If the game uses a similar naming scheme to XCOM 2 (save_IRONMAN) then there should be no problems using this tool for that game as well.

This tool is NOT intended to be used for save scumming, and you may run into issues trying to use it as such. This was not really done intentionally, but here are two reasons not to: 1) it's against the spirit of ironman and 2) It's much more difficult to restore a backup from a bad action through this tool than it is just to play with ironman disabled.

EDIT: Initially I was concerned about the possibility of making save corruption worse with this tool through repeated backups. This has been shown to be a non-issue through testing against multiple campaigns across XCOM2 and XCOM2: WotC. 
