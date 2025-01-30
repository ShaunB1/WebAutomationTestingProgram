param (
    [string]$username,
    [string]$password,
    [string]$hostname,
    [string]$port,
    [string]$sid,
    [string]$scriptPath
)

$sqlplusArguments = "${username}/${password}@${hostname}:${port}/${sid}"

$sqlCommands = @"
@'$scriptPath'
EXIT
"@

$processInfo = New-Object System.Diagnostics.ProcessStartInfo
$processInfo.FileName = "sqlplus"
$processInfo.Arguments = $sqlplusArguments
$processInfo.RedirectStandardInput = $true
$processInfo.RedirectStandardOutput = $true
$processInfo.RedirectStandardError = $true
$processInfo.UseShellExecute = $false
$processInfo.CreateNoWindow = $true

$process = [System.Diagnostics.Process]::Start($processInfo)

$process.StandardInput.WriteLine($sqlCommands)
$process.StandardInput.Close()

$output = $process.StandardOutput.ReadToEnd()
$errorOutput = $process.StandardError.ReadToEnd()

$process.WaitForExit()

Write-Output "SQL*Plus Output:"
Write-Output $output

if ($process.ExitCode -ne 0) {
    Write-Error "sqlplus exited with code $($process.ExitCode)"
    if ($errorOutput) {
        Write-Error "Error Output: $errorOutput"
    }
}
