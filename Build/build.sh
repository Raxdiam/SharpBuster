#!/bin/bash

PROJECT=../SharpBuster/SharpBuster.csproj
BUILD_DIR=../Build
CONFIGURATION=Release
SINGLE_FILE=true
RID=$1

dotnet publish $PROJECT -c $CONFIGURATION -r $RID -p:PublishDir=$BUILD_DIR/$RID -p:PublishSingleFile=$SINGLE_FILE