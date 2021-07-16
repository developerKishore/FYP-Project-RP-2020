using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.IO;
using ZXing.QrCode;

namespace L12.TagHelpers
{
   [HtmlTargetElement("qrcode")]
   public class QRCodeTagHelper : TagHelper
   {
      public override async void Process(TagHelperContext context, TagHelperOutput output)
      {

         var QrcodeContent = (await output.GetChildContentAsync()).GetContent();
         output.SuppressOutput();

         var w = context.AllAttributes["width"]?.Value.ToString() ?? "200";
         var h = context.AllAttributes["height"]?.Value.ToString() ?? "200";

         var width  = Int32.Parse(w);  // width of the Qr Code   
         var height = Int32.Parse(h);  // height of the Qr Code   
         var margin = 0;
         var qrCodeWriter = new ZXing.BarcodeWriterPixelData
         {
            Format = ZXing.BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
               Height = height,
               Width = width,
               Margin = margin
            }
         };
         var pixelData = qrCodeWriter.Write(QrcodeContent);
         // creating a bitmap from the raw pixel data; if only black and white colors are used it makes no difference   
         // that the pixel data ist BGRA oriented and the bitmap is initialized with RGB   
         using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
         using (var ms = new MemoryStream())
         {
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), 
               System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
               // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image   
               System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            }
            finally
            {
               bitmap.UnlockBits(bitmapData);
            }
            // save to stream as PNG   
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            output.TagName = "img";
            output.Attributes.Clear();
            output.Attributes.Add("width", width);
            output.Attributes.Add("height", height);
            output.Attributes.Add("src", String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray())));
         }

      }
   }
}
