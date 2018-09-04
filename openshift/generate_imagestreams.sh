#!/bin/bash

IFS='%'

HEADER='{
    "kind": "List",
    "apiVersion": "v1",
    "metadata": {},
    "items": [
        {
            "kind": "BuildConfig",
            "apiVersion": "v1",
            "metadata": {
                "name": "mssql2017",
                "annotations": {
                    "description": "Builds the mssql:2017 image"
                }
            },
            "spec": {
                "source": {
                    "dockerfile": "'

FOOTER='"
                },
                "strategy": {
                    "type": "Docker"
                },
                "output": {
                    "to": {
                        "kind": "ImageStreamTag",
                        "name": "mssql:2017"
                    }
                },
                "triggers": [
                    {
                        "type": "ImageChange"
                    },
                    {
                        "type": "ConfigChange"
                    }
                ]
            }
        },
        {
            "kind": "ImageStream",
            "apiVersion": "v1",
            "metadata": {
                "name": "mssql"
            }
        }
    ]
}'

DOCKERFILE=$(cat Dockerfile)
# trim comments
DOCKERFILE=$(echo $DOCKERFILE | sed '/^#/ d')
# trim empty lines
DOCKERFILE=$(echo $DOCKERFILE | sed '/^\s*$/d')
# escape \
DOCKERFILE=$(echo $DOCKERFILE | sed 's/\\/\\\\/g')
# escape "
DOCKERFILE=$(echo $DOCKERFILE | sed 's/\"/\\"/g')
# escape \n
DOCKERFILE=$(echo $DOCKERFILE | sed ':a;N;$!ba;s/\n/\\n/g')

echo -n ${HEADER}${DOCKERFILE}${FOOTER} >imagestreams.json