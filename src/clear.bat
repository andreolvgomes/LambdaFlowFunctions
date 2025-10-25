@echo off
SETLOCAL EnableDelayedExpansion

echo Limpando pastas obj dos projetos .NET Core...
echo.

set "rootDir=%~dp0"
set "count=0"

for /d /r "%rootDir%" %%d in (obj) do (
    if exist "%%d" (
        echo Encontrado: %%d
        rmdir /s /q "%%d"
        set /a count+=1
    )
)

echo.
echo Concluído! Pastas obj removidas: %count%
echo.
pause