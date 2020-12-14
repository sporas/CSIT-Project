# make sure you adjust this to point to the folder you want to monitor
$PathToMonitor = "C:\Encrypted"

# explorer $PathToMonitor

$FileSystemWatcher = New-Object System.IO.FileSystemWatcher
$FileSystemWatcher.Path  = $PathToMonitor
$FileSystemWatcher.IncludeSubdirectories = $true

# make sure the watcher emits events
$FileSystemWatcher.EnableRaisingEvents = $true

$Action = 
{
    $details = $event.SourceEventArgs
    $Name = $details.Name
    $FullPath = $details.FullPath
    $OldFullPath = $details.OldFullPath
    $OldName = $details.OldName
    $ChangeType = $details.ChangeType
    $Timestamp = $event.TimeGenerated

    function Publish-Vault
    {
        param(
            $FilePath
        )

        #Remove encryption extension from filename
     #   $NameTest = [System.IO.Path]::GetFileNameWithoutExtension($FilePath).ToLower();

        #Remove encryption extension from original filepath (to be used later)
        $Placeholder = $FilePath.Substring(0,$FilePath.Length-4) 
        Write-Host "Placeholder: $Placeholder" 

        #obtain filename without any extensions but with classification still intact (uses placeholder varaible mentioned above)
        $FileName = [System.IO.Path]::GetFileNameWithoutExtension($Placeholder).ToLower();
     #   Write-Host "NameTest: $NameTest" -ForegroundColor Yellow
        Write-Host "FileName: $FileName" -ForegroundColor Yellow


        Write-Host "" #To create a new line

        if ($FileName -match 'Restricted_')
        {
            $Classification = 'Restricted'
            Write-Host "Classification: $Classification" -ForegroundColor Green
            #Remove classification from file name
            $FileName = $FileName.Substring(11) -replace " ", "_"
        }

        if ($FileName -match 'Confidential_')
        {
            $Classification = 'Confidential'
            Write-Host "Classification: $Classification" -ForegroundColor Yellow
            #Remove classification from file name
            $FileName = $FileName.Substring(13) -replace " ", "_"

        }

        if ($FileName -match 'Secret_')
        {
            $Classification = 'Secret'
            Write-Host  "Classification: $Classification" -ForegroundColor Red
            #Remove classification from file name
            $FileName = $FileName.Substring(7)  -replace " ", "_"
        }


  
        Write-Host "File Name to be uploaded: $FileName" -ForegroundColor Green

        #Extract encryption type ie GPG
        $EncryptionType = [System.IO.Path]::GetExtension($FilePath);
        Write-Host "Encryption format: $EncryptionType" -ForegroundColor Cyan
 
        #Extract original file type ie txt
        $FileType = [System.IO.Path]::GetExtension($Placeholder).ToLower();
        Write-Host "Original file extension: $FileType" -ForegroundColor cyan


        # returns the base64 string
        Write-Host "Does it even reach here"
        Write-Host $FilePath


        try
        {

             [byte[]] $Filelol = Get-Content $FilePath -Encoding byte -raw


        }
        catch
        {
            Write-Host "Encoding fail ughh" -ForegroundColor Magenta
        }
        try
        {
             $Base64String = [Convert]::ToBase64String($Filelol)
             Write-Host "Successfully converted to base64" -ForegroundColor Yellow
             Write-Host $Base64String
        }
        catch
        {
            Write-Host "Base64 conversion fails" -ForegroundColor Yellow
            Write-Host "The string if there is any: $Base64String"
 
        }

        $sentdata = @{ "data" = @{"Classification"= $Classification; "Base64_data"= $Base64String ; "Encryption_type"=$EncryptionType; "File_type"=$FileType} }

        $UpdateProperties = ConvertTo-Json -InputObject $sentdata
        Write-Host "json properties successfully converted"
        #Set header and URL for API request
        $URL = 'http://127.0.0.1:8200/v1/secret/data/sharepoint/' + $FileName
        $headers =@{"X-Vault-Token"="s.kMYgQ904sXgTuv9KM0RP8u5N" }

        #API request
        Invoke-RestMethod -Method POST -Uri $URL -Header $headers -body $UpdateProperties 
       $res= Invoke-RestMethod -METHOD GET -Uri $URL -Header $headers
      Write-Host $res


    } #End of function

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

    }#End of function
 
        


     switch ($ChangeType)
     {
         'Created' 
         {
            Write-Host "Created Action Initiated: $FullPath"
   
            #API REQUEST
            Publish-Vault -FilePath $FullPath
            

             #Balloon Tip Notification
            $text = "File {0} was sent to HashiCorp Vault" -f $Name
            $text1 = "File {0} is stored in kv engine for safekeeping." -f $Name
            BalloonTipping -Text $text1 -Title $text

         

            #Removing item from encrypted folder after it has been sent to Vault
            Remove-Item -Path $Fullpath
            Write-Host $Name "has been removed!"
            Start-Sleep -s 2
         }

        
         default { Write-Host $_ -ForegroundColor Red -BackgroundColor White }

     }


}


# add event handlers
$handlers = . {

 #  Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Changed -Action $Action -SourceIdentifier FSChange 
   Register-ObjectEvent -InputObject $FileSystemWatcher -EventName Created -Action $Action -SourceIdentifier FSCreate

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
 
    # remove background jobs
    $handlers | Remove-Job
    # remove filesystemwatcher
    $FileSystemWatcher.EnableRaisingEvents = $false
    $FileSystemWatcher.Dispose()
    "Event Handler disabled."
}
