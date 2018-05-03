@echo off
REM This batch file registers a custom protocol handler for yashbot.
REM Example: yashbot://xinlX2VJgvY
REG ADD HKEY_CLASSES_ROOT\yashbot /f
REG ADD HKEY_CLASSES_ROOT\yashbot /ve /d "URL:yashbot" /f
REG ADD HKEY_CLASSES_ROOT\yashbot\ /v "URL Protocol" /t REG_SZ /d "" /f
SET work_dir=%~dp0
SET exe_path="\"%work_dir%yashbot.exe\" \"%%1\""
REG ADD HKEY_CLASSES_ROOT\yashbot\shell\open\command /ve /d %exe_path% /f
