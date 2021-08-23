echo Copying "CSASM.Core.dll" to "..\..\..\CSASM\"
:: Error codes less than 8 should just be ignored
(robocopy . ..\..\..\..\CSASM\ CSASM.Core.dll) ^& IF %ERRORLEVEL% LSS 8 SET ERRORLEVEL = 0