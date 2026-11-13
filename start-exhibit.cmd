@echo off
setlocal
set "EXHIBIT_CHROME=%ProgramFiles%\Google\Chrome\Application\chrome.exe"
if not exist "%EXHIBIT_CHROME%" set "EXHIBIT_CHROME=%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe"
if not exist "%EXHIBIT_CHROME%" set "EXHIBIT_CHROME=%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe"
if not exist "%EXHIBIT_CHROME%" (
  echo Google Chrome was not found.
  pause
  exit /b 1
)

start "" "%EXHIBIT_CHROME%" --user-data-dir="%LOCALAPPDATA%\FirstFlightExhibit\Chrome" --no-first-run --no-default-browser-check --autoplay-policy=no-user-gesture-required --start-fullscreen "%~dp0index.html"
