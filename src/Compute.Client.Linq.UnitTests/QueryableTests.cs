using System;
using System.Linq;
using System.Threading.Tasks;
using DD.CBU.Compute.Api.Client.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Compute.Client.Linq.UnitTests
{
	[TestClass]
	public class QueryableTests : BaseApiClientTestFixture
	{
        [TestMethod]
        public async Task CustomerImages()
        {
            SetCustomerImageUri("?operatingSystemFamily=UNIX&operatingSystemId=123");

            var result = await GetApiClient().AsQueryable().CustomerImages()
                .Where(image => image.operatingSystem.family == "UNIX")
                .Where(image => image.operatingSystem.id == "123")
                .ToPagedResponseAsync();

            Assert.AreEqual(3, result.totalCount);
        }

        [TestMethod]
        public async Task Vlans()
        {
            SetVlanUri("?privateIpv4Address=0.0.0.0&ipv6Address=0.0.0.0&orderBy=networkDomainId.DESCENDING");

            var result = await GetApiClient().AsQueryable().Vlans()
                .Where(vlan => vlan.privateIpv4Range.address == "0.0.0.0")
                .Where(vlan => vlan.ipv6Range.address == "0.0.0.0")
                .OrderByDescending(vlan => vlan.networkDomain.id)
                .ToPagedResponseAsync();

            Assert.AreEqual(1, result.totalCount);
        }

        [TestMethod]
        public async Task FirewallRules()
        {
            var networkDomainId = Guid.NewGuid();
            SetFirewallUri("?networkDomainId=" + networkDomainId);

            var result = await GetApiClient().AsQueryable().FirewallRules(networkDomainId)
                .ToPagedResponseAsync();

            Assert.AreEqual(1, result.totalCount);
        }

        [TestMethod]
        public async Task NatRules()
        {
            var networkDomainId = Guid.NewGuid();
            SetNatRuleUri(networkDomainId, "&state=NORMAL&pageSize=10&pageNumber=3&orderBy=createTime");

            var result = await GetApiClient().AsQueryable().NatRules(networkDomainId)
                .Where(natRule => natRule.state == "NORMAL")
                .OrderBy(natRule => natRule.createTime)
                .Take(10)
                .Skip(20)
                .ToPagedResponseAsync();

            Assert.AreEqual(2, result.totalCount);
        }

        [TestMethod]
        public async Task PoolMembers()
        {
            SetPoolMemberUri("?nodeIp=10.10.10.10&nodeStatus=NORMAL&poolName=Test&orderBy=nodeId,poolId.DESCENDING");

            var result = await GetApiClient().AsQueryable().PoolMembers()
                .Where(poolMember => poolMember.node.ipAddress == "10.10.10.10")
                .Where(poolMember => poolMember.node.status == "NORMAL")
                .Where(poolMember => poolMember.pool.name == "Test")
                .OrderBy(poolMember => poolMember.node.id)
                .ThenByDescending(poolMember => poolMember.pool.id)
                .ToPagedResponseAsync();

            Assert.AreEqual(2, result.totalCount);
        }

        [TestMethod]
        public async Task Servers()
        {
            SetServerUri("?networkDomainId=123&vlanId=456&operatingSystemId=789&orderBy=privateIpv4,ipv6.DESCENDING");

            var result = await GetApiClient().AsQueryable().Servers()
                .Where(server => server.networkInfo.networkDomainId == "123")
                .Where(server => server.networkInfo.primaryNic.vlanId == "456")
                .Where(server => server.operatingSystem.id == "789")
                .OrderBy(server => server.nic.privateIpv4)
                .ThenByDescending(server => server.networkInfo.primaryNic.ipv6)
                .ToPagedResponseAsync();

            Assert.AreEqual(5, result.totalCount);
        }

        [TestMethod]
        public async Task VirtualListeners()
        {
            SetVirtualIstenerUri("?poolId=123&persistenceProfileId=456&orderBy=clientClonePoolId,fallbackPersistenceProfileId.DESCENDING");

            var result = await GetApiClient().AsQueryable().VirtualListeners()
                .Where(listener => listener.pool.id == "123")
                .Where(listener => listener.persistenceProfile.id == "456")
                .OrderBy(listener => listener.clientClonePool.id)
                .ThenByDescending(listener => listener.fallbackPersistenceProfile.id)
                .ToPagedResponseAsync();

            Assert.AreEqual(5, result.totalCount);
        }

        [TestMethod]
        public async Task DefaultIrules()
        {
            var networkDomainId = Guid.NewGuid();
            SetDefaultIrulesUri(networkDomainId, "&virtualListenerType=STANDARD&virtualListenerProtocol=HTTP&orderBy=name.DESCENDING,id");

            var result = await GetApiClient().AsQueryable().DefaultIrules(networkDomainId)
                .Where(rule => rule.virtualListenerCompatibility.Any(v => v.type == "STANDARD"))
                .Where(rule => rule.virtualListenerCompatibility.Any(v => v.protocol == "HTTP"))
                .OrderByDescending(rule => rule.irule.name)
                .ThenBy(rule => rule.irule.id)
                .ToPagedResponseAsync();

            Assert.AreEqual(4, result.totalCount);
        }

        [TestMethod]
        public async Task DefaultPersistenceProfiles()
        {
            var networkDomainId = Guid.NewGuid();
            SetDefaultPersistenceProfileUri(networkDomainId, "&virtualListenerType=STANDARD&virtualListenerProtocol=HTTP");

            var result = await GetApiClient().AsQueryable().DefaultPersistenceProfiles(networkDomainId)
                .Where(profile => profile.virtualListenerCompatibility.Any(v => v.type == "STANDARD"))
                .Where(profile => profile.virtualListenerCompatibility.Any(v => v.protocol == "HTTP"))
                .ToPagedResponseAsync();

            Assert.AreEqual(4, result.totalCount);
        }
    }
}
