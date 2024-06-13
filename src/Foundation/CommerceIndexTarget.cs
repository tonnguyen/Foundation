using Optimizely.ContentGraph.Cms.Services.Internal;

namespace Foundation
{
    public class CommerceIndexTarget : IIndexTarget
    {
        /// <summary>
        /// The content references.
        /// </summary>
        public ContentReference ContentRoot => ServiceLocator.Current.GetInstance<ReferenceConverter>().GetRootLink();
    }
}
