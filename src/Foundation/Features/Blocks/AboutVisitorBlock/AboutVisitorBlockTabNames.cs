using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    [GroupDefinitions]
    public class AboutVisitorBlockTabNames
    {
        [Display(Name = "Visitor Groups", Order = 11)]
        public const string VisitorGroups = "VisitorGroups";
        [Display(Name = "Recent Activity", Order = 12)]
        public const string RecentActivity = "RecentActivity";
        [Display(Name = "Key Topics", Order = 13)]
        public const string KeyTopics = "KeyTopics";
    }
}