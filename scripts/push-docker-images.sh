#!/bin/bash

set -eo pipefail

REPOSITORY_URI=$1
GIT_COMMIT=$2

if [ "$CODEBUILD_GIT_BRANCH" = "master" ]; then
    docker push $REPOSITORY_URI:$GIT_COMMIT;
    docker push $REPOSITORY_URI:latest;
fi