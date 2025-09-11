<!-- ---
title: Linux Installation
description: 
published: true
date: 2022-09-23T02:21:38.712Z
tags: 
editor: markdown
dateCreated: 2022-09-23T02:21:38.712Z
---
 -->

# Linux Installation

> WARNING: Instructions need to be updated.
<!-- {.is-warning} -->

Instructions documented here are for non-Docker builds.

## ARMv7 Linux

[Install .NET SDK](https://sukesh.me/2020/07/07/how-to-install-net-core-on-raspberry-pi/)

```bash
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet

cd ~

git clone --recurse-submodules -j8 \
	https://github.com/Ladder99/fanuc-driver.git

cd fanuc-driver/fanuc

dotnet restore "fanuc.csproj"

dotnet build "fanuc.csproj" \
	-c Release \
	/nowarn:CS0618 \
	/nowarn:CS8632 \
	/nowarn:CS1998 \
	/nowarn:CS8032 \
	-p:DefineConstants=ARMV7

dotnet publish "fanuc.csproj" \
	-c Release \
	/nowarn:CS0618 \
	/nowarn:CS8632 \
	/nowarn:CS1998 \
	/nowarn:CS8032 \
	-p:DefineConstants=ARMV7

./bin/Release/netcoreapp3.1/fanuc \
	--nlog ../examples/fanuc-driver/nlog-example-nlog.config \
  --config ../examples/fanuc-driver/config-example.system.yml,../examples/fanuc-driver/config-example.user.yml,../examples/fanuc-driver/config-example.machines.yml
```

## x86 Linux

[Install .NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)

```bash
export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet

# You may want to disable the .NET telemetry
export DOTNET_CLI_TELEMETRY_OPTOUT='true'

# Choose a folder for `fanuc-driver` (it should be either empty or non-existent, but the parent folder needs to exist)
fanuc_driver_folder="$HOME/fanuc-driver"

# Clone the sources
git clone --recurse-submodules -j8 https://github.com/Ladder99/fanuc-driver.git "$fanuc_driver_folder"

dotnet publish "$fanuc_driver_folder/fanuc/fanuc.csproj" --self-contained true --runtime linux-x64 /nowarn:CS0618 /nowarn:CS8632 /nowarn:CS1998 -p:DefineConstants=LINUX64

# Configure `config.yml` and `nlog.config`
# Example files: "$fanuc_driver_folder/fanuc/config.yml" and "$fanuc_driver_folder/fanuc/nlog.config"

# Start an MQTT broker (outside `fanuc-driver`)

# Start `fanuc-driver`
"$fanuc_driver_folder/fanuc/bin/Debug/netcoreapp3.1/linux-x64/publish/fanuc" --nlog "$fanuc_driver_folder/fanuc/nlog.config" --config "$fanuc_driver_folder/fanuc/config.system.yml,$fanuc_driver_folder/fanuc/config.user.yml,$fanuc_driver_folder/fanuc/config.machines.yml"
```

