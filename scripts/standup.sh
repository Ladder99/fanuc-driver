#!/bin/bash

# stop and remove containers
docker container stop fanuc_driver
docker container stop agent
docker container rm fanuc_driver
docker container rm agent
docker container prune

# remove images
docker rmi $(docker images --format '{{.Repository}}:{{.Tag}}' | grep 'ladder99/fanuc-driver')
docker rmi $(docker images --format '{{.Repository}}:{{.Tag}}' | grep 'ladder99/agent')
docker image prune

mkdir ~/fanuc
cd ~/fanuc

# remove directories
sudo rm -rf volumes
sudo rm -rf fanuc-driver

# clone repo
git clone --recurse-submodules -j8 https://github.com/Ladder99/fanuc-driver.git

# checkout develop branch
cd fanuc-driver
git checkout develop
cd base-driver
git checkout develop

cd ~/fanuc/fanuc-driver

# set vars for build
profile=mtc
os=LINUX64
image=ladder99/fanuc-driver
commit=$(git rev-parse --short HEAD)
tag=linux64-develop-$commit

# build driver container
docker build \
	-f Dockerfile.$os \
  --tag=$image:$tag .

cd ~/fanuc/fanuc-driver/examples

# prepare volumes
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
# set the driver image tag
#export FOCAS_TGT=$( docker images ladder99/fanuc-driver | tail -n +2 | awk 'NR==1{print $1":"$2}' )
export FOCAS_TGT=$tag

# standup profile
docker-compose -f compose.yml --profile $profile up -d
