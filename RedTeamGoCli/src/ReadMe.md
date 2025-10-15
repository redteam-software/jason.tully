# RedTeamGo Cli

A set of tools making Go development easier.


## Commands
### auto-pr
Using the current Go PHP v4 or v5 projects directory,  creates a pull request, auto merges it and displays the progress of the associated git hub action.
The PR title and description are automatically pulled from the latest commit.
The default target branch is `uatcode_v3`.  This can be overwritten by passing the --target option.


###
### cf-sync
Using the current project directory, starts a local file system monitor to synchronize changes between the local Cold Fusion Go project and the remotely deployed UAT instance.
For this command to function correctly, you must be connected through WireGuard.

User and password can be supplied either directly:
`--user` and `--password`

or through a configuration file with the appropriate credentials.

`--config C:\\mycredentials.txt`

```
user=myuser
password=mypassword
```
You can also place a configuration file  named "rtgo.txt" in $USER_PROFILE/.secrets and it will be automatically picked up._

Optionally you can set the the batch size and debounce timeout to help throttle many change events and the ftp host/remote directory

`--batch` - default is 5
`--debounce` - default is  10 seconds
`--host`
`--remote-dir`



## Examples

### Auto PR for Go v4
```
cd "D:\Development\RedTeam\Go\go-production-v4"

rtgo auto-pr
```

### Run Go Cold Fusion File Synch

Stores user and password in a local configuration file
```
cd "D:\Development\RedTeam\Go\all-go-projects\go-production-cf"

rtgo cf-sync --config \"C:\\Users\\Jason Tully\\.secrets\\ColdFusionFtpServerUAT.txt\""
```

### Run Go Cold Fusion File Synch (rtgo.txt)

Reads "rtgo.txt" in $USER_PROFILE/.secrets.

```
cd "D:\Development\RedTeam\Go\all-go-projects\go-production-cf"

rtgo cf-sync 
```

### Run Go Cold Fusion File Synch

Supplies user and password directly 
```
cd "D:\Development\RedTeam\Go\all-go-projects\go-production-cf"

rtgo cf-sync --user test --password test
```

### Sample Configuration File
```
user=test
password=test
```