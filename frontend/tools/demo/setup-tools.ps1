$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$directory = Join-Path $repo 'artifacts/demo-tools'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$archive = Join-Path $directory 'ffmpeg.zip'
$checksum = Join-Path $directory 'ffmpeg.sha256'
# Windows build provider linked by ffmpeg.org; pin the release used for these demos.
$url = 'https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-9.0.1-essentials_build.zip'
Invoke-WebRequest $url -OutFile $archive -TimeoutSec 180
Invoke-WebRequest ($url + '.sha256') -OutFile $checksum -TimeoutSec 30
$expected = ((Get-Content -LiteralPath $checksum -Raw).Trim() -split '\s+')[0]
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected) { throw 'FFmpeg SHA256 verification failed' }
Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $directory 'ffmpeg') -Force
Push-Location (Join-Path $repo 'frontend')
try {
    & node node_modules/playwright/cli.js install chromium
    if ($LASTEXITCODE -ne 0) { throw 'Playwright browser installation failed' }
} finally { Pop-Location }
