#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/

./scripts/build.sh
dotnet pack Src
mkdir source
cp Src/bin/Debug/*.nupkg source
cp paket-files/github.com/megaease/zipkin4net/source/* source
