@echo off
cd %~dp0

start SM1/AMISystemManagementUI.exe
start Agg1/AMIAggregator.exe
start Agg2/AMIAggregator.exe
start Dev1/RESProjekat.exe
start Dev2/RESProjekat.exe
start Dev3/RESProjekat.exe

exit
