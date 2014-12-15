using System;
using System.Collections.Generic;

namespace Simple.NH.Querying
{
    [Serializable]
    public class SessionOpenedEvent
    {
        private readonly List<string> _filtersToDisable = new List<string>();

        public SessionOpenedEvent()
        {
            this.EnableFilters = true;
        }

        public bool EnableFilters { get; set; }

        public IEnumerable<string> FiltersToDisable { get { return _filtersToDisable; } }

        public bool ShouldDisableFilter(string filterName)
        {
            return _filtersToDisable.Contains(filterName);
        }

        public void AddFilterToDisable(FilterName filterName)
        {
            _filtersToDisable.Add(filterName.ToString());
        }

        public void AddFiltersToDisable(IEnumerable<FilterName> filterNames)
        {
            if (filterNames != null)
            {
                foreach (var filterName in filterNames)
                {
                    AddFilterToDisable(filterName);
                }
            }
        }
    }
}