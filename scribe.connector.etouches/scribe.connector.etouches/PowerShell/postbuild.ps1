stop-service -displayname "Scribe Online Agent"
Remove-Item 'C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.eTouches\*' -Recurse
Copy-Item 'E:\_git\etouches\scribe.connector.etouches\scribe.connector.etouches\bin\Debug\*' -Destination 'C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.eTouches' -Recurse
stop-service -displayname "Scribe Online Agent"
