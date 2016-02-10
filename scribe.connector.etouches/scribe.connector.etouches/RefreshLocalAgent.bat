net use \\psf\Home /user:shaneedmonds
net stop "Scribe Online Agent"
xcopy bin\Debug\*.* "C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.etouches\" /Y 
net start "Scribe Online Agent"
PAUSE