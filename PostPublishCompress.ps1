$workingDir = $PWD.Path
$publishDir = "$workingDir\bin\Release\net8.0-windows\win-x64\publish"
$exeFile = "$publishDir\StatementMover.exe"
$zipFile = "release\StatementMover.zip"
$exeFileDest = "release\StatementMover.exe"

# Ensure the Release folder exists, if not, create it
$releaseFolder = "release"
if (-not (Test-Path $releaseFolder)) {
    New-Item -ItemType Directory -Path $releaseFolder
}

# If the zip file already exists, remove it
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

# Compress just the .exe file into the zip
Write-Host "Compressing $exeFile into $zipFile..."
Add-Type -AssemblyName "System.IO.Compression.FileSystem"
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipFile, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Write-Host "Compression complete! The file is located at $zipFile" -ForegroundColor Green

if (Test-Path $exeFileDest) {
    Remove-Item $exeFileDest -Force
}

# Move the .exe to the release folder
Write-Host "Moving $exeFile to release/..."
Move-Item -Path $exeFile -Destination "release"
Write-Host "File has been moved to release/" -ForegroundColor Green
