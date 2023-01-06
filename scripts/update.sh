#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/../

dotnet paket update
$SCRIPT_PATH/build.sh