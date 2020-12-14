


# make sure you adjust this to point to the folder you want to monitor
$PathToMonitor = "C:\Pending_Decryption"

# explorer $PathToMonitor

$FileSystemWatcher = New-Object System.IO.FileSystemWatcher
$FileSystemWatcher.Path  = $PathToMonitor
$FileSystemWatcher.IncludeSubdirectories = $true

# make sure the watcher emits events
$FileSystemWatcher.EnableRaisingEvents = $true



# define the code that should execute when a file change is detected
$Action = 
{
    $details = $event.SourceEventArgs
    $Name = $details.Name
    $FullPath = $details.FullPath
    $OldFullPath = $details.OldFullPath
    $OldName = $details.OldName
    $ChangeType = $details.ChangeType
    $Timestamp = $event.TimeGenerated

    $BaseName=$Name.Substring(0,$Name.Length-4)

    $inputpath = "C:\Pending_Decryption\" +$Name
    $outputpath = "C:\Decrypted\" + $BaseName

    $text = "{0} was {1} at {2}" -f $FullPath, $ChangeType, $Timestamp
    Write-Host ""
    Write-Host $text -ForegroundColor Green


     # you can also execute code based on change type here
    switch ($ChangeType)
    {
        'Changed' 
        {
            gpg --pinentry-mode loopback --yes --output $outputpath --decrypt $FullPath
            
            $text3 = "INPUT: {0}" -f $FullPath
            $text4 = "OUTPUT: {0}" -f $outputpath

            Write-Host $text3
            Write-Host $text4

            Start-Sleep -s 15


            Remove-Item -Path $Fullpath
            Write-Host $Name "has been removed!"
    
        }

        'Created' 
        { 
       
         #  gpg --pinentry-mode loopback --yes --output $outputpath --decrypt $FullPath
       #   gpg --output "C:\Decrypted\Approved_Interim Report.pdf" --decrypt "C:\Pending_Decryption\Approved_Interim Report.pdf.gpg" 
        
           # gpg --pinentry-mode=loopback --decrypt $FullPath --output $outputpath
            $text = "File {0} was decrypted with GPG" -f $Name
            Write-Host $text -ForegroundColor Yellow
            $text1 = "File {0} has been sent to Decrypted folder. Renamed to {1}" -f $Name, $BaseName
            Write-Host $text1
          #  Remove-Item -Path $Fullpath
           # Write-Host $Name "has been removed!" 

           

            

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
        'Deleted' { "DELETED"}
            # uncomment the below to mimick a time intensive handler
            <#
            Write-Host "Deletion Handler Start" -ForegroundColor Gray
            Start-Sleep -Seconds 4    
            Write-Host "Deletion Handler End" -ForegroundColor Gray
            #>
  
        'Renamed' 
        { 
            # this executes only when a file was renamed
            $text = "File {0} was renamed to {1}" -f $OldName, $Name
            Write-Host $text -ForegroundColor Yellow
            

        }

        default { Write-Host $_ -ForegroundColor Red -BackgroundColor White }
    }
}



# add event handlers
$handlers = . {
    Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Changed -Action $Action -SourceIdentifier FSChange
   Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Created -Action $Action -SourceIdentifier FSCreate
  #  Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Deleted -Action $Action -SourceIdentifier FSDelete
    Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Renamed -Action $Action -SourceIdentifier FSRename
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
    #Unregister-Event -SourceIdentifier FSDelete
    Unregister-Event -SourceIdentifier FSRename
    # remove background jobs
    $handlers | Remove-Job
    # remove filesystemwatcher
    $FileSystemWatcher.EnableRaisingEvents = $false
    $FileSystemWatcher.Dispose()
    "Event Handler disabled."
}


