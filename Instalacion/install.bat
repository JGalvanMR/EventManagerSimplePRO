@echo off
echo ========================================
echo  INSTALADOR EVENT MANAGER SIMPLE
echo ========================================
echo.

REM Crear carpeta de instalación
set INSTALL_DIR=C:\EventManager
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copiar archivos necesarios
echo Copiando archivos...
xcopy /E /Y ".\*" "%INSTALL_DIR%\"

REM Crear acceso directo en escritorio
echo Creando acceso directo...
set SCRIPT="%TEMP%\CreateShortcut.vbs"
echo Set oWS = WScript.CreateObject("WScript.Shell") > %SCRIPT%
echo sLinkFile = "%USERPROFILE%\Desktop\Event Manager.lnk" >> %SCRIPT%
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> %SCRIPT%
echo oLink.TargetPath = "%INSTALL_DIR%\EventManagerSimple.exe" >> %SCRIPT%
echo oLink.WorkingDirectory = "%INSTALL_DIR%" >> %SCRIPT%
echo oLink.Description = "Gestor de Eventos Offline" >> %SCRIPT%
echo oLink.Save >> %SCRIPT%
cscript /nologo %SCRIPT%
del %SCRIPT%

echo.
echo ========================================
echo  INSTALACIÓN COMPLETADA
echo ========================================
echo.
echo La aplicación ha sido instalada en:
echo %INSTALL_DIR%
echo.
echo Se ha creado un acceso directo en el escritorio.
echo.
echo Presiona cualquier tecla para salir...
pause > nul