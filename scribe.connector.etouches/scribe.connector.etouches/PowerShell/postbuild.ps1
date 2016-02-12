# Get the ID and security principal of the current user account
$myWindowsID=[System.Security.Principal.WindowsIdentity]::GetCurrent()
$myWindowsPrincipal=new-object System.Security.Principal.WindowsPrincipal($myWindowsID)
 
# Get the security principal for the Administrator role
$adminRole=[System.Security.Principal.WindowsBuiltInRole]::Administrator
 
# Check to see if we are currently running "as Administrator"
if ($myWindowsPrincipal.IsInRole($adminRole))
   {
   # We are running "as Administrator" - so change the title and background color to indicate this
   $Host.UI.RawUI.WindowTitle = $myInvocation.MyCommand.Definition + "(Elevated)"
   $Host.UI.RawUI.BackgroundColor = "DarkBlue"
   clear-host
   }
else
   {
   # We are not running "as Administrator" - so relaunch as administrator
   
   # Create a new process object that starts PowerShell
   $newProcess = new-object System.Diagnostics.ProcessStartInfo "PowerShell";
   
   # Specify the current script path and name as a parameter
   $newProcess.Arguments = $myInvocation.MyCommand.Definition;
   
   # Indicate that the process should be elevated
   $newProcess.Verb = "runas";
   
   # Start the new process
   [System.Diagnostics.Process]::Start($newProcess);
   
   # Exit from the current, unelevated, process
   exit
   }
Write-Host "Stopping Service";
stop-service -displayname "Scribe Online Agent"
Start-Sleep -s 5
Write-Host "Replacing Files";
Remove-Item 'C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.eTouches\*' -Recurse
Copy-Item 'c:\_git\etouches\scribe.connector.etouches\scribe.connector.etouches\bin\Debug\*' -Destination 'C:\Program Files (x86)\Scribe Software\Scribe Online Agent\Connectors\Scribe.Connector.eTouches' -Recurse
Start-Sleep -s 5
Write-Host "Starting Service";
start-service -displayname "Scribe Online Agent"
