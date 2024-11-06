$workingDir = $PWD.Path
$publishDir = "$workingDir\bin\Release\net8.0-windows\win-x64\publish"
$objDir = "$publishDir\bin"
$exeFile = "$publishDir\StatementMover.exe"
$readmeFile = "$publishDir\README.txt"
$shortcutFile = "$publishDir\StatementMover.exe.lnk"
$zipFile = "release\StatementMover.zip"

if (Test-Path $objDir) {
    Remove-Item -Path $objDir -Recurse -Force
}

if (-not (Test-Path $objDir)) {
    New-Item -ItemType Directory -Path $objDir
}

$nlFile = "$publishDir\nl\PdfiumViewer.resources.dll"
Move-Item $nlFile -Destination "$objDir\$_"
$x64File = "$publishDir\x64\pdfium.dll"
Move-Item $x64File -Destination "$objDir\$_"
if (Test-Path "$publishDir\nl") {
    Remove-Item -Path "$publishDir\nl" -Force
}
if (Test-Path "$publishDir\x64") {
    Remove-Item -Path "$publishDir\x64" -Force
}

Get-ChildItem -Path $publishDir -File | Where-Object { $_.Name -ne 'README.md' } | ForEach-Object {
    Move-Item $_.FullName -Destination "$objDir\$_"
}

if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}
Add-Type -AssemblyName "System.IO.Compression.FileSystem"
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipFile)
