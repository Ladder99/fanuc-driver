# fanuc-driver
  
This solution is built on top of Fanuc Focas libraries for interfacing with Fanuc controllers and publishing data to an MQTT broker or another target.

The primary goal of this solution is to maintain the machine data in its native source format with slight transformations to make it more human readable at the target.  The intention behind this approach is to allow the developer to reference original Focas API documentation further downstream to aid in their transformation and translation efforts.   

## MQTT Topic Structure - Suggested

```
fanuc/{machine-id}/{observation-name}
```

```
fanuc/{machine-id}/{observation-name}/{controller-execution-path-number}
```

```
fanuc/{machine-id}/{observation-name}/{controller-execution-path-number}/{machine-axis-name}
```

## MQTT Payload Structure - Suggested



## Building and Running

### armv7
  
Follow .NET Core SDK installation instructions here: https://sukesh.me/2020/07/07/how-to-install-net-core-on-raspberry-pi/  
  
Clone the repository, build the project, and run it.  
  
```  
git clone https://github.com/Ladder99/fanuc-driver.git  

cd fanuc-driver/fanuc  

dotnet build  /nowarn:CS0618 -p:DefineConstants=ARMV7  

./bin/Debug/netcoreapp3.1/fanuc  
```
  
### win32
  
Install JetBrains Rider and build for 32-bit CPU.  
  
### linux64

#### DOES NOT WORK: assuming interop field sizes do not match architecture.

Follow .NET Core SDK installation instructions here: https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu  
  
Clone the repository, build the project, and run it.  
  
```  
git clone https://github.com/Ladder99/fanuc-driver.git  

cd fanuc-driver/fanuc  

dotnet build  /nowarn:CS0618 -p:DefineConstants=LINUX64 

./bin/Debug/netcoreapp3.1/fanuc  
```
  
### linux32
  
Not tested.  
  