#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/

rm -rf Src/bin/Release/*.nupkg
rm -rf source
./scripts/build.sh
dotnet pack Src --configuration Release
mkdir source
cp Src/bin/Release/*.nupkg source