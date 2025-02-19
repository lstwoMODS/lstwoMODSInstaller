@echo off
setlocal

set "ZIP_FILE=%~1"
set "DEST_FOLDER=%~2\BepInEx"

if not exist "%DEST_FOLDER%" mkdir "%DEST_FOLDER%"

powershell -Command "Expand-Archive -Path \"%ZIP_FILE%\" -DestinationPath \"%DEST_FOLDER%\" -Force"

del "%ZIP_FILE%"

exit /b
