net stop "Scribe Online Agent"
xcopy E:\_git\etouches\scribe.connector.etouches\scribe.connector.etouches\bin\Debug\*.* "C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.etouches\" /Y 
net start "Scribe Online Agent"
PAUSE