# mssql-server-rhel
# Maintainers: Travis Wright (twright-msft on GitHub)
# GitRepo: https://github.com/twright-msft/mssql-server-rhel

# Base OS layer: latest RHEL 7
FROM registry.access.redhat.com/rhel7

### Atomic/OpenShift Labels - https://github.com/projectatomic/ContainerApplicationGenericLabels
LABEL name='microsoft/mssql-server-linux' \
      vendor='Microsoft' \
      version='14.0' \
      release='1' \
      summary='MS SQL Server' \
      description='MS SQL Server is .....' \
### Required labels above - recommended below
      url='https://www.microsoft.com/en-us/sql-server/' \
      io.k8s.description='MS SQL Server is .....' \
      io.k8s.display-name='MS SQL Server'

# Install latest mssql-server package
RUN REPOLIST=rhel-7-server-rpms,packages-microsoft-com-mssql-server-2017,packages-microsoft-com-prod && \
    curl -o /etc/yum.repos.d/mssql-server.repo https://packages.microsoft.com/config/rhel/7/mssql-server-2017.repo && \
    curl -o /etc/yum.repos.d/msprod.repo https://packages.microsoft.com/config/rhel/7/prod.repo && \
    ACCEPT_EULA=Y yum -y install --disablerepo '*' --enablerepo ${REPOLIST} --setopt=tsflags=nodocs \
      mssql-server mssql-tools unixODBC-devel && \
    yum clean all

RUN echo $'#!/bin/sh\\nif ! whoami &> /dev/null; then\\n  if [ -w /etc/passwd ]; then\\n    echo "${USER_NAME:-sqlservr}:x:$(id -u):0:${USER_NAME:-sqlservr} user:${HOME}:/sbin/nologin" >> /etc/passwd\\n  fi\\nfi\\nexec "$@"\\n' \
    >> /opt/mssql-tools/bin/uid_entrypoint && chmod +x /opt/mssql-tools/bin/uid_entrypoint

ENV PATH=${PATH}:/opt/mssql/bin:/opt/mssql-tools/bin
RUN mkdir -p /var/opt/mssql/data && \
    chmod -R g=u /var/opt/mssql /etc/passwd

### Containers should not run as root as a good practice
USER 10001

# Default SQL Server TCP/Port
EXPOSE 1433

VOLUME /var/opt/mssql

### user name recognition at runtime w/ an arbitrary uid - for OpenShift deployments
ENTRYPOINT [ "uid_entrypoint" ]
# Run SQL Server process
CMD sqlservr