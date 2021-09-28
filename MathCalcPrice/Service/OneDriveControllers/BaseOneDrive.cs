using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MathCalcPrice.Service.OneDriveControllers
{
    abstract public class BaseOneDrive
    {
        private static string Instance = "https://login.microsoftonline.com/";
        private static string ClientId = "f4f7d338-486e-490f-a87e-298f413f8942";
        private static string TenantId = "8a648ae3-f42e-4858-b848-ef62d3422f6d";
        private static string access_token { get; set; }

        public GraphServiceClient SingAndReturnMe(string userName = "n.ognev@bimprogress.team", string password = "Gfgekz2002")
        {
            string authority = string.Concat(Instance, TenantId);
            string resource = "https://graph.microsoft.com";
            try
            {
                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(userName, password);
                AuthenticationContext authContext = new AuthenticationContext(authority);
                var result = authContext.AcquireTokenAsync(resource, ClientId, userPasswordCredential).Result;
                var graphserviceClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            access_token = authContext.AcquireTokenSilentAsync(resource, ClientId).Result.AccessToken;
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", access_token);
                            return Task.FromResult(0);
                        }));
                return graphserviceClient;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
            return null;
        }
    }
}
