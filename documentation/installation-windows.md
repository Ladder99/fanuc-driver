<!-- ---
title: Windows Installation
description: 
published: true
date: 2022-09-23T02:21:38.712Z
tags: 
editor: markdown
dateCreated: 2022-09-23T02:21:38.712Z
---
 -->

# Windows Installation

Instructions documented here are for non-Docker builds.

1. Install Windows x86 .NET 7.0 Runtime from https://dotnet.microsoft.com/en-us/download/dotnet/7.0.
    * [Direct Link to 7.0.16](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-7.0.16-windows-x86-installer)
2. Download latest fanuc-driver Windows release from https://github.com/Ladder99/fanuc-driver/releases.
    * [Direct link to 0.7](https://github.com/Ladder99/fanuc-driver/releases/download/0.7/fanuc-driver-0.7-windows32.zip)
3. Download latest MTConnect Agent release from https://github.com/mtconnect/cppagent/releases.
    * [Direct link to 2.2.0.17](https://github.com/mtconnect/cppagent/releases/download/v2.2.0.17/agent-2.2.0.17-win32.zip)
4. Unblock and decompress the release archives into `c:\fanuc`, for example.
    * Right click on zip file, select properties, check the `Unblock` checkbox, and press OK.
    * Your folder structure will look similar to this:

    ```
    c:\fanuc
    |
    |- ladder99
    |  |
    |  |- fanuc-driver
    |
    |- agent-2.2.0.17-win32
    ```

Below is the folder and files structure that you can expect inside `c:\fanuc\ladder99\fanuc-driver` folder.

```
fanuc-driver
|
|- logs                             ... location of runtime log files
|
|- runtimes                         ... .NET supporting files
|
|- user
|   |
|   |- agent.cfg                    ... MTConnect Agent example configuration
|   |- config.machines.yml          ... machines configuration file
|   |- config.system.yml            ... system configuration file  
|   |- config.user.yml              ... user configuration file
|   |- devices_template.xml         ... MTConnect Agent devices information model blank template
|   |- nlog.config                  ... logging configuration file
|   |- win_install.bat              ... install fanuc-driver as a Windows Service (run as administrator)
|   |- win_restart.bat              ... restart fanuc-driver Windows Service (run as administrator)
|   |- win_run.bat                  ... run fanuc-driver from console
|   |- win_start.bat                ... start fanuc-driver Windows Service (run as administrator)
|   |- win_stop.bat                 ... stop fanuc-driver Windows Service (run as administrator)
|   |- win_uninstall.bat            ... stop and remove fanuc-driver Windows Service (run as administrator)
|   |- mtc_device_*.xml             ... MTConnect device model generated at runtime
|
|- fanuc.exe                        ... fanuc-driver executable
|- *.*                              ... fanuc-driver supporting files
```

## Configure Machines

1. Open `c:\fanuc\ladder99\fanuc-driver\user\config.machines.yml` and replace its contents with the YAML section below. YAML is tab and white space sensitive.  In the below example, use two white spaces to indent, not the tab key.  Modify the `l99.driver.fanuc.FanucMachine` section to the correct address and port of your Fanuc controller.

Below example will:
- Connect to Fanuc controller at 192.168.111.12 on port 8193.
- Collect basic machine, state, and production data. 
- Serve MTConnect SHDR on port 7878 to an MTConnect Agent.

```yaml
## ./user/config.machines.yml
## runtime configuration section
##
machines:
  - id: f_sim
    ## enabled: enable or disable collection for this machine
    enabled: !!bool true
    ## type: machine type
    type: l99.driver.fanuc.FanucMachine, fanuc
    ## strategy: strategy type
    strategy: l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc
    ## handler: handler type
    handler: l99.driver.fanuc.handlers.FanucOne, fanuc
    ## transport: transport type
    transport: l99.driver.fanuc.transports.SHDR, fanuc
    ##
    ## machine instance attributes
    ##
    l99.driver.fanuc.FanucMachine, fanuc:
      ## sweep_ms: the interval at which machine is polled
      sweep_ms: !!int 1000
      net:
        ## ip: machine network address
        ip: 192.168.111.12
        ## port: focas port
        port: !!int 8193
        ## timeout_s: focas api timeout, must be above zero
        timeout_s: !!int 3
    ##
    ## strategy instance attributes
    ##
    l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc:
      ## stay_connected: connect and disconnect every polling interval
      ##   this is helpful when collecting a lot of data to reduce the time we spend talking to the NC
      stay_connected: !!bool true
      ## exclusions: exclude paths, axes, and spindles from collection
      #exclusions:
      ## exclude path data only, but still collect axis and spindle data for this path
      #  1: 
      ## exlude axis X and spindle S data by name
      #  2: [ X, S ]
      ## exclude path data and all axes and spindles 
      #  3: [ % ]
      ## collectors: list of data collected by category
      collectors:
      - l99.driver.fanuc.collectors.MachineInfo, fanuc
      #- l99.driver.fanuc.collectors.Alarms, fanuc
      #- l99.driver.fanuc.collectors.Messages, fanuc
      - l99.driver.fanuc.collectors.StateData, fanuc
      #- l99.driver.fanuc.collectors.ToolData, fanuc
      - l99.driver.fanuc.collectors.ProductionData, fanuc
      #- l99.driver.fanuc.collectors.GCodeData, fanuc
      #- l99.driver.fanuc.collectors.AxisData, fanuc
      #- l99.driver.fanuc.collectors.SpindleData, fanuc
      #- l99.driver.fanuc.collectors.Macro, fanuc
      #- l99.driver.fanuc.collectors.Pmc, fanuc
    ##
    ## handler instance attributes
    ##
    l99.driver.fanuc.handlers.FanucOne, fanuc:
      ## iso8601: format date instead of milliseconds since 1970
      iso8601: !!bool true
      ## change_only: send data only when it changes
      change_only: !!bool false
      ## skip_interval: send internal data, useful for troubleshooting
      skip_internal: !!bool true
    ##
    ## transport instance attributes
    ##
    l99.driver.fanuc.transports.SHDR, fanuc:
      ## shdr transform scripts
      << : *default-shdr-transformers
      ## model generation
      << : *default-shdr-model-genny
      ## device_key: used to prefix shdr key to target specific device
      device_key: ~
      ## device_name: used for substitution (eg. make shdr keys unique)
      device_name: f_sim
      net:
        ## port: agent listening port
        port: !!int 7878
        ## heartbeat_ms: agent heartbeat
        heartbeat_ms: !!int 10000
        ## filter_duplicates: filter duplicates at adapter
        filter_duplicates: !!bool false
    ##
    ## alarm collector instance attributes
    ##
    l99.driver.fanuc.collectors.Alarms, fanuc:
      ## stateful: alarm list behavior
      ##   false: alarms are removed from list once cleared
      ##   true: alarms remain in list once cleared with additional properties
      stateful: !!bool true
    ##
    ## operator messages collector instance attributes
    ##
    l99.driver.fanuc.collectors.Messages, fanuc:
      ## stateful: message list behavior
      ##   false: messages are removed from list once cleared
      ##   true: messages remain in list once cleared with additional properties
      stateful: !!bool true
    ##
    ## production data instance attributes
    ##
    l99.driver.fanuc.collectors.ProductionData, fanuc:
      ## unsupported: collection of data supported by a subset of NC models
      unsupported: !!bool false
    ##
    ## g-code data instance attributes
    ##
    l99.driver.fanuc.collectors.GCodeData, fanuc:
      ## block_counter: rely on NC provided block count (not supported by all NC models)
      block_counter: !!bool false
      ## buffer_length: number of characters to read-ahead from exeuting program
      buffer_length: !!int 512
    ##
    ## macro data
    ##
    l99.driver.fanuc.collectors.Macro, fanuc:
      map:
        - id: test
          path: 1
          address: !!int 100
    ##
    ## pmc data
    ##
    l99.driver.fanuc.collectors.Pmc, fanuc:
      map:
        - id: test
          address: R0050
          type: word

```

## Prepare MTConnect Agent and Run Adapter in Console

1. Copy `c:\fanuc\ladder99\fanuc-driver\user\agent.cfg` and `c:\fanuc\ladder99\fanuc-driver\user\devices_template.xml` into `c:\fanuc\agent-2.2.0.17-win32\bin` folder.
2. Rename `c:\fanuc\agent-2.2.0.17-win32\bin\devices_template.xml` to `c:\fanuc\agent-2.2.0.17-win32\bin\devices.xml` so that it matches the `Devices` parameter inside `c:\fanuc\agent-2.2.0.17-win32\bin\agent.cfg`.
3. Set the `Host` parameter inside the `Adapter_1` section of `c:\fanuc\agent-2.2.0.17-win32\bin\agent.cfg` to `localhost` so that the MTConnect Agent knows to connect to the adapter on the same computer. `Device` and `Port` parameters will remain unchanged as defined in `c:\fanuc\ladder99\fanuc-driver\user\config.machines.yml`.

```
Adapters {

  Adapter_1 {
    Device = f_sim
    Host = localhost
    Port = 7878
  }
}
```

3. Run `c:\fanuc\ladder99\fanuc-driver\user\win_run.bat` to start the adapter.
4. Once the Fanuc controller comes online, you will see a `[f_sim] Strategy started` message.

```
2024/02/16 10:45:49.803|INFO|[f_sim] Strategy initializing |l99.driver.fanuc.strategies.FanucMultiStrategy|
2024/02/16 10:45:52.877|WARN|[f_sim] Strategy initialization pending (0 min) |l99.driver.fanuc.strategies.FanucMultiStrategy|
2024/02/16 10:46:22.523|INFO|[f_sim] Strategy initialized |l99.driver.fanuc.strategies.FanucMultiStrategy|
2024/02/16 10:46:23.615|INFO|[f_sim] Strategy started |l99.driver.fanuc.strategies.FanucMultiStrategy|
```

5. When the adapter starts it creates a `c:\fanuc\ladder99\fanuc-driver\mtc_device_f_sim.xml` file.  This file contains the MTConnect Device Information model for our `f_sim` device.  Copy the contents of this file into `c:\fanuc\agent-2.2.0.17-win32\bin\devices.xml`, between `<Devices> </Devices>`.

```xml
<MTConnectDevices 
  xmlns:m="urn:mtconnect.org:MTConnectDevices:2.0" 
  xmlns="urn:mtconnect.org:MTConnectDevices:2.0" 
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
  xsi:schemaLocation="urn:mtconnect.org:MTConnectDevices:2.0 http://schemas.mtconnect.org/schemas/MTConnectDevices_2.0.xsd">
  <Header creationTime="2022-02-22T00:00:00Z" sender="SENDER_ID" instanceId="" version="2.0.0.0" deviceModelChangeTime="2022-02-22T00:00:00Z" assetBufferSize="8096" assetCount="140" bufferSize="4096">
  </Header>
  <Devices>
    <!-- <Device/> elements generated by fanuc-driver go here -->
  </Devices>
</MTConnectDevices>
```

6. Start the MTConnect Agent by running `C:\fanuc\agent-2.2.0.17-win32\bin\agent.exe debug` from Command Prompt.  When the MTConnect Agent connects to the adapter you will see a successful connection in the MTConnect Agent console output.

```
2024-02-16T17:04:34.189193Z (0x0000454c) [debug] Connector::resolved->Connector::connect: Connecting to data source: localhost on port: 7878
2024-02-16T17:04:36.219061Z (0x0000454c) [info] Connector::connected: Connected with: 127.0.0.1:7878
2024-02-16T17:04:36.219061Z (0x0000454c) [debug] Connector::connected->Connector::sendCommand: (Port:62859) Sending PING
2024-02-16T17:04:36.232062Z (0x0000454c) [debug] Connector::reader->Connector::parseSocketBuffer->Connector::processLine: (Port:62859) Received a PONG for localhost on port 7878
```

7. Open `http://localhost:5000/current` in your browser to view the current MTConnect Agent values.  

8. Let's enable more data to be collected from the controller.  Open `c:\fanuc\ladder99\fanuc-driver\user\config.machines.yml` and uncomment the following in the `collectors` section.

```yaml
collectors:
      - l99.driver.fanuc.collectors.MachineInfo, fanuc
      - l99.driver.fanuc.collectors.Alarms, fanuc
      - l99.driver.fanuc.collectors.Messages, fanuc
      - l99.driver.fanuc.collectors.StateData, fanuc
      - l99.driver.fanuc.collectors.ToolData, fanuc
      - l99.driver.fanuc.collectors.ProductionData, fanuc
      - l99.driver.fanuc.collectors.GCodeData, fanuc
      - l99.driver.fanuc.collectors.AxisData, fanuc
      - l99.driver.fanuc.collectors.SpindleData, fanuc
      #- l99.driver.fanuc.collectors.Macro, fanuc
      #- l99.driver.fanuc.collectors.Pmc, fanuc
```

9. Save the file and restart the adapter by running `c:\fanuc\ladder99\fanuc-driver\user\win_run.bat`.
10. Refresh `http://localhost:5000/current`.


## Running as a Windows Service

1. Run `./user/win_install.bat` as administrator to install the fanuc-driver Windows Service.
2. Modify the fanuc-driver Windows Service to log-on as `Network Service` user.
3. Run `./user/win_start.bat` to start fanuc-driver Windows Service.