@echo off
set /p "path=path to Video Tool(with executable): "
set data=\"%path%\" \"%%1\"
pause
reg add "HKCR\*\shell\VideoTool" /d "Video Tool"
reg add "HKCR\*\shell\VideoTool\command" /d "%data%"
timeout 3