using System;
using System.Linq;
using System.Threading.Tasks;
using DD.CBU.Compute.Api.Client;
using DD.CBU.Compute.Api.Client.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Compute.Client.Linq.UnitTests
{
	[TestClass]
	public class ComputeApiQueryProviderTests : BaseApiClientTestFixture
	{
        [TestMethod]
        public async Task Paging_WithoutSkip()
        {
            SetNetworkDomainUri("?pageSize=10&pageNumber=1");

            var result = await GetApiClient().AsQueryable().NetworkDomains()
                .Take(10)
                .ToPagedResponseAsync();

            Assert.AreEqual(2, result.totalCount);
        }

        [TestMethod]
        public async Task Paging_WithSkip()
        {
            SetNetworkDomainUri("?pageSize=10&pageNumber=3");

            var pageNumber = 3;
            var pageSize = 10;
            var result = await GetApiClient().AsQueryable().NetworkDomains()
                .Take(pageSize)
                .Skip((pageNumber - 1) * pageSize)
                .ToPagedResponseAsync();

            Assert.AreEqual(2, result.totalCount);
        }

        [TestMethod]
        public async Task Ordering()
        {
            SetNetworkDomainUri("?orderBy=state,datacenterId,name.DESCENDING");

            var result = await GetApiClient().AsQueryable().NetworkDomains()
                .OrderBy(n => n.state)
                .ThenBy(n => n.datacenterId)
                .ThenByDescending(n => n.name)
                .ToPagedResponseAsync();

            Assert.AreEqual(2, result.totalCount);
        }

        [TestMethod]
        public void Filtering_Equals()
        {
            SetNetworkDomainUri("?datacenterId=AU1&datacenterId=AU2");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.datacenterId == "AU1" || n.datacenterId == "AU2")
                .ToList();
        }

        [TestMethod]
        public void Filtering_GreaterThan()
        {
            SetNetworkDomainUri("?createTime.GT=2015-12-31T00%3a00%3a00Z");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.createTime > new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc))
                .ToList();
        }

        [TestMethod]
        public void Filtering_LessOrEqual()
        {
            SetNetworkDomainUri("?createTime.LE=2015-12-31T00%3a00%3a00Z");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.createTime <= new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Utc))
                .ToList();
        }

        [TestMethod]
        public void Filtering_StartsWith()
        {
            SetNetworkDomainUri("?name.LIKE=Test*");

            var value = "Test";
            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.name.StartsWith(value.ToString()))
                .ToList();
        }

        [TestMethod]
        public void Filtering_EndsWith()
        {
            SetNetworkDomainUri("?name.LIKE=*Test");

            var value = "Test";
            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.name.EndsWith(value.ToString()))
                .ToList();
        }

        [TestMethod]
        public void Filtering_Contains()
        {
            SetNetworkDomainUri("?name.LIKE=*Test*");

            var value = "Test";
            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.name.Contains(value.ToString()))
                .ToList();
        }

        [TestMethod]
        public void Filtering_Null()
        {
            SetNetworkDomainUri("?name.NULL");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.name == null)
                .ToList();
        }

        [TestMethod]
        public void Filtering_NotNull()
        {
            SetNetworkDomainUri("?name.NOT_NULL");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.name != null)
                .ToList();
        }

        [TestMethod]
        public void Filtering_Any()
        {
            var networkDomainId = Guid.NewGuid();
            SetDefaultPersistenceProfileUri(networkDomainId, "&virtualListenerType=STANDARD");

            var result = GetApiClient().AsQueryable().DefaultPersistenceProfiles(networkDomainId)
                .Where(n => n.virtualListenerCompatibility.Any(v => v.type == "STANDARD"))
                .ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void Count()
        {
            SetNetworkDomainUri("?state=NORMAL&pageSize=1&pageNumber=1000000");

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.state == "NORMAL")
                .Count();

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void SelectAndSingle()
        {
            var id = Guid.NewGuid();
            SetNetworkDoaminUri(id);

            var result = GetApiClient().AsQueryable().NetworkDomains()
                .Where(n => n.id == id.ToString())
                .Select(n => n.name)
                .Single();

            Assert.AreEqual("Development Network Domain", result);
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            var id = Guid.NewGuid();
            SetVlanUri("?id=" + id.ToString() + "&pageSize=1&pageNumber=1");

            var result = GetApiClient().AsQueryable().Vlans()
                .Where(n => n.id == id.ToString())
                .Select(n => n.name)
                .FirstOrDefault();

            Assert.IsNotNull(result);
        }
    }
}
