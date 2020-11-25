using Foundation.Features.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Features.Blocks.IdioAnalyseBlock
{
    public class IdioAnalyseBlockViewModel : BlockViewModel<IdioAnalyseBlock>
    {
        public IdioAnalyseBlockViewModel(IdioAnalyseBlock currentBlock) : base(currentBlock)
        {

        }

        public List<IdioTopicViewModel> Topics { get; set; }
        public string Content { get; set; }
    }
}