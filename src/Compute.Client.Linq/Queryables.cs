using System;
using System.Linq;

using DD.CBU.Compute.Api.Client.Interfaces;
using DD.CBU.Compute.Api.Contracts.Network20;
using DD.CBU.Compute.Api.Contracts.Requests.Infrastructure;
using DD.CBU.Compute.Api.Contracts.Requests.Network20;
using DD.CBU.Compute.Api.Contracts.Requests.Server20;
using DD.CBU.Compute.Api.Contracts.Requests.Tagging;

namespace DD.CBU.Compute.Api.Client.Linq
{
    using Internal;

    /// <summary>
    /// Provides access to queryable asset collections.
    /// </summary>
    public class Queryables
    {
        /// <summary>
        /// The Compute API client instance.
        /// </summary>
        private readonly IComputeApiClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Queryables"/> class.
        /// </summary>
        /// <param name="client">The Compute API client instance.</param>
        public Queryables(IComputeApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _client = client;
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="DatacenterType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<DatacenterType> Datacenters()
        {
            return new ComputeApiQueryBuilder<DataCenterListOptions, DatacenterType, string>()
                .OnQuery((filterable, pageable) => _client.Infrastructure.GetDataCentersPaginated(pageable, filterable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="OperatingSystemType"/>.
        /// </summary>
        /// <param name="datacenterId">The datacenter id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<OperatingSystemType> OperatingSystems(string datacenterId)
        {
            return new ComputeApiQueryBuilder<OperatingSystemListOptions, OperatingSystemType, Guid>()
                .OnQuery((filterable, pageable) => _client.Infrastructure.GetOperatingSystems(datacenterId, pageable, filterable))
                .MapParameter("displayName", "name")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="NetworkDomainType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<NetworkDomainType> NetworkDomains()
        {
            return new ComputeApiQueryBuilder<NetworkDomainListOptions, NetworkDomainType, Guid>()
                .OnGet(id => _client.Networking.NetworkDomain.GetNetworkDomain(id))
                .OnQuery((filterable, pageable) => _client.Networking.NetworkDomain.GetNetworkDomainsPaginated(filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="VlanType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<VlanType> Vlans()
        {
            return new ComputeApiQueryBuilder<VlanListOptions, VlanType, Guid>()
                .OnGet(id => _client.Networking.Vlan.GetVlan(id))
                .OnQuery((filterable, pageable) => _client.Networking.Vlan.GetVlansPaginated(filterable, pageable))
                .MapParameter("networkDomain.id", "networkDomainId")
                .MapParameter("privateIpv4Range.address", "privateIpv4Address")
                .MapParameter("ipv6Range.address", "ipv6Address")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="PublicIpBlockType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<PublicIpBlockType> PublicIpBlocks(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<PublicIpListOptions, PublicIpBlockType, Guid>()
                .OnGet(id => _client.Networking.IpAddress.GetPublicIpBlock(id))
                .OnQuery((filterable, pageable) => _client.Networking.IpAddress.GetPublicIpBlocksPaginated(networkDomainId, pageable, filterable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="ReservedPublicIpv4AddressType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<ReservedPublicIpv4AddressType> ReservedPublicIpv4Addresses(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<ReservedPublicIpv4ListOptions, ReservedPublicIpv4AddressType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.IpAddress.GetReservedPublicAddressesForNetworkDomainPaginated(networkDomainId, pageable, filterable))
                .MapParameter("value", "ipAddress")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="ReservedPrivateIpv4AddressType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<ReservedPrivateIpv4AddressType> ReservedPrivateIpv4Addresses()
        {
            return new ComputeApiQueryBuilder<ReservedPrivateIpv4ListOptions, ReservedPrivateIpv4AddressType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.IpAddress.GetReservedPrivateIpv4AddressesPaginated(filterable, pageable))
                .MapParameter("value", "ipAddress")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="ReservedIpv6AddressType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<ReservedIpv6AddressType> ReservedIpv6Addresses()
        {
            return new ComputeApiQueryBuilder<ReservedIpv6ListOptions, ReservedIpv6AddressType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.IpAddress.GetReservedIpv6AddressesPaginated(filterable, pageable))
                .MapParameter("value", "ipAddress")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="FirewallRuleType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<FirewallRuleType> FirewallRules(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<FirewallRuleListOptions, FirewallRuleType, Guid>()
                .OnGet(id => _client.Networking.FirewallRule.GetFirewallRule(id))
                .OnQuery((filterable, pageable) => _client.Networking.FirewallRule.GetFirewallRulesPaginated(filterable, pageable))
                .Filter("networkDomainId", networkDomainId)
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="IpAddressListType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<IpAddressListType> IpAddressLists(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<IpAddressListOptions, IpAddressListType, Guid>()
                .OnGet(id => _client.Networking.FirewallRule.GetIpAddressList(id))
                .OnQuery((filterable, pageable) => _client.Networking.FirewallRule.GetIpAddressListsPaginated(networkDomainId, filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="PortListType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<PortListType> PortLists(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<PortListOptions, PortListType, Guid>()
                .OnGet(id => _client.Networking.FirewallRule.GetPortList(id))
                .OnQuery((filterable, pageable) => _client.Networking.FirewallRule.GetPortListsPaginated(networkDomainId, filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="NatRuleType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<NatRuleType> NatRules(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<NatRuleListOptions, NatRuleType, Guid>()
                .OnGet(id => _client.Networking.Nat.GetNatRule(id))
                .OnQuery((filterable, pageable) => _client.Networking.Nat.GetNatRulesPaginated(networkDomainId, filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="NodeType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<NodeType> Nodes()
        {
            return new ComputeApiQueryBuilder<NodeListOptions, NodeType, Guid>()
                .OnGet(id => _client.Networking.VipNode.GetNode(id))
                .OnQuery((filterable, pageable) => _client.Networking.VipNode.GetNodesPaginated(filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="PoolType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<PoolType> Pools()
        {
            return new ComputeApiQueryBuilder<PoolListOptions, PoolType, Guid>()
                .OnGet(id => _client.Networking.VipPool.GetPool(id))
                .OnQuery((filterable, pageable) => _client.Networking.VipPool.GetPoolsPaginated(filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="PoolMemberType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<PoolMemberType> PoolMembers()
        {
            return new ComputeApiQueryBuilder<PoolMemberListOptions, PoolMemberType, Guid>()
                .OnGet(id => _client.Networking.VipPool.GetPoolMember(id))
                .OnQuery((filterable, pageable) => _client.Networking.VipPool.GetPoolMembersPaginated(filterable, pageable))
                .MapParameter("pool.id", "poolId")
                .MapParameter("pool.name", "poolName")
                .MapParameter("node.id", "nodeId")
                .MapParameter("node.name", "nodeName")
                .MapParameter("node.ipAddress", "nodeIp")
                .MapParameter("node.status", "nodeStatus")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="VirtualListenerType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<VirtualListenerType> VirtualListeners()
        {
            return new ComputeApiQueryBuilder<VirtualListenerListOptions, VirtualListenerType, Guid>()
                .OnGet(id => _client.Networking.VipVirtualListener.GetVirtualListener(id))
                .OnQuery((filterable, pageable) => _client.Networking.VipVirtualListener.GetVirtualListenersPaginated(filterable, pageable))
                .MapParameter("pool.id", "poolId")
                .MapParameter("clientClonePool.id", "clientClonePoolId")
                .MapParameter("persistenceProfile.id", "persistenceProfileId")
                .MapParameter("fallbackPersistenceProfile.id", "fallbackPersistenceProfileId")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="DefaultHealthMonitorType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<DefaultHealthMonitorType> DefaultHealthMonitors(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<DefaultHealthMonitorListOptions, DefaultHealthMonitorType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.VipSupport.GetDefaultHealthMonitorsPaginated(networkDomainId, filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="DefaultPersistenceProfileType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<DefaultPersistenceProfileType> DefaultPersistenceProfiles(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<DefaultPersistenceProfileListOptions, DefaultPersistenceProfileType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.VipSupport.GetDefaultPersistenceProfilesPaginated(networkDomainId, filterable, pageable))
                .MapParameter("virtualListenerCompatibility.type", "virtualListenerType")
                .MapParameter("virtualListenerCompatibility.protocol", "virtualListenerProtocol")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="DefaultIruleType"/>.
        /// </summary>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<DefaultIruleType> DefaultIrules(Guid networkDomainId)
        {
            return new ComputeApiQueryBuilder<DefaultIruleListOptions, DefaultIruleType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.VipSupport.GetDefaultIrulesPaginated(networkDomainId, filterable, pageable))
                .MapParameter("irule.id", "id")
                .MapParameter("irule.name", "name")
                .MapParameter("virtualListenerCompatibility.type", "virtualListenerType")
                .MapParameter("virtualListenerCompatibility.protocol", "virtualListenerProtocol")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="SecurityGroupType"/>.
        /// </summary>
        /// <param name="vlanId">The VLAN id.</param>
        /// <param name="serverId">The server id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<SecurityGroupType> SecurityGroups(Guid? vlanId, Guid? serverId)
        {
            if (!vlanId.HasValue && !serverId.HasValue)
                throw new ArgumentException("Either vlanId or serverId must be specified.");

            var builder = new ComputeApiQueryBuilder<SecurityGroupListOptions, SecurityGroupType, Guid>()
                .OnQuery((filterable, pageable) => _client.Networking.SecurityGroup.GetSecurityGroupsPaged(vlanId, serverId, pageable, filterable));

            if (vlanId.HasValue)
                builder.Filter("vlanId", vlanId.Value);

            if (serverId.HasValue)
                builder.Filter("serverId", serverId.Value);

            return builder.Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="ServerType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<ServerType> Servers()
        {
            return new ComputeApiQueryBuilder<ServerListOptions, ServerType, Guid>()
                .OnGet(id => _client.ServerManagement.Server.GetServer(id))
                .OnQuery((filterable, pageable) => _client.ServerManagement.Server.GetServersPaginated(filterable, pageable))
                .MapParameter("nic.networkId", "networkId")
                .MapParameter("nic.privateIpv4", "privateIpv4")
                .MapParameter("networkInfo.networkDomainId", "networkDomainId")
                .MapParameter("networkInfo.primaryNic.vlanId", "vlanId")
                .MapParameter("networkInfo.primaryNic.privateIpv4", "privateIpv4")
                .MapParameter("networkInfo.primaryNic.ipv6", "ipv6")
                .MapParameter("operatingSystem.id", "operatingSystemId")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="NicWithSecurityGroupType"/>.
        /// </summary>
        /// <param name="vlanId">The VLAN id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<NicWithSecurityGroupType> Nics(Guid vlanId)
        {
            return new ComputeApiQueryBuilder<ListNicsOptions, NicWithSecurityGroupType, Guid>()
                .OnQuery((filterable, pageable) => _client.ServerManagement.Server.ListNics(vlanId, filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="AntiAffinityRuleType"/>.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <param name="networkDomainId">The network domain id.</param>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<AntiAffinityRuleType> AntiAffinityRules(Guid? serverId, Guid? networkDomainId)
        {
            if (!serverId.HasValue && !networkDomainId.HasValue)
                throw new ArgumentException("Either serverId or networkDomainId must be specified.");

            var builder = new ComputeApiQueryBuilder<AntiAffinityRuleListOptions, AntiAffinityRuleType, Guid>()
                .OnQuery((filterable, pageable) => _client.ServerManagement.AntiAffinityRule.GetAntiAffinityRulesPaginated(filterable, pageable));

            if (serverId.HasValue)
                builder.Filter("serverId", serverId.Value);

            if (networkDomainId.HasValue)
                builder.Filter("networkDomainId", networkDomainId.Value);

            return builder.Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="OsImageType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<OsImageType> OsImages()
        {
            return new ComputeApiQueryBuilder<ServerOsImageListOptions, OsImageType, Guid>()
                .OnGet(id => _client.ServerManagement.ServerImage.GetOsImage(id))
                .OnQuery((filterable, pageable) => _client.ServerManagement.ServerImage.GetOsImages(filterable, pageable))
                .MapParameter("operatingSystem.id", "operatingSystemId")
                .MapParameter("operatingSystem.family", "operatingSystemFamily")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="CustomerImageType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<CustomerImageType> CustomerImages()
        {
            return new ComputeApiQueryBuilder<ServerCustomerImageListOptions, CustomerImageType, Guid>()
                .OnGet(id => _client.ServerManagement.ServerImage.GetCustomerImage(id))
                .OnQuery((filterable, pageable) => _client.ServerManagement.ServerImage.GetCustomerImages(filterable, pageable))
                .MapParameter("operatingSystem.id", "operatingSystemId")
                .MapParameter("operatingSystem.family", "operatingSystemFamily")
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="TagKeyType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<TagKeyType> TagKeys()
        {
            return new ComputeApiQueryBuilder<TagKeyListOptions, TagKeyType, Guid>()
                .OnGet(id => _client.Tagging.GetTagKey(id))
                .OnQuery((filterable, pageable) => _client.Tagging.GetTagKeysPaginated(filterable, pageable))
                .Build();
        }

        /// <summary>
        /// Cretaes an IQueryable instance for <see cref="TagType"/>.
        /// </summary>
        /// <returns>The IQueryable instance</returns>
        public IQueryable<TagType> Tags()
        {
            return new ComputeApiQueryBuilder<TagListOptions, TagType, Guid>()
                .OnQuery((filterable, pageable) => _client.Tagging.GetTagsPaginated(filterable, pageable))
                .Build();
        }
    }
}
