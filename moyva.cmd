@echo off
setlocal
if defined MOYVA_PYTHON (
  set "CLI_PYTHON=%MOYVA_PYTHON%"
) else if exist "%~dp0.venv-training\Scripts\python.exe" (
  set "CLI_PYTHON=%~dp0.venv-training\Scripts\python.exe"
) else (
  set "CLI_PYTHON=python"
)
"%CLI_PYTHON%" "%~dp0tools\ai\moyva_cli_main.py" %*
exit /b %ERRORLEVEL%
