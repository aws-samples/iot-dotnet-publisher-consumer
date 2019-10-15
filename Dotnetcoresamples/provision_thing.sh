CERTIFICATE_PEM_LOCATION="certificates/certificate.cert.pem"
CA_CERTIFICATE_LOCATION="certificates/AmazonRootCA1.crt"

mkdir certificates

if test -f "certificates/AmazonRootCA1.crt"; then
    echo "Root CA certificate already exists.  Skipping download."
else
    echo "Downloading Amazon root CA."
    curl -o "$CA_CERTIFICATE_LOCATION" "https://www.amazontrust.com/repository/AmazonRootCA1.pem"
fi

if test -f "$CERTIFICATE_PEM_LOCATION"; then
    echo "Certificates already exist.  Skipping creation."
    # get the cert id
    CERTIFICATE_ID=$(basename $(find certificates -name '*.name') .name)
else
    echo "Creating certificate.."
    CERTIFICATE_ID=$(aws iot create-keys-and-certificate \
        --certificate-pem-outfile "$CERTIFICATE_PEM_LOCATION" \
        --public-key-outfile "certificates/certificate.public.key" \
        --private-key-outfile "certificates/certificate.private.key" \
        --set-as-active | jq -r .certificateId)

    openssl pkcs12 -export -in "$CERTIFICATE_PEM_LOCATION" \
        -inkey "certificates/certificate.private.key" \
        -out "certificates/certificate.cert.pfx" \
        -certfile "$CA_CERTIFICATE_LOCATION" \
        -password pass:MyPassword1

    touch "certificates/$CERTIFICATE_ID.name"
fi

echo "Creating thing: dotnetthing.."
aws iot register-thing \
    --template-body file://provisioning_template.json \
    --parameters ThingName="dotnetthing",CertificateId="$CERTIFICATE_ID"