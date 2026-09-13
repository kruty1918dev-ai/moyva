@echo off
setlocal
if not defined MOYVA_PYTHON set "MOYVA_PYTHON=python"
"%MOYVA_PYTHON%" "%~dp0tools\ai\moyva_train.py" %*
exit /b %errorlevel%
