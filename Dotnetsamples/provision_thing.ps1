Import-Module AWSPowerShell

$ScriptPath = $MyInvocation.MyCommand.Path
$ScriptDirectory = Split-Path $ScriptPath

$CertificatePEMLocation = "$ScriptDirectory\certificates\certificate.cert.pem"
$CACertificateLocation = "$ScriptDirectory\certificates\AmazonRootCA1.crt"

if (!(Test-Path "$ScriptDirectory\certificates")) {
    New-Item -Path $ScriptDirectory -Name "certificates" -ItemType "directory"
}

if (Test-Path "$CACertificateLocation" -PathType Leaf) {
    Write-Output "Root CA certificate already exists.  Skipping download."
}
else {
    Write-Output "Downloading Amazon Root CA"
    Invoke-WebRequest -Uri "https://www.amazontrust.com/repository/AmazonRootCA1.pem" -OutFile $CACertificateLocation
}

if (Test-Path $CertificatePEMLocation) {
    Write-Output "Certificates already exist.  Skipping creation."
    Get-ChildItem -Path $ScriptDirectory\certificates -Filter *.name -Recurse -File | ForEach-Object {
        $CertificateId = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
    }
}
else {
    Write-Output "Creating certificate.."
    $KeysAndCertificate = New-IOTKeysAndCertificate -SetAsActive $TRUE
    $KeysAndCertificate.CertificatePem | Out-File $CertificatePEMLocation -Encoding ascii
    $KeysAndCertificate.KeyPair.PublicKey | Out-File "$ScriptDirectory\certificates\certificate.public.key" -Encoding ascii
    $KeysAndCertificate.KeyPair.PrivateKey | Out-File "$ScriptDirectory\certificates\certificate.private.key" -Encoding ascii
    Write-Output $KeysAndCertificate.KeyPair.PrivateKey

    New-Item "$ScriptDirectory\certificates\$($KeysAndCertificate.CertificateId).name" -type file
    $CertificateId = $KeysAndCertificate.CertificateId

    openssl pkcs12 -export -in "$CertificatePEMLocation" -inkey "$ScriptDirectory\certificates\certificate.private.key" -out "$ScriptDirectory\certificates\certificate.cert.pfx" -certfile "$CACertificateLocation" -password pass:MyPassword1
}

Write-Output "Certificate ID: $CertificateId"
Write-Output "Creating thing: aws-iot-dotnet-publisher-consumer-framework.."
$ProvisioningTemplate = Get-Content -Path "$ScriptDirectory\provisioning_template.json" -Raw
Register-IOTThing `
    -TemplateBody $ProvisioningTemplate `
    -Parameter @{ "ThingName"="aws-iot-dotnet-publisher-consumer-framework"; "CertificateId"="$CertificateId" } | Out-Null

$EndpointName = Get-IOTEndpoint

Write-Output "Replacing placeholder with endpoint $EndpointName.."
Get-ChildItem -Path $ScriptDirectory -Filter *.cs -Recurse -File | ForEach-Object {
    if( (Select-String -Path $_.FullName -Pattern "<<your-iot-endpoint>>") -ne $null) {
        Write-Output "Replacing placeholder in $($_.FullName)"
        (Get-Content $_.FullName).Replace("<<your-iot-endpoint>>", $EndpointName) | Set-Content $_.FullName
    }
}
