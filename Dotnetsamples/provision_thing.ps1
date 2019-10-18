Import-Module AWSPowerShell

$CertificatePEMLocation = "certificates\certificate.cert.pem"
$CACertificateLocation = "certificates\AmazonRootCA1.crt"

if (!(Test-Path "certificates")) {
    New-Item -Path "." -Name "certificates" -ItemType "directory"
}

if (Test-Path $CACertificateLocation -PathType Leaf) {
    Write-Output "Root CA certificate already exists.  Skipping download."
}
else {
    Write-Output "Downloading Amazon Root CA"
    Invoke-WebRequest -Uri "https://www.amazontrust.com/repository/AmazonRootCA1.pem" -OutFile $CACertificateLocation
}

if (Test-Path $CertificatePEMLocation) {
    Write-Output "Certificates already exist.  Skipping creation."
    Get-ChildItem -Path .\ -Filter *.name -Recurse -File -Name| ForEach-Object {
        $CertificateId = [System.IO.Path]::GetFileNameWithoutExtension($_)
    }
}
else {
    Write-Output "Creating certificate.."
    $KeysAndCertificate = New-IOTKeysAndCertificate -SetAsActive $TRUE
    $KeysAndCertificate.CertificatePem | Out-File $CertificatePEMLocation -Encoding ascii
    $KeysAndCertificate.KeyPair.PublicKey | Out-File "certificates/certificate.public.key" -Encoding ascii
    $KeysAndCertificate.KeyPair.PrivateKey | Out-File "certificates/certificate.private.key" -Encoding ascii
    Write-Output $KeysAndCertificate.KeyPair.PrivateKey

    New-Item "certificates/$($KeysAndCertificate.CertificateId).name" -type file
    $CertificateId = $KeysAndCertificate.CertificateId

    openssl pkcs12 -export -in "$CertificatePEMLocation" -inkey "certificates\certificate.private.key" -out "certificates\certificate.cert.pfx" -certfile "$CACertificateLocation" -password pass:MyPassword1
}

Write-Output $CertificateId
Write-Output "Creating thing: dotnetthing.."
$ProvisioningTemplate = Get-Content -Path "provisioning_template.json" -Raw
Write-Output $ProvisioningTemplate
Register-IOTThing `
    -TemplateBody $ProvisioningTemplate `
    -Parameter @{ "ThingName"="dotnetthing2"; "CertificateId"="$CertificateId" }
