# INZFS
cf inzfs-docker-lab \
initial_app_domain \
--hostname inzfs-docker-lab \
[--port 443 ]

cf add-network-policy inzfs-docker-lab --destination-app inzfs-docker-lab --protocol tcp --port 443

<PropertyGroup>
  <DockerfileRunArguments>-p 8883:8883</DockerfileRunArguments>
</PropertyGroup>