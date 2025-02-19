@echo off
setlocal

set "SOURCE_FILE=%~1"
set "DEST_FOLDER=%~2\BepInEx\plugins"

if not exist "%DEST_FOLDER%" mkdir "%DEST_FOLDER%"

move /Y "%SOURCE_FILE%" "%DEST_FOLDER%"
