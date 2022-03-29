#!/bin/bash

mkdir ~/fanuc
cd ~/fanuc

sudo rm -rf volumes
sudo rm -rf fanuc-driver

git clone --recurse-submodules -j8 https://github.com/Ladder99/fanuc-driver.git

cd fanuc-driver
git checkout develop
cd base-driver
git checkout develop

cd ~/fanuc/fanuc-driver

profile=mtc
os=LINUX64
image=ladder99/fanuc-driver
commit=$(git rev-parse --short HEAD)
tag=linux64-develop-$commit

docker build \
	-f Dockerfile.$os \
  --tag=$image:$tag .

cd ~/fanuc/fanuc-driver/examples

mkdir -p ../../volumes/fanuc-driver
cp fanuc-driver/nlog-example-linux.config ../../volumes/fanuc-driver/nlog.config
cp fanuc-driver/config-example.yml ../../volumes/fanuc-driver/config.yml

mkdir -p ../../volumes/mosquitto/config
mkdir -p ../../volumes/mosquitto/data
mkdir -p ../../volumes/mosquitto/log
cp mosquitto.conf ../../volumes/mosquitto/config/mosquitto.conf

mkdir -p ../../volumes/agent
cp mtconnect/agent.cfg ../../volumes/agent/agent.cfg
cp mtconnect/devices_example.xml ../../volumes/agent/devices.xml

# the architecture you are on (linux64,arm)
# FOCAS_TGT sets the docker image tag
#export FOCAS_TGT=$( docker images ladder99/fanuc-driver | tail -n +2 | awk 'NR==1{print $1":"$2}' )
export FOCAS_TGT=$tag

docker-compose -f compose.yml --profile $profile up -d