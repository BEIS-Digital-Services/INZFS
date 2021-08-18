FROM prom/prometheus
WORKDIR /src
ADD prometheus/prometheus.yml /etc/prometheus/prometheus.yml
ADD prometheus/alert.yml /etc/prometheus/alert.yml
ADD prometheus/prometheus.rules.yml /etc/prometheus/prometheus.rules.yml
ADD prometheus/alertmanager.yml /etc/alertmanager/alertmanager.yml