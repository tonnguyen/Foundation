using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Features.Shared;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace Foundation.Features.Blocks.IdioAnalyseBlock
{
    [TemplateDescriptor(Default = true)]
    public class IdioAnalyseBlockController : BlockController<IdioAnalyseBlock>
    {
        private readonly IPageRouteHelper _pageRouteHelper;
        private readonly string apiRootUrl = ConfigurationManager.AppSettings["IdioAnalyse.RootUrl"];
        private readonly string resourceGetTopics = ConfigurationManager.AppSettings["IdioAnalyse.ResourceGetTopics"];
        public IdioAnalyseBlockController(IPageRouteHelper pageRouteHelper)
        {
            _pageRouteHelper = pageRouteHelper;
        }

        public override ActionResult Index(IdioAnalyseBlock currentBlock)
        {
            var model = new IdioAnalyseBlockViewModel(currentBlock);
            var currentPage = _pageRouteHelper.Page;
            var pageData = currentPage as FoundationPageData;

            if (pageData != null)
            {
                var pageContent = GetPlainTextFromHtml(pageData.MainBody.ToString());
                model.Topics = GetTopicsFromContent(pageContent);
                model.Content = pageData.MainBody.ToString();
            }
            return PartialView("~/Features/Blocks/IdioAnalyseBlock/IdioAnalyseBlock.cshtml", model);
        }

        private List<IdioTopicViewModel> GetTopicsFromContent(string content)
        {
            var topics = new List<IdioTopicViewModel>();
            try
            {
                // Set up the request
                var client = new RestClient(apiRootUrl);
                var request = new RestRequest(resourceGetTopics);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { body = content });

                // Execute the request to get the profile
                var response = client.Post(request);
                var responseContent = response.Content;

                // Get the results as a JArray object
                var items = JArray.Parse(responseContent);
                topics = items.Select(o =>
                {
                    double.TryParse(o["weight"].ToString(), out var weight);
                    return new IdioTopicViewModel()
                    {
                        Title = o["title"].ToString(),
                        Weight = weight,
                        Anchor = o["anchor"].ToString()
                    };
                }).OrderByDescending(o => o.Weight)
                 .ToList();
                return topics;
            }
            catch (Exception)
            {
                return topics;
            }
        }

        private string GetPlainTextFromHtml(string html)
        {
            try
            {
                HtmlDocument htmldoc = new HtmlDocument();
                htmldoc.LoadHtml(html);

                return htmldoc.DocumentNode.InnerText;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}