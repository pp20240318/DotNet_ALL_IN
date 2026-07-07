# Copy admin-app to a path without '#' and start Vite dev server.
$source = Split-Path -Parent $PSScriptRoot
$target = "E:\99Test\watchshop-admin-dev"

Write-Host "Syncing admin-app -> $target"

New-Item -ItemType Directory -Force -Path $target | Out-Null

robocopy $source $target /MIR /XD node_modules dist .git /NFL /NDL /NJH /NJS /nc /ns /np | Out-Null

Set-Location $target

if (!(Test-Path "$target\node_modules")) {
  pnpm install
}

Write-Host "Starting dev server from $target"
pnpm exec vite
