sc.exe stop fanuc-driver

:loop
sc.exe query fanuc-driver | find "STOPPED"
if errorlevel 1 (
  timeout 1
  goto loop
)

sc.exe start fanuc-driver
