using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Foundation.Cms;
using Foundation.Features.Shared;
using Foundation.Infrastructure;

namespace Foundation.Features.Blocks.IdioAnalyseBlock
{
    [ContentType(DisplayName = "Idio Analyse Block",
        GUID = "7B6725AD-5194-445D-BEEA-5634FF3E9182",
        Description = "Block to show information of content analysed by Idio",
        GroupName = GroupNames.Content)]
    [SiteImageUrl("~/assets/icons/cms/blocks/CMS-icon-block-30.png")]
    public class IdioAnalyseBlock : FoundationBlockData
    {
        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);
            BackgroundColor = "white";
        }
    }
}