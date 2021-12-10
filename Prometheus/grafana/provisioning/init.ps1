# start Grafana and give it time to spin up
Start-Process grafana-server -NoNewWindow 
Start-Sleep 20

# create new user
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes('admin:admin'))
Invoke-RestMethod `
 -Method Post `
 -ContentType 'application/json' `
 -Headers @{Authorization="Basic $auth"} `
 -Body '{ "name":"viewer", "email":"viewer@org.com", "login":"viewer",  "password":"readonly" }' `
 -Uri https://inzfs-prom-grafana.london.cloudapps.digital/api/admin/users 

# set user's home dashboard     
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes('viewer:readonly'))
Invoke-RestMethod `
 -Method Put `
 -ContentType 'application/json' `
 -Headers @{Authorization="Basic $auth"} `
 -Body '{ "homeDashboardId":4 }' `
 -Uri https:/inzfs-prom-grafana.london.cloudapps.digital/api/user/preferences
