graphicsprocessor
=================

A C# library for image processing and Win32 screen capture

Here's an example that shows how to convert a bitmap to monochrome and vertically split it into 10 parts

```C#

            Bitmap image = new Bitmap("image.bmp");

            BitmapProcessor.BlackAndWhite(ref image);

            List<Bitmap> split = BitmapProcessor.Split(image, 10);
```
