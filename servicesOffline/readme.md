https://inzfs-prod-redirect.london.cloudapps.digital/servicesOffline.html

cf map-route inzfs-prod-redirect london.cloudapps.digital/servicesOffline.html --hostname inzfs-prod --port 443
cf map-route inzfs-prod london.cloudapps.digital --hostname inzfs-prod-redirect --port 443
cf unmap-route inzfs-prod london.cloudapps.digital --hostname inzfs-prod --port 443

..
cf map-route inzfs-prod london.cloudapps.digital --hostname inzfs-prod --port 443
cf unmap-route inzfs-prod london.cloudapps.digital --hostname inzfs-prod-redirect --port 443