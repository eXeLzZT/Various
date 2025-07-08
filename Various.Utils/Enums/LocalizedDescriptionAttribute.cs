using System;
using System.ComponentModel;
using System.Resources;

namespace Various.Utils.Enums
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly ResourceManager _resourceManager;
        private readonly string _resourceKey;

        public override string Description
        {
            get
            {
                var description = _resourceManager.GetString(_resourceKey);

                return string.IsNullOrWhiteSpace(description) ? $"[[{_resourceKey}]]" : description;
            }
        }

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }
    }
}
