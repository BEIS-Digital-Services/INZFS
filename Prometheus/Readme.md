alerts  = 

# http_requets times in seconds
(rate(response_time_bucket{app="inzfs-lab",le="0.5",status_range="2xx"}[24h])) > 0.1
histogram_quantile(0.95, (rate(response_time_bucket{app="inzfs-lab", status_range="2xx"}[24h]))) > 0.1

# HELP http_request_duration_seconds The duration of HTTP requests processed by an ASP.NET Core application.
# TYPE http_request_duration_seconds histogram
(rate(http_request_duration_seconds_bucket{code="200",method="GET",controller="",action="",le="1.024"}[24h])) > 1

requests

cpu percentage
cpu{organisation="beis-netzero"} > 75
sum(rate(cpu{app="inzfs-lab"}[5m])) > 75

memroy percentage
memory_utilization{organisation="beis-netzero"} > 75
sum(rate(memory_utilization{app="inzfs-lab"}[5m])) > 75

disk utilization
disk_utilization > 50

Need to build 2 using prometheus dotnet gauge

1 = page load times
2 = server response times
3 =  unsusaul amount of requests per min

fiz respnse times
histogram_quantile(0.95, sum(rate(response_time_bucket{app="inzfs-staging",status_range="2xx"}[30m])) by (le)) .1

under 95% or 0.5 milliseconds
(rate(response_time_bucket{le="0.5",status_range="2xx"}[1d]))cf