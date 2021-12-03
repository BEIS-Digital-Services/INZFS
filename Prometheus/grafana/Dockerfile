FROM grafana/grafana
WORKDIR /src
ADD provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/prometheus_ds.yml
ADD provisioning/dashboards/ /etc/grafana/provisioning/dashboards/
ADD eef-dashboard/ /etc/grafana/dashboards/
ADD eef dashboard/ /var/lib/grafana/dashboards/
