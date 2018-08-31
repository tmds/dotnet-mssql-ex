## Adding SQL Server to OpenShift

There is no pre-built SQL Server image. We can make OpenShift build this image for us. This requires OpenShift to be configured with subscription credentials and the Docker strategy allowed. 

Use the following command to build the SQL Server image in OpenShift:

```
$ oc create -f https://raw.githubusercontent.com/tmds/dotnet-mssql-ex/master/openshift/imagestreams.json
```

**note**: this image is not automatically rebuild for security fixes. To trigger a build execute: `oc start-build mssql2017`.

To facilitate setting up SQL Server instances, you can import this template:

```
$ oc create -f https://raw.githubusercontent.com/tmds/dotnet-mssql-ex/master/openshift/template.json
```

## Deploying SQL Server

Using the template:

```
$ oc new-app --template=mssql -p NAME=mssql1 -p ACCEPT_EULA=Y -p NAMESPACE=`oc project -q`
```

To see all parameters of the template, you can use `oc process --parameters mssql`.

```
$ oc process --parameters mssql
NAME                DESCRIPTION                                                                  GENERATOR           VALUE
NAME                The name assigned to all of the frontend objects defined in this template.                       mssql
MEMORY_LIMIT        Maximum amount of memory the container can use.                                                  512Mi
IMAGE               The SQL Server image tag.                                                                        mssql:2017
NAMESPACE           The OpenShift namespace where the SQL Server image resides.                                      openshift
MSSQL_SA_PASSWORD                                                                                expression          [a-zA-Z0-9]{8}
ACCEPT_EULA         'Y' to accept the EULA (https://go.microsoft.com/fwlink/?linkid=857698).                         
VOLUME_CAPACITY     Volume space available for data, e.g. 512Mi, 2Gi                                                 512Mi
```
