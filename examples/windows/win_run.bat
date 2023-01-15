cd c:\ladder99\fanuc-driver

fanuc.exe ^
	--nlog .\user\nlog.config ^
	--config .\user\config.system.yml,.\user\config.user.yml,.\user\config.machines.yml
