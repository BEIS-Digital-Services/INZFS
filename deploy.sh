#!/bin/sh
export password=$(dgpassword)

///wget -q -O - https://packages.cloudfoundry.org/debian/cli.cloudfoundry.org.key | sudo apt-key add -
//echo "deb https://packages.cloudfoundry.org/debian stable main" | sudo tee /etc/apt/sources.list.d/cloudfoundry-cli.list

//sudo apt-get update

//sudo apt-get install cf7-cli

cf login -a api.london.cloud.service.gov.uk -u 'david.gardiner@beis.gov.uk' -p $dgpassword -o beis-netzero -s sandbox

CF_DOCKER_PASSWORD=$(dockerRegistryPassword) cf push inzfs-sandbox -i 1 -m 512M --docker-image netzeroregistry.azurecr.io/inzfs:sandbox --docker-username netzeroregistry --no-route --no-manifest
// CF_DOCKER_PASSWORD=$(dockerRegistryPassword) cf push $(dockerAppName) --docker-image $(dockerRegistryUrl)/$(dockerRepository):$(dockerImageTag) --docker-username $(dockerRegistryUsername)

cf map-route inzfs-sandbox london.cloudapps.digital --hostname inzfs-sandbox