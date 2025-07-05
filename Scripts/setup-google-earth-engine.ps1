# Google Earth Engine Setup Script for Bus Buddy Project
# Run this script in PowerShell as Administrator

Write-Host '🌍 Bus Buddy - Google Earth Engine Setup' -ForegroundColor Green
Write-Host 'Project ID: busbuddy-465000' -ForegroundColor Yellow

# Check if Google Cloud SDK is installed
Write-Host "`n1. Checking Google Cloud SDK..." -ForegroundColor Cyan
if (Get-Command gcloud -ErrorAction SilentlyContinue) {
    Write-Host '✅ Google Cloud SDK is installed' -ForegroundColor Green
} else {
    Write-Host '❌ Google Cloud SDK not found' -ForegroundColor Red
    Write-Host 'Please download and install from: https://cloud.google.com/sdk/docs/install-windows' -ForegroundColor Yellow
    exit 1
}

# Set the project
Write-Host "`n2. Setting up project..." -ForegroundColor Cyan
gcloud config set project busbuddy-465000

# Enable required APIs
Write-Host "`n3. Enabling required APIs..." -ForegroundColor Cyan
Write-Host 'Enabling Earth Engine API...'
gcloud services enable earthengine.googleapis.com
Write-Host 'Enabling Cloud Resource Manager API...'
gcloud services enable cloudresourcemanager.googleapis.com

# Create service account
Write-Host "`n4. Creating service account..." -ForegroundColor Cyan
$serviceAccountExists = gcloud iam service-accounts list --filter="email:bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com" --format="value(email)" 2>$null

if ($serviceAccountExists) {
    Write-Host '✅ Service account already exists' -ForegroundColor Green
} else {
    Write-Host 'Creating new service account...'
    gcloud iam service-accounts create bus-buddy-gee `
        --description="Bus Buddy Google Earth Engine Service Account" `
        --display-name="Bus Buddy GEE"
}

# Create keys directory if it doesn't exist
Write-Host "`n5. Setting up keys directory..." -ForegroundColor Cyan
$keysDir = 'keys'
if (!(Test-Path $keysDir)) {
    New-Item -ItemType Directory -Path $keysDir
    Write-Host '✅ Created keys directory' -ForegroundColor Green
}

# Generate service account key
Write-Host "`n6. Generating service account key..." -ForegroundColor Cyan
$keyPath = 'keys\bus-buddy-gee-key.json'
if (Test-Path $keyPath) {
    Write-Host '⚠️  Key file already exists. Backup and regenerate? (y/n): ' -NoNewline -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq 'y' -or $response -eq 'Y') {
        Move-Item $keyPath "$keyPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        gcloud iam service-accounts keys create $keyPath `
            --iam-account=bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com
    }
} else {
    gcloud iam service-accounts keys create $keyPath `
        --iam-account=bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com
}

# Grant necessary permissions
Write-Host "`n7. Granting permissions..." -ForegroundColor Cyan
Write-Host 'Granting Earth Engine Reader role...'
gcloud projects add-iam-policy-binding busbuddy-465000 `
    --member="serviceAccount:bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com" `
    --role="roles/earthengine.reader"

Write-Host 'Granting Earth Engine Writer role...'
gcloud projects add-iam-policy-binding busbuddy-465000 `
    --member="serviceAccount:bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com" `
    --role="roles/earthengine.writer"

# Test authentication
Write-Host "`n8. Testing authentication..." -ForegroundColor Cyan
try {
    gcloud auth activate-service-account --key-file=$keyPath
    Write-Host '✅ Authentication successful' -ForegroundColor Green
} catch {
    Write-Host '❌ Authentication failed' -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

# Security recommendations
Write-Host "`n🔐 Security Recommendations:" -ForegroundColor Magenta
Write-Host '1. Keep the key file (keys/bus-buddy-gee-key.json) secure'
Write-Host '2. Never commit the key file to source control'
Write-Host '3. Rotate the key periodically'
Write-Host '4. Monitor API usage in Google Cloud Console'

# Next steps
Write-Host "`n🚀 Next Steps:" -ForegroundColor Green
Write-Host "1. Add 'keys/' to your .gitignore file"
Write-Host '2. Test the integration by running the Bus Buddy application'
Write-Host '3. Configure your specific service area coordinates in appsettings.json'
Write-Host '4. Visit the Google Earth Engine Code Editor: https://code.earthengine.google.com/'

Write-Host "`n✅ Google Earth Engine setup complete!" -ForegroundColor Green
Write-Host 'Your project is now configured with:'
Write-Host '  - Project ID: busbuddy-465000'
Write-Host '  - Service Account: bus-buddy-gee@busbuddy-465000.iam.gserviceaccount.com'
Write-Host '  - Configuration: Updated in appsettings.json'

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
