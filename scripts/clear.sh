#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/../

dotnet nuget locals all --clear
dotnet tool restore
rm -rf source
rm -rf packages
rm -rf paket-files