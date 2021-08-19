FROM grafana/grafana
WORKDIR /src
ADD provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/prometheus_ds.yml
ADD provisioning/dashboards/ /etc/grafana/provisioning/dashboards/
ADD grafana/eef-dashboard/ eef-/var/lib/grafana/dashboards
