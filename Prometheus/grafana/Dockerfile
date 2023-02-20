FROM grafana/grafana:9.4.0-beta1-ubuntu
WORKDIR /src
ADD provisioning/datasources/prometheus_ds.yml /etc/grafana/provisioning/datasources/prometheus_ds.yml
# ADD provisioning/dashboard-provider.yml /etc/grafana/provisioning/dashboards/
ADD provisioning/dashboards/ /var/lib/grafana/dashboards/
ADD provisioning/dashboards/ /etc/grafana/provisioning/dashboards/
ADD eef-dashboard/ /etc/grafana/dashboards/
ADD eef-dashboard/ /var/lib/grafana/dashboards/

ADD provisioning/init.sh .
USER root
RUN chmod +x init.sh
RUN apk add --update curl
RUN curl --version
RUN ./init.sh
