# fanuc-driver
  
## armv7
  
Follow .NET Core SDK installation instructions here: https://sukesh.me/2020/07/07/how-to-install-net-core-on-raspberry-pi/  
  
Clone the repository, build the project, and run it.  
  
```  
git clone https://github.com/Ladder99/fanuc-driver.git  

cd fanuc-driver/fanuc  

dotnet build  /nowarn:CS0618 -p:DefineConstants=ARMV7  

./bin/Debug/netcoreapp3.1/fanuc  
```
  
## win32
  
Install JetBrains Rider and build for 32-bit CPU.  
  
## linux64

### DOES NOT WORK: assuming interop field sizes have to be redone

Follow .NET Core SDK installation instructions here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
  
Clone the repository, build the project, and run it.  
  
```  
git clone https://github.com/Ladder99/fanuc-driver.git  

cd fanuc-driver/fanuc  

dotnet build  /nowarn:CS0618 -p:DefineConstants=LINUX64 

./bin/Debug/netcoreapp3.1/fanuc  
```
  
## linux32
  
Not tested.  
  