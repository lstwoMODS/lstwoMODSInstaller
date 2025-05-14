@echo off
setlocal

set "SOURCE_FILE=%~1"
set "DEST_FOLDER=%~2"
set "PLUGINS_FOLDER=%DEST_FOLDER%\BepInEx\plugins"
set "GGM_TARGET_FOLDER=%DEST_FOLDER%\Wobbly Life_Data"

if not exist "%PLUGINS_FOLDER%" mkdir "%PLUGINS_FOLDER%"
if not exist "%GGM_TARGET_FOLDER%" mkdir "%GGM_TARGET_FOLDER%"

for %%F in ("%SOURCE_FILE%") do set "FILE_NAME=%%~nxF"

if /I "%FILE_NAME%"=="globalgamemanagers" (
    copy /Y "%SOURCE_FILE%" "%GGM_TARGET_FOLDER%\"
) else (
    copy /Y "%SOURCE_FILE%" "%PLUGINS_FOLDER%\"
)

del "%SOURCE_FILE%"

exit /b
