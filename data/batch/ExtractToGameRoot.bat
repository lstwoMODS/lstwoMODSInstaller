@echo off
setlocal

set "ZIP_FILE=%~1"
set "GAME_FOLDER=%~2"

powershell -NoProfile -Command ^
    "Expand-Archive -LiteralPath '%ZIP_FILE%' -DestinationPath '%GAME_FOLDER%' -Force"

del "%ZIP_FILE%"

exit /b
