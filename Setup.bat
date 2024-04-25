@ECHO OFF

MKDIR "%LOCALAPPDATA%\TotkRegistryToolkit"
curl --silent -o "%LOCALAPPDATA%\TotkRegistryToolkit\tkrt.exe" -L "https://github.com/ArchLeaders/TotkRegistryToolkit/releases/latest/download/win-x64.exe"
"%LOCALAPPDATA%\TotkRegistryToolkit\tkrt.exe" init

ECHO Setup Complete
PAUSE