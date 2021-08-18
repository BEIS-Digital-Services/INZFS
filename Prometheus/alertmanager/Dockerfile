FROM prom/alertmanager

WORKDIR /go/src/github.com/prometheus/alertmanager
COPY    . /go/src/github.com/prometheus/alertmanager

RUN apt-get install make \
    && make build \
    && cp alertmanager /bin/ \
    && mkdir -p /etc/alertmanager/template \
    && mv ./alertmanager/config.yml /etc/alertmanager/config.yml \
    && rm -rf /go

