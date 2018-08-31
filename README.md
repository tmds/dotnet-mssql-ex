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
$ oc new-app --template=mssql -p NAME=mssql1 -p ACCEPT_EULA=y -p NAMESPACE=`oc project -q`
```