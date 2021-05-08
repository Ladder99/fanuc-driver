# fanuc-driver

## armv7
  
# https://sukesh.me/2020/07/07/how-to-install-net-core-on-raspberry-pi/  
git clone https://github.com/Ladder99/fanuc-driver.git  
cd fanuc-driver/fanuc  
dotnet build -p:DefineConstants=ARMV7  
./bin/Debug/netcoreapp3.1/fanuc  
