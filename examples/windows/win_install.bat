sc.exe create fanuc-driver binpath= "%~dp0..\fanuc.exe --nlog %~dp0\nlog.config --config %~dp0\config.system.yml,%~dp0\config.user.yml,%~dp0\config.machines.yml" DisplayName= "fanuc-driver" start= auto
sc.exe description fanuc-driver "Ladder99 Fanuc Focas data acquisition driver"