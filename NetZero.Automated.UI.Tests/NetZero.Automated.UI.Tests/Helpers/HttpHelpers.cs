using NetZero.Automated.UI.Tests.TestData;
using NetZero.Automated.UI.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NetZero.Automated.UI.Tests.Helpers
{
    public class HttpHelpers
    {
        public static string AdminAccessCookie;
        public static string RequestToken;
        private static string AntiforgeryToken;
        private string GetAuthCookieForLogin(string username, string password)
        {
            string cookie = string.Empty;
            var accessKeys = GetRequestVerificationAndAntiForgeryToken(ConfigurationSetUp.BaseUrl + "/Login");
            Uri postUrl = new Uri(ConfigurationSetUp.BaseUrl + "/INZFS.Theme/Account/Login");
            using (var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false })
            using (var client = new HttpClient(handler) { BaseAddress = postUrl })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", accessKeys["RequestToken"]),
                    new KeyValuePair<string, string>("UserName", username),
                    new KeyValuePair<string, string>("Password", password)
                });
                var message = new HttpRequestMessage(HttpMethod.Post, postUrl) { Content = content };
                message.Content.Headers.Add("content-length", "220");
                message.Headers.Add("cookie", accessKeys["Cookie"]);
                var result = client.SendAsync(message).Result;
                cookie = result.Headers.GetValues("set-cookie").ToList().FirstOrDefault(c=>c.StartsWith("orchauth_Default")).Split(';').First();
            }
            return cookie;
        }

        private string GetAuthCookieAfterRegistration(string username, string password)
        {
            string cookie = string.Empty;
            var accessKeys = GetRequestVerificationAndAntiForgeryToken(ConfigurationSetUp.BaseUrl + "/Register");
            Uri postUrl = new Uri(ConfigurationSetUp.BaseUrl + "/Register");
            using (var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false })
            using (var client = new HttpClient(handler) { BaseAddress = postUrl })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", accessKeys["RequestToken"]),
                    new KeyValuePair<string, string>("UserName", username),
                    new KeyValuePair<string, string>("Email", username+"@test.com"),
                    new KeyValuePair<string, string>("Password", password),
                    new KeyValuePair<string, string>("ConfirmPassword", password)
                });
                var message = new HttpRequestMessage(HttpMethod.Post, postUrl) { Content = content };
                message.Content.Headers.Add("content-length", "269");
                message.Headers.Add("cookie", accessKeys["Cookie"]);
                var result = client.SendAsync(message).Result;
                cookie = result.Headers.GetValues("set-cookie").ToList().FirstOrDefault(c => c.StartsWith("orchauth_Default")).Split(';').First();
            }
            return cookie;
        }

        private void PostCompanyDetails(string authCookie)
        {
            var accessKeys = GetRequestVerificationAndAntiForgeryToken(ConfigurationSetUp.BaseUrl + "/FundApplication/section/company-details", authCookie);
            Uri postUrl = new Uri(ConfigurationSetUp.BaseUrl + "/FundApplication/Create");
            using (var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false })
            using (var client = new HttpClient(handler) { BaseAddress = postUrl })
            {
                HttpContent contentType = new StringContent("contentType"); 
                HttpContent companyName = new StringContent("CompanyDetailsPart.CompanyName");
                HttpContent companyNumber = new StringContent("CompanyDetailsPart.CompanyNumber"); 
                HttpContent submitPublish = new StringContent("submit.Publish");
                HttpContent requestToken = new StringContent("__RequestVerificationToken");

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(contentType, "CompanyDetails");
                    formData.Add(companyName, "test auto comp");
                    formData.Add(companyNumber, "25875642");
                    formData.Add(submitPublish, "submit.Publish");
                    formData.Add(requestToken, accessKeys["RequestToken"]);
                    var message = new HttpRequestMessage(HttpMethod.Post, postUrl) { Content = formData };
                    message.Content.Headers.Add("content-length", "830");
                    message.Content.Headers.Add("ContentType", "multipart/form-data; boundary=----WebKitFormBoundaryrMGQy86BOLz5h3R4 ");
                    message.Headers.Add("cookie", authCookie);
                    var result = client.SendAsync(message).Result;
                }
                
            }
        }

        internal void TestHttpHelpers()
        {

            var authCookie = GetAuthCookieAfterRegistration("Test9", "Password123");
            PostCompanyDetails(authCookie);

        }

        private Dictionary<string, string> GetRequestVerificationAndAntiForgeryToken(string url)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    List<string> cookies = response.Headers.GetValues("set-cookie").ToList();
                    AntiforgeryToken = cookies.FirstOrDefault(c => c.StartsWith("orchantiforgery_Default")).Split(';').First();
                    dict.Add($"Cookie", AntiforgeryToken);
                    if (response.EnsureSuccessStatusCode().StatusCode.ToString().ToLower() == "ok")
                    {
                        string responseBody = string.Empty;
                        using (HttpContent content = response.Content)
                        {
                            var responseStr = content.ReadAsStringAsync();
                            responseBody = responseStr.Result.Replace(" ", string.Empty);
                        }
                        int strFrom = responseBody.IndexOf("name=\"__RequestVerificationToken\"type=\"hidden\"value=\"") + "name=\"__RequestVerificationToken\"type=\"hidden\"value=\"".Length;
                        int strTo = responseBody.LastIndexOf("\"/></form>");
                        RequestToken = responseBody.Substring(strFrom, strTo - strFrom);
                        dict.Add("RequestToken", RequestToken);
                    }
                }
                catch (HttpRequestException ex)
                {
                    
                }
            }
            return dict;
        }

        private Dictionary<string, string> GetRequestVerificationAndAntiForgeryToken(string url, string authCookie)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var message = new HttpRequestMessage(HttpMethod.Get, url);
                    message.Headers.Add("cookie", authCookie);
                    var result = client.SendAsync(message).Result;
                    dict.Add($"Cookie", "");
                    if (result.EnsureSuccessStatusCode().StatusCode.ToString().ToLower() == "ok")
                    {
                        string responseBody = string.Empty;
                        using (HttpContent content = result.Content)
                        {
                            var responseStr = content.ReadAsStringAsync();
                            responseBody = responseStr.Result.Replace(" ", string.Empty);
                        }
                        int strFrom = responseBody.IndexOf("name=\"__RequestVerificationToken\"type=\"hidden\"value=\"") + "name=\"__RequestVerificationToken\"type=\"hidden\"value=\"".Length;
                        int strTo = responseBody.LastIndexOf("\"/></form>");
                        RequestToken = responseBody.Substring(strFrom, strTo - strFrom);
                        dict.Add("RequestToken", RequestToken);
                    }
                }
                catch (HttpRequestException ex)
                {

                }
            }
            return dict;
        }
        public void DeleteTestUser(string url)
        {
            Uri baseAddress = new Uri(url);
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", RequestToken)
                });
                var message = new HttpRequestMessage(HttpMethod.Post, baseAddress) { Content = content };
                message.Content.Headers.Add("content-length", "225");
                message.Headers.Add("cookie", $"{AntiforgeryToken}; {GetAuthCookieForLogin(AdminUser.Username, AdminUser.Password)}");
                var result = client.SendAsync(message).Result;
            }
        }
    }
}
