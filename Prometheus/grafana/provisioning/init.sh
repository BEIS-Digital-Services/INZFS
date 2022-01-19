#!/bin/bash

grafana-server \
 --homepath="$GF_PATHS_HOME" \
 --config="$GF_PATHS_CONFIG" \
 cfg:default.paths.data="$GF_PATHS_DATA" &
sleep 10 &&

curl \
 -XPOST \
 -H "Content-Type: application/json" \
 -d '{ "name":"viewer", "email":"viewer@org.com", "login":"viewer",  "password":"$(viewer_password)" }' \
 http://$(grafana_admin):$(grafana_password)@localhost:3000/api/admin/users 

curl \
 -X PUT \
 -H 'Content-Type: application/json' \
 -d '{ "homeDashboardId":4 }' \
 http://viewer:$(viewer_password)@localhost:3000/api/user/preferences
