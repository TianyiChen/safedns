This file aims at providing reliable DNS resolve results and sharing them.

I got this idea from DNSCrypt form opendns. But I failed to share the result in my home network. So I changed the DummyDNServer and want it to be a part of safe DNS service.

Using this tool will start a DNS service and the source was provided via https connection which is much safer than traditional DNS connection.
You can use this tool to enhance the security of DNS when you are using a public network(For instance, a public WIFI) and under other circumstances (For example, if your ISP hijacks your DNS queries)

The only several things you need to assure are:
1.Resolve the domain name of the safe DNS server first and save it in your host, or the service won't be available.
2.The connection via port 443 is not blocked.
3.The port 53 of the computer is not used.
