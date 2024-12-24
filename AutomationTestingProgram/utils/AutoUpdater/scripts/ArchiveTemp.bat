@echo off

set _my_datetime=%date%_%time%
set _my_datetime=%_my_datetime: =_%
set _my_datetime=%_my_datetime::=%
set _my_datetime=%_my_datetime:/=_%
set _my_datetime=%_my_datetime:.=_%
set path=%1

if NOT EXIST %path% (
    mkdir %path%
)

if NOT EXIST %path%\Archive (
    mkdir %path%\Archive
)

REM Taskkill /IM "soffice.bin" /F

for /f "tokens=1* delims=." %%i in ('dir %path%\ /b 2^> nul') do (
    if Exist "%path%\Archive\%%i.%%j" (
       echo %%i.%%j exists
       move "%path%\%%i.%%j" "%path%\%%i_%_my_datetime%.%%j"
       move "%path%\%%i_%_my_datetime%.%%j" "%path%\Archive\%%i_%_my_datetime%.%%j"
    ) Else (
	move "%path%\%%i.%%j" "%path%\Archive\%%i.%%j" 2> nul
    )
)
exit 0