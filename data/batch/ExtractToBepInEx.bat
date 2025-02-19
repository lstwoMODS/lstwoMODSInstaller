@echo off
setlocal

set "ZIP_FILE=%~1"
set "DEST_FOLDER=%~2\BepInEx"

powershell -Command "Expand-Archive -Path \"%ZIP_FILE%\" -DestinationPath \"%DEST_FOLDER%\" -Force"

del "%ZIP_FILE%"
