
$condition = "true"

While ($condition -eq 'true')
{


    #User inputs file name for retrieval


    $FileName = Read-Host -Prompt 'Input the file to be retrieved' 
    $FileName = $FileName.ToLower()
    $FileName = $FileName -replace " ", "_"
    Write-Host "Your input for the file is '$FileName'" -ForegroundColor Yellow


    #Set header and URL for API request
    $URL = 'http://127.0.0.1:8200/v1/secret/data/sharepoint/' + $FileName
    $headers =@{"X-Vault-Token"="s.kMYgQ904sXgTuv9KM0RP8u5N" }

    #API request


    try
    {
        $response = Invoke-RestMethod -METHOD GET -Uri $URL -Header $headers


        #Extract relevant information from response
        $base64 = $response.data.data.Base64_data
        #Write-Host $base64 -ForegroundColor Green

        $Classification = $response.data.data.Classification
        Write-Host "Classification: $Classification" -ForegroundColor Yellow

        $EncryptionType = $response.data.data.Encryption_type
        Write-Host "Encryption format: $EncryptionType" -ForegroundColor Cyan

        $FileType = $response.data.data.File_type
        Write-Host "File Extension: $FileType" -ForegroundColor Cyan

        $FilePlace = "C:\Pending_Decryption\" + $Classification + "_" + $FileName + $FileType +$EncryptionType

        Write-Host "File Path: $FilePlace" -ForegroundColor Green


        #Create File
        $ByteArray = [System.Convert]::FromBase64String($base64);
        [System.IO.File]::WriteAllBytes($FilePlace, $ByteArray);
        Start-Sleep -s 3

    }


    catch
    {
        Write-Host "File cannot be found!" -ForegroundColor Yellow
         Start-Sleep -s 3
    }

    $ExitClause = Read-Host -Prompt "Enter [Exit] or [Continue]" 

    if ($ExitClause -eq 'Exit')
    {
        $condition = "false"
    }

    if ($ExitClause -eq 'Continue')
    {
        $condition = "true"
        Write-Host "Restarting..."
        Start-Sleep -s 1
    }

}
