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
using System.Linq;
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
            var visitorGroups = new List<string>();
            var events = new List<TrackedEventViewModel>();
            var profile = new ProfileViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var userInfo = _userProvider.GetUser(User.Identity.Name);
                profile = GetProfileByEmail(userInfo.Email);
                if (currentBlock.ShowRecentActivitySection)
                    events = GetEventsByEmail(User.Identity.Name, currentBlock.MaxEventsToShow);
                if (currentBlock.ShowVisitorGroupSection)
                    visitorGroups = GetVisitorGroups(currentBlock.MaxVisitorsToShow);
            }
            else
            {
                profile = GetProfileByDeviceId(deviceId);
                if (currentBlock.ShowRecentActivitySection)
                    events = GetEventsByDeviceId(deviceId, currentBlock.MaxEventsToShow);
            }

            var model = new AboutVisitorBlockViewModel(currentBlock)
            {
                ProfileId = profile.Id,
                Name = profile.Name,
                Email = profile.Email,
                Location = profile.GetAddress(),

                VisitorGroups = visitorGroups,
                Events = events
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

        private ProfileViewModel GetProfileByDeviceId(string deviceId)
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

        private List<TrackedEventViewModel> GetEventsByEmail(string email, int limit)
        {
            // Set up the request
            var client = new RestClient(apiRootUrl);
            var request = new RestRequest(resourceGetEvents, Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Filter the profiles based on the current device id
            request.AddParameter("$filter", $"User.Email eq {email}");
            request.AddParameter("$orderBy", "EventTime DESC");
            request.AddParameter("$top", limit);

            // Execute the request to get the profile
            var getEventResponse = client.Execute(request);
            var getEventContent = getEventResponse.Content;

            // Get the results as a JArray object
            var eventResponseObject = JObject.Parse(getEventContent);
            var eventArray = (JArray)eventResponseObject["items"];

            return eventArray.Select(e => new TrackedEventViewModel()
            {
                EventTime = e["EventTime"]?.ToString(),
                EventType = e["EventType"]?.ToString(),
                Value = e["Value"]?.ToString()
            }).ToList();
        }
        private List<TrackedEventViewModel> GetEventsByDeviceId(string deviceId, int limit)
        {
            // Set up the request
            var client = new RestClient(apiRootUrl);
            var request = new RestRequest(resourceGetEvents, Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Filter the profiles based on the current device id
            request.AddParameter("$filter", $"DeviceId eq {deviceId}");
            request.AddParameter("$orderBy", "EventTime DESC");
            request.AddParameter("$top", limit);

            // Execute the request to get the profile
            var getEventResponse = client.Execute(request);
            var getEventContent = getEventResponse.Content;

            // Get the results as a JArray object
            var eventResponseObject = JObject.Parse(getEventContent);
            var eventArray = (JArray)eventResponseObject["items"];

            return eventArray.Select(e => new TrackedEventViewModel()
            {
                EventTime = e["EventTime"]?.ToString(),
                EventType = e["EventType"]?.ToString(),
                Value = e["Value"]?.ToString()
            }).ToList();
        }
    }
}