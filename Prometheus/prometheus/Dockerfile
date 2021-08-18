FROM prom/prometheus
WORKDIR /src
ADD prometheus/prometheus.yml /etc/prometheus/prometheus.yml
ADD prometheus/alert.yml /etc/prometheus/alert.yml
ADD prometheus/prometheus.rules.yml /etc/prometheus/prometheus.rules.yml

FROM prom/alertmanager
WORKDIR /src
ADD alertmanager/alertmanager.yml /etc/alertmanager/alertmanager.yml