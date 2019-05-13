A Windows Service that monitors other Windows Services if they are stopped and starts it back up again. In the App.config file, you can specify the time on how often this service monitors other services, the log file location, when to manually restart services, and the services you want to be monitored.

To install:
1. Open cmd prompt
2. cd C:\ServiceWatcher\bin\Release
3. c:\windows\microsoft.net\framework\v4.0.30319\installutil.exe /i ServiceWatcher.exe

To Uninstall:
1. Open cmd prompt
2. cd C:\ServiceWatcher\bin\Release
3. c:\windows\microsoft.net\framework\v4.0.30319\installutil.exe /u ServiceWatcher.exe
