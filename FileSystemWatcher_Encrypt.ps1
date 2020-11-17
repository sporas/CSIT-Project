# make sure you adjust this to point to the folder you want to monitor
$PathToMonitor = "C:\SP_Pending"

$FileSystemWatcher = New-Object System.IO.FileSystemWatcher
$FileSystemWatcher.Path  = $PathToMonitor
$FileSystemWatcher.IncludeSubdirectories = $true

# make sure the watcher emits events
$FileSystemWatcher.EnableRaisingEvents = $true



# define the code that should execute when a file change is detected
$Action = {
    $details = $event.SourceEventArgs
    $Name = $details.Name
    $FullPath = $details.FullPath
    $OldFullPath = $details.OldFullPath
    $OldName = $details.OldName
    $ChangeType = $details.ChangeType
    $Timestamp = $event.TimeGenerated

    $inputpath = "C:\SP_Pending\" + $Name
   # $outputpath = "C:\SP_Encrypted\" +$Name + ".gpg"
    $outputpath = "\\MURAKAMIPC\Pending_Decryption\" +$Name + ".gpg"


    $text = "{0} was {1} at {2}" -f $FullPath, $ChangeType, $Timestamp
    Write-Host ""
    Write-Host $text -ForegroundColor Green

    


        # you can also execute code based on change type here
    switch ($ChangeType)
    {
        'Created' 
        { 
           gpg --cipher-algo AES256 --recipient "test@lab1.local" --output $outputpath --encrypt $FullPath

           $text = "File {0} was encrypted with GPG using AES256" -f $Name
           Write-Host $text -ForegroundColor Yellow
           Write-Host "File" $Name " has been sent to Murakami user for decryption. Proceeding to remove unencrypted file. . ."

           Remove-Item -Path $FullPath
           Write-Host $Name " has been removed!"




        }
        'Changed' { "CHANGE" }
        'Deleted' { "DELETED"}
        'Renamed' {   }
        default { Write-Host $_ -ForegroundColor Red -BackgroundColor White }
    }

}

# add event handlers
$handlers = . {
 #   Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Changed -Action $Action -SourceIdentifier FSChange
    Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Created -Action $Action -SourceIdentifier FSCreate
 #   Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Deleted -Action $Action -SourceIdentifier FSDelete
 #   Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Renamed -Action $Action -SourceIdentifier FSRename
}

Write-Host "Watching for changes to $PathToMonitor"



try
{
    do
    {
        Wait-Event -Timeout 1
        Write-Host "." -NoNewline
        
    } while ($true)
}
finally
{
    # this gets executed when user presses CTRL+C
    # remove the event handlers
 #   Unregister-Event -SourceIdentifier FSChange
    Unregister-Event -SourceIdentifier FSCreate
 #   Unregister-Event -SourceIdentifier FSDelete
 #   Unregister-Event -SourceIdentifier FSRename
    # remove background jobs
    $handlers | Remove-Job
    # remove filesystemwatcher
    $FileSystemWatcher.EnableRaisingEvents = $false
    $FileSystemWatcher.Dispose()
    "Event Handler disabled."
}