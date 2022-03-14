#!/bin/bash

set -eo pipefail

REPOSITORY_URI=$1
GIT_COMMIT=$2

docker buildx build --load --platform linux/arm64 --tag $REPOSITORY_URI:$GIT_COMMIT .
docker tag $REPOSITORY_URI:$GIT_COMMIT $REPOSITORY_URI:latest