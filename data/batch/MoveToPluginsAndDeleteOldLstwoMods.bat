@echo off
setlocal

set "SOURCE_FILE=%~1"
set "DEST_FOLDER=%~2\BepInEx\plugins"

if not exist "%DEST_FOLDER%" mkdir "%DEST_FOLDER%"

if exist "%DEST_FOLDER%\lstwoMODS.dll" del "%DEST_FOLDER%\lstwoMODS.dll"

if exist "%DEST_FOLDER%\NotAzzamods.dll" del "%DEST_FOLDER%\NotAzzamods.dll"

copy /Y "%SOURCE_FILE%" "%DEST_FOLDER%"

del "%SOURCE_FILE%"

exit /b
