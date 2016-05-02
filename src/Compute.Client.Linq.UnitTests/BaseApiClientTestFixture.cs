using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DD.CBU.Compute.Api.Client;
using DD.CBU.Compute.Api.Client.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Compute.Client.Linq.UnitTests
{
    /// <summary>
    /// A base API client test fixture.
    /// </summary>
    public class BaseApiClientTestFixture
    {
		protected Guid accountId = new Guid("A3D9AACC-A273-45A5-919D-0F4C41C0763B");

		protected Dictionary<Uri, string> requestsAndResponses = new Dictionary<Uri, string>();

        protected void SetCustomerImageUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetMcp2CustomerImages(accountId).ToString() + query, UriKind.Relative), "ListCustomerImages.xml");
        }

        protected void SetNetworkDoaminUri(Guid id)
        {
            requestsAndResponses.Add(new Uri(ApiUris.NetworkDomain(accountId, id).ToString(), UriKind.Relative), "GetNetworkDomain.xml");
        }

        protected void SetNetworkDomainUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.NetworkDomains(accountId).ToString() + query, UriKind.Relative), "ListNetworkDomains.xml");
        }

        protected void SetFirewallUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetFirewallRules(accountId).ToString() + query, UriKind.Relative), "ListFirewallRules.xml");
        }

        protected void SetNatRuleUri(Guid networkDomainId, string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetDomainNatRules(accountId, networkDomainId.ToString()).ToString() + query, UriKind.Relative), "ListNatRules.xml");
        }

        protected void SetPoolMemberUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetPoolMembers(accountId).ToString() + query, UriKind.Relative), "ListPoolMembers.xml");
        }

        protected void SetServerUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetMcp2Servers(accountId).ToString() + query, UriKind.Relative), "ListServers.xml");
        }

        protected void SetVirtualIstenerUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetVirtualListeners(accountId).ToString() + query, UriKind.Relative), "ListVirtualListeners.xml");
        }

        protected void SetVlanUri(string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetVlanByOrgId(accountId).ToString() + query, UriKind.Relative), "ListVlans.xml");
        }

        protected void SetDefaultIrulesUri(Guid networkDomainId, string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetDefaultIrule(accountId, networkDomainId).ToString() + query, UriKind.Relative), "ListDefaultIrules.xml");
        }

        protected void SetDefaultPersistenceProfileUri(Guid networkDomainId, string query)
        {
            requestsAndResponses.Add(new Uri(ApiUris.GetDefaultPersistenceProfile(accountId, networkDomainId).ToString() + query, UriKind.Relative), "ListDefaultPersistenceProfiles.xml");
        }

        protected ComputeApiClient GetApiClient()
		{
            requestsAndResponses.Add(ApiUris.MyAccount, "GetMyAccountDetails.xml");

            var fakeClient = new Mock<IHttpClient>();
            var httpClient = fakeClient.Object;
            fakeClient.Setup(f => f.GetAsync(It.IsAny<Uri>())).Returns<Uri>(uri => GetResponse(uri));

            var client = new ComputeApiClient(httpClient);
            Task.Run(() => client.LoginAsync(new NetworkCredential(string.Empty, string.Empty))).Wait();
            return client;
        }

        private Task<HttpResponseMessage> GetResponse(Uri requesrUri)
        {
            if (!requestsAndResponses.ContainsKey(requesrUri))
            {
                Assert.Fail("No API response has been defined for URI " + requesrUri);
            }

            var response = requestsAndResponses[requesrUri];

            var sampleFolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\SampleOutputs");
            var targetFile = Path.Combine(sampleFolderLocation, response);
            var contents = File.ReadAllText(targetFile);

            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(contents, Encoding.UTF8, "text/xml"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/"))
            };

            return Task.FromResult(message);
        }
    }
}