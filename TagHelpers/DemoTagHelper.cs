using Microsoft.AspNetCore.Razor.TagHelpers;

namespace L12.TagHelpers
{
   [HtmlTargetElement("bigsmall")]
   public class DemoTagHelper : TagHelper
   {
      public async override void Process(TagHelperContext context, 
                                         TagHelperOutput output)
      {
         var content = (await output.GetChildContentAsync()).GetContent();

         string newText = "";
         for (int i = 0; i < content.Length; i++)
         {
            if (i % 2 == 0)
               newText += "<i>" + content.Substring(i, 1).ToUpper() + "</i>";
            else
               newText += "<u>" + content.Substring(i, 1).ToLower() + "</u>";
         }

         output.TagName = "b"; // Replaces <bigsmall> with <b>"
         output.Content.SetHtmlContent(newText);
      }
   }
}

