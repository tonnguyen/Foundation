using Foundation.Features.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    public class AboutVisitorBlockViewModel : BlockViewModel<AboutVisitorBlock>
    {
        public AboutVisitorBlockViewModel(AboutVisitorBlock currentBlock) : base(currentBlock)
        {
            HeadingText = currentBlock.HeadingText;
            ShowVisitorGroupSection = currentBlock.ShowVisitorGroupSection;
            ShowRecentActivitySection = currentBlock.ShowRecentActivitySection;
            ShowKeyTopicsSection = currentBlock.ShowKeyTopicsSection;
            VisitorGroupSectionHeadingText = currentBlock.VisitorGroupSectionHeadingText;
            RecentActivitySectionHeadingText = currentBlock.RecentActivitySectionHeadingText;
            KeyTopicsSectionHeadingText = currentBlock.KeyTopicsSectionHeadingText;
            Width = currentBlock.Width;

            VisitorGroups = new List<string>();
            Events = new List<TrackedEventViewModel>();
            Topics = new List<TopicViewModel>();
        }

        public virtual string HeadingText { get; set; }
        public bool ShowVisitorGroupSection { get; set; }
        public bool ShowRecentActivitySection { get; set; }
        public bool ShowKeyTopicsSection { get; set; }

        public string VisitorGroupSectionHeadingText { get; set; }
        public string RecentActivitySectionHeadingText { get; set; }
        public string KeyTopicsSectionHeadingText { get; set; }

        public string ProfileId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }

        //section data
        public List<string> VisitorGroups { get; set; }
        public IEnumerable<TrackedEventViewModel> Events { get; set; }
        public IEnumerable<TopicViewModel> Topics { get; set; }

        //styles
        public int Width { get; set; }
    }
}