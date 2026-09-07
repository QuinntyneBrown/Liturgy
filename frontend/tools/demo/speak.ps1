param([Parameter(Mandatory)][string]$Manifest, [Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$speaker = New-Object -ComObject SAPI.SpVoice
$voice = @($speaker.GetVoices() | Where-Object { $_.GetDescription() -like '*David*' })[0]
if (-not $voice) { throw 'Microsoft David Desktop is required. Install the Windows English (US) speech voice.' }
$speaker.Voice = $voice
$speaker.Rate = -1
$speaker.Volume = 100
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$segments = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
foreach ($segment in $segments) {
    if ($segment.id -notmatch '^[a-z0-9-]+$') { throw 'Invalid segment ID' }
    $stream = New-Object -ComObject SAPI.SpFileStream
    try {
        $stream.Format.Type = 22 # 22.05 kHz, 16-bit mono PCM
        $stream.Open((Join-Path $OutputDirectory ($segment.id + '.wav')), 3, $false)
        $speaker.AudioOutputStream = $stream
        $speaker.Speak($segment.text) | Out-Null
    } finally { $stream.Close() }
}
