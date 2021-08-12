$ docker run --name alertmanager -d -p 127.0.0.1:9093:9093 quay.io/prometheus/alertmanager



cf map-route inzfs-lab2-redirect london.cloudapps.digital --hostname inzfs-lab2 --port 443
cf map-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2-redirect --port 443
cf unmap-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2 --port 443

cf map-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2 --port 443
cf unmap-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2-redirect --port 443
