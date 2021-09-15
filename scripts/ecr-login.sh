#!/bin/bash

set -eo pipefail

aws ecr-public get-login-password | docker login --username AWS --password-stdin public.ecr.aws