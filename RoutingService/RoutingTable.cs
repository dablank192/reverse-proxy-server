using System;

namespace reverse_proxy_server.RoutingService;

public class RoutingTable
{
    public string PathPrefix {get; set;} = "";
    public string BackendHost {get; set;} = "";
    public string BackendPort {get; set;} = "";
}
