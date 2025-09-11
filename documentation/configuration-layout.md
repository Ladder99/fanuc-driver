<!-- ---
title: Configuration Layout
description: 
published: true
date: 2022-09-23T18:15:27.084Z
tags: 
editor: markdown
dateCreated: 2022-09-23T18:09:06.220Z
--- -->

# Configuration Layout

The YAML configuration example is composed of three parts: [system](https://raw.githubusercontent.com/Ladder99/fanuc-driver/main/examples/fanuc-driver/config-example.system.yml), [user](https://raw.githubusercontent.com/Ladder99/fanuc-driver/main/examples/fanuc-driver/config-example.user.yml), [machines](https://raw.githubusercontent.com/Ladder99/fanuc-driver/main/examples/fanuc-driver/config-example.machines.yml). The examples are best viewed using https://jsonformatter.org/yaml-parser because it dereferences nodes to give you a complete view.

## Major Sections

Configuration is broken down into three main sections:

- `System`
  - Node templates referenced elsewhere within the configuration.
  - Default transformations for supported transports.
- `User`
  - Fanuc machine properties such as IP address and polling interval.
  - Groupings of data to be collected from the machine.
  - Transport configuration properties such as what port to emit SHDR on.
- `Machines`
  - Defines each machine's pipleine during runtime by assembling sections from `System` and `User`.
  
## Configuring a Machine

Configuration of each machine are simply a YAML merge (`<<`) and dereference (`*`) operations of existing nodes in the YAML file.

### Short Form

```yaml
machines:
  - id: my_fanuc
    <<: *machine-base
    #<<: *machine-disabled
    <<: *source-1
    <<: *collector-1
    <<: *target-1
    <<: *no-filter
```

`*machine-base` reference is expended into:

```yaml
#machine:
  machine-base: &machine-base
    enabled: !!bool true
    type: l99.driver.fanuc.FanucMachine, fanuc
    strategy: l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc
    handler: l99.driver.fanuc.handlers.FanucOne, fanuc
```

`*source-1` reference is expanded into:

```yaml
#user:
  source-1: &source-1
    l99.driver.fanuc.FanucMachine, fanuc:
      sweep_ms: !!int 1000
      net:
        ip: 10.1.10.211
        port: !!int 8193
        timeout_s: !!int 3
```

### Long Form

At run time the machine is expanded as shown below.  Note that we are not dereferencing `&default-shdr-transformers` or `&default-shdr-model-genny` for brevity. 

```yaml
machines:
  - id:	my_fanuc
  enabled: !!bool true
  type: l99.driver.fanuc.FanucMachine, fanuc
  strategy: l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc
  handler: l99.driver.fanuc.handlers.FanucOne, fanuc
  l99.driver.fanuc.FanucMachine, fanuc:
    sweep_ms: !!int 1000
    net:
      ip:	10.1.10.211
      port: !!int 8193
      timeout_s: !!int 3
  l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc:
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
  l99.driver.fanuc.transports.SHDR, fanuc:
    << : *default-shdr-transformers
    << : *default-shdr-model-genny
    device_name: f_sim
      net:
        port: !!int 7878
        heartbeat_ms: !!int 10000
        interval_ms: !!int 1000
  l99.driver.fanuc.handlers.FanucOne, fanuc:
    change_only: !!bool true
    skip_internal: !!bool true
```

Long form is completely acceptable and typically more readable.

## Limiting Collected Data

The default data collectors have been tested against a wide range of Fanuc controllers.  However, if you run into unexpected issues, you can modify what data is collected by removing specific collectors.

```yaml
machines:
  - id:	my_fanuc
  enabled: !!bool true
  ...
  l99.driver.fanuc.strategies.FanucMultiStrategy, fanuc:
    collectors:
    - l99.driver.fanuc.collectors.MachineInfo, fanuc
    #- l99.driver.fanuc.collectors.Alarms, fanuc
    #- l99.driver.fanuc.collectors.Messages, fanuc
    - l99.driver.fanuc.collectors.StateData, fanuc
    #- l99.driver.fanuc.collectors.ToolData, fanuc
    #- l99.driver.fanuc.collectors.ProductionData, fanuc
    #- l99.driver.fanuc.collectors.GCodeData, fanuc
    #- l99.driver.fanuc.collectors.AxisData, fanuc
    #- l99.driver.fanuc.collectors.SpindleData, fanuc
  ...
```
