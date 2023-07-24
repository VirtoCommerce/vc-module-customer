using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public sealed class IconService : IIconService
    {
        private readonly IImageService _imageService;
        private readonly IImageResizer _imageResizer;

        private const int _defaultImageSize = 90;

        public IconService(IImageService imageService, IImageResizer imageResizer)
        {
            _imageService = imageService;
            _imageResizer = imageResizer;
        }

        public Task ResizeIcon(IconResizeRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                throw new ArgumentNullException(request.Url);
            }

            if (request.Width == null || request.Height == null)
            {
                request.Width = _defaultImageSize;
                request.Height = _defaultImageSize;
            }

            return InnerResizeAsync(request);
        }

        private async Task InnerResizeAsync(IconResizeRequest request)
        {
            var image = await _imageService.LoadImageAsync(request.Url, out var format);

            if (image.Width != image.Height)
            {
                var imageSide = Math.Min(image.Width, image.Height);
                image = _imageResizer.Crop(image, imageSide, imageSide, AnchorPosition.Center);
            }

            image = _imageResizer.FixedSize(image, request.Width.Value, request.Height.Value, Color.Transparent);

            await _imageService.SaveImageAsync(request.Url, image, format, JpegQuality.High);
        }
    }
}
