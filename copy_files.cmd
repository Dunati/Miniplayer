
@echo on

robocopy ""%~1 "%~2" /MIR /MT /XD  "%~2\uBlock0.chromium" /NDL /NFL /NP /NS /NC /BYTES /NJH /NJS

if %ERRORLEVEL% LEQ 3 exit 0
exit %ERRORLEVEL%