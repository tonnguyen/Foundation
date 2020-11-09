using EPiServer.Framework.DataAnnotations;
using EPiServer.Personalization.VisitorGroups;
using EPiServer.Security;
using EPiServer.Shell.Security;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Security;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    [TemplateDescriptor(Default = true)]
    public class AboutVisitorBlockController : BlockController<AboutVisitorBlock>
    {
        private readonly IVisitorGroupRepository _visitorGroupRepository;
        private readonly IVisitorGroupRoleRepository _visitorGroupRoleRepository;
        private readonly UIUserProvider _userProvider;
        private readonly string apiRootUrl = ConfigurationManager.AppSettings["ProfileStore.Url"];
        private readonly string subscriptionKey = ConfigurationManager.AppSettings["ProfileStore.SubscriptionKey"];
        private readonly string resourceGetProfiles = ConfigurationManager.AppSettings["ProfileStore.ResourceGetProfiles"];
        private readonly string resourceGetEvents = ConfigurationManager.AppSettings["ProfileStore.ResourceGetEvents"];

        private readonly string idioApiRootUrl = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.IdioApiRootUrl"];
        private readonly string resourceGetTopics = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.ResourceGetTopics"];
        private readonly string idioAppKey = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.IdioApplicationKey"];
        private readonly string idioAppSecretKey = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.IdioApplicationSecretKey"];
        private readonly string idioDeliveryKey = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.IdioDeliveryKey"];
        private readonly string idioDeliverySecretKey = ConfigurationManager.AppSettings["foundation:AboutVisitorBlock.IdioDeliverySecretKey"];

        public AboutVisitorBlockController(IVisitorGroupRepository visitorGroupRepository,
            IVisitorGroupRoleRepository visitorGroupRoleRepository,
            UIUserProvider userProvider)
        {
            _visitorGroupRepository = visitorGroupRepository;
            _visitorGroupRoleRepository = visitorGroupRoleRepository;
            _userProvider = userProvider;
        }

        public override ActionResult Index(AboutVisitorBlock currentBlock)
        {
            var deviceId = Request.Cookies["_madid"]?.Value;
            var idioId = Request.Cookies["iv"]?.Value;
            var visitorGroups = new List<string>();
            var events = new List<TrackedEventViewModel>();
            var profile = new ProfileViewModel();
            var topics = new List<TopicViewModel>();

            if (User.Identity.IsAuthenticated)
            {
                var userInfo = _userProvider.GetUser(User.Identity.Name);
                profile = GetProfileByEmail(userInfo.Email);
                if (!string.IsNullOrEmpty(profile.Id))
                {
                    if (currentBlock.ShowRecentActivitySection)
                        events = GetEventsByEmail(userInfo.Email, currentBlock.MaxEventsToShow);
                    if (currentBlock.ShowVisitorGroupSection)
                        visitorGroups = GetVisitorGroups(currentBlock.MaxVisitorsToShow);
                }
            }
            else
            {
                profile = GetProfileByDeviceId(deviceId);
                if (!string.IsNullOrEmpty(profile.Id))
                {
                    if (currentBlock.ShowRecentActivitySection)
                        events = GetEventsByDeviceId(deviceId, currentBlock.MaxEventsToShow);
                }
            }
            if (currentBlock.ShowKeyTopicsSection)
                topics = GetTopicsByProfileId(idioId, currentBlock.MaxTopicsToShow);

            var model = new AboutVisitorBlockViewModel(currentBlock)
            {
                ProfileId = profile.Id,
                IdioId = idioId,
                Name = profile.Name,
                Email = profile.Email,
                Location = profile.GetAddress(),

                VisitorGroups = visitorGroups,
                Events = events,
                Topics = topics
            };
            return PartialView("~/Features/Blocks/AboutVisitorBlock/AboutVisitorBlock.cshtml", model);
        }

        private List<string> GetVisitorGroups(int limit)
        {
            List<string> visitorGroups = new List<string>();
            var helper = new VisitorGroupHelper(_visitorGroupRoleRepository);
            var groups = _visitorGroupRepository.List();

            foreach (var visitorGroup in groups)
            {
                if (helper.IsPrincipalInGroup(User, visitorGroup.Name))
                    visitorGroups.Add(visitorGroup.Name);
            }

            return visitorGroups.Skip(0).Take(limit).ToList();
        }

        private ProfileViewModel GetProfileByEmail(string email)
        {
            try
            {
                // Set up the request
                var client = new RestClient(apiRootUrl);
                var request = new RestRequest(resourceGetProfiles, Method.GET);
                request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Filter the profiles based on the current device id
                request.AddParameter("$filter", "Info.Email eq " + email);

                // Execute the request to get the profile
                var getProfileResponse = client.Execute(request);
                var getProfileContent = getProfileResponse.Content;

                // Get the results as a JArray object
                var profileResponseObject = JObject.Parse(getProfileContent);
                var profileArray = (JArray)profileResponseObject["items"];

                // Expecting an array of profiles with one item in it
                var profileObject = profileArray.First;
                if (profileObject == null) return new ProfileViewModel();

                var profileInfo = profileObject["Info"];
                return new ProfileViewModel()
                {
                    Id = profileObject["ProfileId"]?.ToString(),
                    Name = profileObject["Name"]?.ToString(),
                    Email = profileInfo != null ? profileInfo["Email"]?.ToString() : "",
                    City = profileInfo != null ? profileInfo["City"]?.ToString() : "",
                    State = profileInfo != null ? profileInfo["State"]?.ToString() : "",
                };
            }
            catch (Exception)
            {
                return new ProfileViewModel();
            }
        }

        private ProfileViewModel GetProfileByDeviceId(string deviceId)
        {
            try
            {
                // Set up the request
                var client = new RestClient(apiRootUrl);
                var request = new RestRequest(resourceGetProfiles, Method.GET);
                request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Filter the profiles based on the current device id
                request.AddParameter("$filter", "DeviceIds eq " + deviceId);

                // Execute the request to get the profile
                var getProfileResponse = client.Execute(request);
                var getProfileContent = getProfileResponse.Content;

                // Get the results as a JArray object
                var profileResponseObject = JObject.Parse(getProfileContent);
                var profileArray = (JArray)profileResponseObject["items"];

                // Expecting an array of profiles with one item in it
                var profileObject = profileArray.First;
                if (profileObject == null) return new ProfileViewModel();

                var profileInfo = profileObject["Info"];
                return new ProfileViewModel()
                {
                    Id = profileObject["ProfileId"]?.ToString(),
                    Name = profileObject["Name"]?.ToString(),
                    Email = profileInfo != null ? profileInfo["Email"]?.ToString() : "",
                    City = profileInfo != null ? profileInfo["City"]?.ToString() : "",
                    State = profileInfo != null ? profileInfo["State"]?.ToString() : "",
                };
            }
            catch (Exception)
            {
                return new ProfileViewModel();
            }
        }

        private List<TrackedEventViewModel> GetEventsByEmail(string email, int limit)
        {
            try
            {
                // Set up the request
                var client = new RestClient(apiRootUrl);
                var request = new RestRequest(resourceGetEvents, Method.GET);
                request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Filter the profiles based on the current device id
                request.AddParameter("$filter", $"User.Email eq {email}");
                request.AddParameter("$orderBy", "EventTime DESC");
                request.AddParameter("$top", limit);

                // Execute the request
                var getEventResponse = client.Execute(request);
                var getEventContent = getEventResponse.Content;

                // Get the results as a JArray object
                var eventResponseObject = JObject.Parse(getEventContent);
                var eventArray = (JArray)eventResponseObject["items"];

                if (eventArray == null)
                    return new List<TrackedEventViewModel>();

                return eventArray.Select(e => new TrackedEventViewModel()
                {
                    EventTime = e["EventTime"]?.ToString(),
                    EventType = e["EventType"]?.ToString(),
                    Value = e["Value"]?.ToString(),
                    PageUri = e["PageUri"]?.ToString()
                }).ToList();
            }
            catch (Exception)
            {
                return new List<TrackedEventViewModel>();
            }
        }
        private List<TrackedEventViewModel> GetEventsByDeviceId(string deviceId, int limit)
        {
            // Set up the request
            try
            {
                var client = new RestClient(apiRootUrl);
                var request = new RestRequest(resourceGetEvents, Method.GET);
                request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Filter the profiles based on the current device id
                request.AddParameter("$filter", $"DeviceId eq {deviceId}");
                request.AddParameter("$orderBy", "EventTime DESC");
                request.AddParameter("$top", limit);

                // Execute the request
                var getEventResponse = client.Execute(request);
                var getEventContent = getEventResponse.Content;

                // Get the results as a JArray object
                var eventResponseObject = JObject.Parse(getEventContent);
                var eventArray = (JArray)eventResponseObject["items"];

                if (eventArray == null)
                    return new List<TrackedEventViewModel>();

                return eventArray.Select(e => new TrackedEventViewModel()
                {
                    EventTime = e["EventTime"]?.ToString(),
                    EventType = e["EventType"]?.ToString(),
                    Value = e["Value"]?.ToString(),
                    PageUri = e["PageUri"]?.ToString()
                }).ToList();
            }
            catch (Exception)
            {
                return new List<TrackedEventViewModel>();
            }
        }

        public List<TopicViewModel> GetTopicsByProfileId(string profileId, int limit)
        {
            try
            {
                // Set up the request
                var requestUri = string.Format(resourceGetTopics, profileId);
                var client = new RestClient(idioApiRootUrl);
                var request = new RestRequest(requestUri, Method.GET);

                var signatureData = $"GET\n{requestUri}\n{DateTime.Now.ToString("yyy-MM-dd")}";

                var appSignature = HMACSHA1Hash(signatureData, idioAppSecretKey);
                var deliverySignature = HMACSHA1Hash(signatureData, idioDeliverySecretKey);
                request.AddHeader("X-App-Authentication", $"{idioAppKey}:{appSignature}");
                request.AddHeader("X-Delivery-Authentication", $"{idioDeliveryKey}:{deliverySignature}");

                request.AddParameter("rpp", limit);

                // Execute the request
                var getTopicsResponse = client.Execute(request);
                var getTopicsContent = getTopicsResponse.Content;

                // Get the results as a JArray object
                var topicResponseObject = JObject.Parse(getTopicsContent);
                var topicArray = (JArray)topicResponseObject["topic"];

                if (topicArray == null)
                    return new List<TopicViewModel>();

                return topicArray.Select(e =>
                {
                    var id = e["id"]?.ToString();
                    var name = e["title"]?.ToString();
                    double score = 0;
                    var scoreString = e["weight"]?.ToString();
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        if (double.TryParse(scoreString, out double parsedScore))
                            score = parsedScore;
                    }
                    return new TopicViewModel()
                    {
                        Id = id,
                        Name = name,
                        Score = score
                    };
                }).ToList();
            }
            catch (Exception)
            {
                return new List<TopicViewModel>();
            }
        }

        #region helpers
        private string HMACSHA1Hash(string input, string secretKey)
        {
            HMACSHA1 myhmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(secretKey));
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }
        #endregion
    }
}