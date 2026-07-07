# Vite 8+ fails when project path contains '#'. Use SUBST to mount a drive without special chars.
$projectRoot = Split-Path -Parent $PSScriptRoot
$drive = "W:"

if (Test-Path $drive) {
  subst $drive /d | Out-Null
}

subst $drive $projectRoot
Set-Location $drive

Write-Host "Dev server via $drive -> $projectRoot"
Write-Host "Press Ctrl+C to stop. Run 'subst $drive /d' if drive stays mounted."

try {
  pnpm exec vite
}
finally {
  if (Test-Path $drive) {
    subst $drive /d | Out-Null
  }
}
