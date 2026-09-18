@echo off
setlocal
if defined MOYVA_PYTHON (
  set "PYTHON=%MOYVA_PYTHON%"
) else if exist "%~dp0.venv-training\Scripts\python.exe" (
  set "PYTHON=%~dp0.venv-training\Scripts\python.exe"
) else (
  set "PYTHON=python"
)
"%PYTHON%" "%~dp0tools\ai\moyva_train.py" %*
exit /b %errorlevel%
