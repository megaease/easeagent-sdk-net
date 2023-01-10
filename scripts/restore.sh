#!/usr/bin/env bash

pushd `dirname $0` > /dev/null
SCRIPT_PATH=`pwd -P`
cd $SCRIPT_PATH/../

dotnet restore Src
dotnet restore Tests
dotnet restore Examples/aspnetcore/common
dotnet restore Examples/aspnetcore/backend
dotnet restore Examples/aspnetcore/frontend