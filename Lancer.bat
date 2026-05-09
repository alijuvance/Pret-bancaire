@echo off
title Systeme de Pret Bancaire
echo ======================================
echo   Systeme de Pret Bancaire
echo   Demarrage de l'application...
echo ======================================
echo.

REM Trouver dotnet automatiquement
set "DOTNET=dotnet"
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    if exist "C:\Program Files\dotnet\dotnet.exe" (
        set "DOTNET=C:\Program Files\dotnet\dotnet.exe"
    ) else (
        echo ERREUR: dotnet n'est pas installe ou introuvable.
        pause
        exit /b 1
    )
)

"%DOTNET%" run --project "%~dp0PretBancaire\PretBancaire.csproj"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Une erreur est survenue. Appuyez sur une touche...
    pause
)
