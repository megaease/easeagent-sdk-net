#!/usr/bin/env bash

set -e

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/

./build.sh
dotnet pack Src
mkdir source
cp Src/bin/Debug/*.nupkg source
