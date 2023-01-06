#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/../

dotnet nuget locals all --clear
dotnet tool restore
dotnet paket restore