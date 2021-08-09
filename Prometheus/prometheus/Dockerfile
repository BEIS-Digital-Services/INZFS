FROM prom/prometheus
# Add in the configuration file from the local directory.
ADD prometheus.yml /etc/prometheus/prometheus.yml
docker build -t prometheus_eef .
docker run -p 9090:9090 prometheus_eef