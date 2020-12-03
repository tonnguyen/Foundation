using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Foundation.Cms;
using Foundation.Features.Shared;
using Foundation.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    [ContentType(DisplayName = "About Visitor Block",
        GUID = "49713B53-2CC5-4579-B7AA-1173B0FCAE18",
        Description = "Block to show information of Visitor",
        GroupName = "Personalization")]
    [SiteImageUrl("~/assets/icons/cms/blocks/CMS-icon-block-30.png")]
    public class AboutVisitorBlock : FoundationBlockData
    {
        [Display(Name = "Heading text", Order = 10, GroupName = SystemTabNames.Content)]
        public virtual string HeadingText { get; set; }

        [Display(Name = "Show", Order = 10, GroupName = AboutVisitorBlockTabNames.VisitorGroups)]
        public virtual bool ShowVisitorGroupSection { get; set; }
        [Display(Name = "Label", Order = 20, GroupName = AboutVisitorBlockTabNames.VisitorGroups)]
        public virtual string VisitorGroupSectionHeadingText { get; set; }
        [Display(Name = "Number of groups to show", Order = 30, GroupName = AboutVisitorBlockTabNames.VisitorGroups)]
        public virtual int MaxVisitorsToShow { get; set; }

        [Display(Name = "Show", Order = 10, GroupName = AboutVisitorBlockTabNames.RecentActivity)]
        public virtual bool ShowRecentActivitySection { get; set; }
        [Display(Name = "Label", Order = 20, GroupName = AboutVisitorBlockTabNames.RecentActivity)]
        public virtual string RecentActivitySectionHeadingText { get; set; }
        [Display(Name = "Number of activities to show", Order = 30, GroupName = AboutVisitorBlockTabNames.RecentActivity)]
        public virtual int MaxEventsToShow { get; set; }

        [Display(Name = "Show", Order = 10, GroupName = AboutVisitorBlockTabNames.KeyTopics)]
        public virtual bool ShowKeyTopicsSection { get; set; }
        [Display(Name = "Label", Order = 20, GroupName = AboutVisitorBlockTabNames.KeyTopics)]
        public virtual string KeyTopicsSectionHeadingText { get; set; }
        [Display(Name = "Number of topics to show", Order = 30, GroupName = AboutVisitorBlockTabNames.KeyTopics)]
        public virtual int MaxTopicsToShow { get; set; }

        [Display(Name = "Width (px)", Order = 80, GroupName = TabNames.BlockStyling)]
        public virtual int Width { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            HeadingText = "About this visitor";
            VisitorGroupSectionHeadingText = "Visitor Groups";
            RecentActivitySectionHeadingText = "Recent Activity";
            KeyTopicsSectionHeadingText = "Key Topics";

            BackgroundColor = "white";
            Width = 400;
            Padding = "p-3";

            ShowVisitorGroupSection = true;
            MaxVisitorsToShow = 5;

            ShowRecentActivitySection = true;
            MaxEventsToShow = 5;

            ShowKeyTopicsSection = true;
            MaxTopicsToShow = 5;
        }
    }
}