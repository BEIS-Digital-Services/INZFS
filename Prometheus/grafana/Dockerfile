FROM grafana/grafana
WORKDIR /src
ADD provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/
ADD provisioning/dashboard-provider.yml /etc/grafana/provisioning/dashboards/
ADD provisioning/dashboards/ /var/lib/grafana/dashboards/
ADD provisioning/init.sh .
ls -l init.sh
chmod +x init.sh
ls -l init.sh
RUN ./init.sh