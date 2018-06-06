@echo off
cd %~dp0

start SM1/AMISystemManagementUI.exe

cd Agg1
start Agg1Bat.bat
cd..

cd Agg2
start Agg2Bat.bat
cd..

cd Dev1
start DevBat.bat
cd..

cd Dev2
start DevBat.bat
cd..

cd Dev3
start DevBat.bat


exit
