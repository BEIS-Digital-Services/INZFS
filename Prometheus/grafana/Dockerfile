FROM grafana/grafana
WORKDIR /src
ADD provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/
ADD provisioning/dashboard-provider.yml /etc/grafana/provisioning/dashboards/
ADD provisioning/dashboards/ /var/lib/grafana/dashboards/
ADD provisioning/init.sh .
USER root
RUN chmod +x init.sh
RUN apt install curl
RUN curl --version
RUN ./init.sh
