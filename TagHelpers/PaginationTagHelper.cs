using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SportWeb.Models;
namespace SportWeb.TagHelpers
{
    [HtmlTargetElement("pagination", Attributes = "pagination-model")]
    public class PaginationTagHelper : TagHelper
    {
        private IUrlHelperFactory _urlHelperFactory;
        public PaginationTagHelper(IUrlHelperFactory helperFactory)
        {
            _urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;
        [HtmlAttributeName("pagination-model")]
        public PaginationModel PaginationModel { get; set; }
        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (PaginationModel == null || PaginationModel.TotalPages <= 1)
                return;
            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            TagBuilder result = new TagBuilder("div");
            for (int i = PaginationModel.StartPage; i <= PaginationModel.EndPage; i++)
            {
                var li = new TagBuilder("li");
                li.AddCssClass("page-item");
                if (i == PaginationModel.CurrentPage)
                    li.AddCssClass("active");

                var a = new TagBuilder("a");
                a.AddCssClass("page-link");
                a.InnerHtml.Append(i.ToString());
                a.Attributes["href"] = urlHelper.Action(PageAction, new { page = i });
                a.InnerHtml.Append(i.ToString());

                li.InnerHtml.AppendHtml(a);
                ul.InnerHtml.AppendHtml(li);
            }
        }
    }
}
