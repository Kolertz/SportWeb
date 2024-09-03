using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SportWeb.TagHelpers
{
    [HtmlTargetElement("avatar-image", Attributes = "user-id")]
    public class AvatarTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AvatarTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //[HtmlAttributeName("user-id")]
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var UserId = httpContext?.Request.Cookies["avatarUrl"];
            var avatarUrl = $"{Directory.GetCurrentDirectory()}/wwwroot/img/{UserId}";
            output.TagName = "img";
            output.Attributes.SetAttribute("src", avatarUrl);
            output.Attributes.SetAttribute("alt", "Avatar");
        }
    }
}