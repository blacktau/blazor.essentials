using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Blazor.Essentials.IntersectionObserverAPI
{
    public class IntersectionObserverOptions
    {
        public ElementReference? Root { get; set; }

        public string RootMargin { get; set; }

        public List<decimal> Threshold { get; set; }
    }
}