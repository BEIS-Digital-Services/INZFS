FROM prom/prometheus
FROM build as prometheus
WORKDIR /src
ADD prometheus/prometheus.yml /etc/prometheus/prometheus.yml
ADD prometheus/alert.yml /etc/prometheus/alert.yml
ADD prometheus/prometheus.rules.yml /etc/prometheus/prometheus.rules.yml
ADD prometheus/prometheus.rules.yml /etc/prometheus/prometheus.rules.yml

FROM grafana/grafana
FROM build as grafana
WORKDIR /app
LABEL grafana=true
ADD grafana/provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/prometheus_ds.yml