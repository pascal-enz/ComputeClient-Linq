# LINQ Provider for Dimension Data Compute API Client

A C# client library which adds LINQ support to the Dimension Data Compute API Client Library.
https://github.com/DimensionDataCBUSydney/DimensionData.ComputeClient

#### Terms
This library does not form part of a service provided by Dimension Data. This utility is provided 'as-is' and Dimension Data cannot provide support for its usage.
**Important: This library is a inovation project only and is not actively developed or supported by the Compute API Client team.**

#### Limitations
* Only Cloud Control API 2.x resources are supported.
* The following fields cannot be queried using LINQ because they don't exist on the entity types.
    * Firewall Rule - *createTime*
    * NAT Rule - *nodeId*
    * NIC - *securityGroup* and *securityGroupId*

## Usage 

##### Getting a Queryable Collection
    using DD.CBU.Compute.Api.Client.Linq;
    
    var client = ComputeApiClient.GetComputeApiClient(...);
    var servers = client.AsQueryable().Servers();

##### Filtering a Collection
    var servers = client.AsQueryable().Servers()
        .Where(server => server.name.StartsWith("A") || server.name.EndsWith("B"))
        .Where(server => server.networkInfo.primaryNic.vlanId == new Guid("fb9f4940-4279-4164-a25d-62835bc20b62"))
        .Where(server => server.createTime > DateTime.Now.AddYears(-1))
        .ToList();

The following Cloud Control API request will be performed:  
/server?name.LIKE=A*&name.LIKE=*B&vlanId=fb9f4940-4279-4164-a25d-62835bc20b62&createTime.GT=2015-05-01T00:00:00Z

##### Sorting a Collection
    var servers = GetApiClient().AsQueryable().Servers()
        .OrderBy(server => server.operatingSystem.family)
        .ThenByDescending(server => server.name)
        .ToList();

The following Cloud Control API request will be performed:  
/server?orderBy=operatingSystemFamily,name.DESCENDING

##### Paging through a Collection
    var pageSize = 10;
    var pageNumber = 3;
    
    var networkDomains = GetApiClient().AsQueryable().NetworkDomains()
        .Take(pageSize)
        .Skip((pageNumber - 1) * pageSize)
        .ToPagedResponse();
        
The following Cloud Control API request will be performed:  
/networkDomain?pageSize=10&pageNumber=3

The number used for skip must be a multiple of the number used for take or zero (default).

Notice you can use the extension methods *ToPagedResponse()* and *ToPagedResponseAsync()* instead of *ToList()* or *ToArray()* in order to retrieve the results with additional paging information like total count.

##### Getting the Item Count only
    var count = GetApiClient().AsQueryable().Vlans()
        .Where(vlan => vlan.state == "REQUIRES_SUPPORT")
        .Count();

Since Cloud Control does not actually support "count only" queries, the following Cloud Control API request will be performed as a workaround:  
/vlan?state=REQUIRES_SUPPORT&pageSize=1&pageNumber=1000000

##### Getting a single Item
    var server = GetApiClient().AsQueryable().Servers()
        .Where(server => server.id == new Guid("fb9f4940-4279-4164-a25d-62835bc20b62"))
        .Single();

The following Cloud Control API request will be performed:  
/server/fb9f4940-4279-4164-a25d-62835bc20b62

Note that this will throw an exception (404 response) if the asset doesn't exist. Use *SingleOrDefault()* instead to perform the folloiwng Cloud Control API request which will not cause an error:  
/server?id=fb9f4940-4279-4164-a25d-62835bc20b62&pageSize=1&pageNumber=1

##### Using LINQ functions that are not supported by Cloud Control
Some LINQ functions like *Select*, *GroupBy* or *Aggregate* are not supported by Cloud Control. However, the Compute API LINQ Provider can perform all unsupported LINQ functions as client-side in-memory operations after the supported functions have been processed by Cloud Control. **It is important to remember that when you write LINQ queries that mix supported and unsupported LINQ functions, the supported ones must come first.** Also note that *ToPagedResponse()* cannot be used anymore after applying unsupported LINQ functions.

    var server = GetApiClient().AsQueryable().Servers()
        .Where(server => server.id == new Guid("fb9f4940-4279-4164-a25d-62835bc20b62"))
        .Select(n => new { n.name, n.state })
        .Single();

The following Cloud Control API request will be performed before the *Select* statememt is applied client-side:  
/server/fb9f4940-4279-4164-a25d-62835bc20b62
