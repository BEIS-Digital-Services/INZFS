$ docker run --name alertmanager -d -p 127.0.0.1:9093:9093 quay.io/prometheus/alertmanager



cf map-route inzfs-lab2-redirect london.cloudapps.digital --hostname inzfs-lab2 --port 443
cf map-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2-redirect --port 443
cf unmap-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2 --port 443

cf map-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2 --port 443
cf unmap-route inzfs-lab2 london.cloudapps.digital --hostname inzfs-lab2-redirect --port 443
cf unmap-route inzfs-lab2-redirect london.cloudapps.digital --hostname inzfs-lab2 --port 443

cf create-user-provided-service logit-ssl-drain -l syslog-tls://ad33fec8-38c6-4b54-b2ff-710c8256ddf0-ls.logit.io:12138

cf bind-service inzfs-lab logit-ssl-drain
cf bind-service inzfs-staging logit-ssl-drain
cf bind-service inzfs-uat logit-ssl-drain

cf restage inzfs-lab
cf restage inzfs-staging
cf restage inzfs-uat
