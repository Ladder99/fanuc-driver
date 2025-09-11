<!-- ---
title: Docker Installation
description: 
published: true
date: 2022-09-23T02:12:10.506Z
tags: 
editor: markdown
dateCreated: 2022-09-23T01:58:36.483Z
---
 -->

# Docker Installation

## Install Docker and Compose

```bash
curl -fsSL https://get.docker.com -o ~/get-docker.sh
sudo sh ~/get-docker.sh

sudo groupadd docker

sudo usermod -aG docker "$USER"
newgrp docker
sudo systemctl enable --now docker
```

## Select Latest Image

[Docker Hub Images](https://hub.docker.com/r/ladder99/fanuc-driver/tags)

Export the latest image tag to `FOCAS_TGT` environment variable.    
Docker-compose will use this variable to pull the correct image.

For x86 Linux architecture use:

```bash
export FOCAS_TGT=linux64-d538eef
```

For ARM Linux architecture use:

```bash
export FOCAS_TGT=arm-d538eef
```

## Clone Repository

```bash
mkdir ~/fanuc
cd ~/fanuc

git clone --recurse-submodules -j8 \
	https://github.com/Ladder99/fanuc-driver.git
```

If you are working with the develop branch then you'll want to check that branch out.

```bash
cd fanuc-driver
git checkout develop
cd base-driver
git checkout develop
```

## Prepare Volumes

Below commands copy the basic configuration files for Fanuc Driver, Mosquitto, and MTConnect Agent.

```bash
cd ~/fanuc/fanuc-driver/examples

mkdir -p ../../volumes/fanuc-driver
cp docker/nlog.config ../../volumes/fanuc-driver/nlog.config
cp docker/config.system.yml ../../volumes/fanuc-driver/config.system.yml
cp docker/config.user.yml ../../volumes/fanuc-driver/config.user.yml
cp docker/config.machines.yml ../../volumes/fanuc-driver/config.machines.yml

mkdir -p ../../volumes/mosquitto/config
mkdir -p ../../volumes/mosquitto/data
mkdir -p ../../volumes/mosquitto/log
cp mosquitto.conf ../../volumes/mosquitto/config/mosquitto.conf

mkdir -p ../../volumes/agent/config
mkdir -p ../../volumes/agent/data
mkdir -p ../../volumes/agent/log
cp docker/agent.cfg ../../volumes/agent/config/agent.cfg
cp docker/devices_template.xml ../../volumes/agent/config/devices.xml
```

## Start Profile

Available profiles:  

| Image | Profiles | Description |
| --- | --- | --- |
| [Portainer](https://hub.docker.com/r/portainer/portainer-ce) | `all`, `admin` | Visual Docker management. |
| [Dozzle](https://hub.docker.com/r/amir20/dozzle/) | `all`, `admin` | Docker log concentrator. |
| [Influx](https://hub.docker.com/_/influxdb) | `all`, `influx` | Time series database. |
| [Agent Unoffical](https://hub.docker.com/r/ladder99/agent) | `all`, `mtc_2203` | Unnoficial MTConnect Agent v2.2.0.3. |
| [Agent Official](https://hub.docker.com/r/mtconnect/agent) | `all`, `mtc` | Official MTConnect Agent latest. |
| [Mosquitto](https://hub.docker.com/_/eclipse-mosquitto) | `all`, `mtc`, `mqtt` | MQTT broker. |
| [Fanuc Driver](https://hub.docker.com/r/ladder99/fanuc-driver) | `all`, `influx`, `mqtt`, `spb`, `mtc_2203`, `mtc`, `fanuc` | Fanuc Focas adapter. |

If you are using the Fanuc Driver in combination with Ladder99 MTConnect Historian then stand up the `fanuc` profile.  
If you want to output data to InfluxDb, MQTT, or as SparkplugB then select the appropriate profile, in [addition](https://docs.docker.com/compose/profiles/#start-multiple-profiles) to `admin`.

```bash
cd ~/fanuc/fanuc-driver/examples

profile=fanuc

docker compose --project-name fanuc --file compose.yml --profile $profile --verbose pull
docker compose --project-name fanuc --file compose.yml --profile $profile --verbose create
docker compose --project-name fanuc --file compose.yml --profile $profile --verbose start
```

# Building an Image

1. Clean Environment

```bash
docker container stop fanuc_driver
docker container stop agent
docker container rm fanuc_driver
docker container rm agent
docker container prune
```

```bash
docker rmi $(docker images --format '{{.Repository}}:{{.Tag}}' | grep '\(ladder99\|mtconnect\)/agent\|ladder99/fanuc-driver')
docker image prune
```

```bash
sudo rm -rf ~/fanuc
```

2. [Clone Repository](#clone-repository)


3. Prepare Build

For x86 Linux architecture use:

```bash
cd ~/fanuc/fanuc-driver

# set vars for build
os=LINUX64
image=ladder99/fanuc-driver
commit=$(git rev-parse --short HEAD)
tag=linux64-$commit
```

For ARM Linux architecture use:

```bash
cd ~/fanuc/fanuc-driver

# set vars for build
os=ARM
image=ladder99/fanuc-driver
commit=$(git rev-parse --short HEAD)
tag=arm-$commit
```

4. Build Image

```bash
# build driver container
docker build \
	-f Dockerfile.$os \
  --tag=$image:$tag .

# the architecture you are on (linux64,arm)
# set the driver image tag
#export FOCAS_TGT=$( docker images ladder99/fanuc-driver | tail -n +2 | awk 'NR==1{print $1":"$2}' )
export FOCAS_TGT=$tag
```

5. Push Image

```bash
docker login

docker push ladder99/fanuc-driver:$FOCAS_TGT
```

6. [Prepare Volumes](#prepare-volumes) 

7. [Start Profile](#start-profile)

