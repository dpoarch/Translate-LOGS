Translate.exe SpencerGifts.Translate.Plugin.TLog.IOrders
IF ERRORLEVEL 1 GOTO Failed

del "\\sgaw\sybwork\iorders\download\iorders.go"
EXIT /B 0


:Failed
del "\\sgaw\sybwork\iorders\download\iorders.go"
EXIT /B 1 