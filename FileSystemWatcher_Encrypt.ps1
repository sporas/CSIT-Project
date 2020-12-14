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
    $outputpath = "\\MURAKAMIPC\Encrypted\" +$Name + ".gpg"


    $text = "{0} was {1} at {2}" -f $FullPath, $ChangeType, $Timestamp
    Write-Host ""
    Write-Host $text -ForegroundColor Green

    


        # you can also execute code based on change type here
    switch ($ChangeType)
    {
        'Created' 
        { 
         #  gpg --yes --cipher-algo AES256 --recipient "test@lab1.local" --output $outputpath --encrypt $FullPath

           $text = "File {0} was encrypted with GPG using AES256" -f $Name
           Write-Host $text -ForegroundColor Yellow
           $text1 = "File {0} has been sent to DecryptionPC for decryption. Proceeding to remove unencrypted file. . ." -f $Name
           Write-Host $text1
           # Remove-Item -Path $FullPath
           Write-Host $Name " has been removed!"

            function BalloonTipping 
            {
 
                [CmdletBinding()]
                param
                (
                [Parameter()]
                $Text,
 
                [Parameter()]
                $Title,
 
                #It must be 'None','Info','Warning','Error'
                $Icon = 'Info'
                )
 
                Add-Type -AssemblyName System.Windows.Forms

                if ($script:balloonToolTip -eq $null)
                {
                $script:balloonToolTip = New-Object System.Windows.Forms.NotifyIcon 
                }
 
                $path = Get-Process -id $pid | Select-Object -ExpandProperty Path
                $balloonToolTip.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($path)
                $balloonToolTip.BalloonTipIcon = $Icon
                $balloonToolTip.BalloonTipText = $Text
                $balloonToolTip.BalloonTipTitle = $Title
                $balloonToolTip.Visible = $true
 

                $balloonToolTip.ShowBalloonTip(30000)
            } 


            BalloonTipping -Text $text1 -Title $text


        }

        'Changed' 
        { 
            gpg --yes --cipher-algo AES256 --recipient "test@lab1.local" --output $outputpath --encrypt $FullPath

            $text3 = "INPUT: {0}" -f $FullPath
            $text4 = "OUTPUT: {0}" -f $outputpath
            Write-Host $text3
            Write-Host $text4

            Remove-Item -Path $FullPath
        }
        'Deleted' { "DELETED"}
        'Renamed' {   }
        default { Write-Host $_ -ForegroundColor Red -BackgroundColor White }
    }

}

# add event handlers
$handlers = . {
    Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Changed -Action $Action -SourceIdentifier FSChange
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
 
    Unregister-Event -SourceIdentifier FSChange
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
