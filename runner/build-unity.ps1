param (
    [string]$OutputPath
 )

$loc = Get-Location
$UnityExec = "C:\Program Files\Unity\Hub\Editor\2019.2.10f1\Editor\Unity.exe"

Write-Output "Start Unity Build $OutputPath\simulator.exe"
& $UnityExec -quit -batchmode -logFile - -stackTraceLogType Full -projectPath $loc\..\simulator -buildWindows64Player $loc\$OutputPath\simulator.exe

Exit 0