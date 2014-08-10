graphicsprocessor
=================

A C# library for image processing and Win32 screen capture

Here's an example that shows how to capture a screen region, convert to monochrome and vertically split it into 10 parts

```C#
// Capture a square region 100x100 from the top left of the screen  
Bitmap image = ScreenCapture.Instance.CaptureScreen(0, 0, 100, 100);

// Convert it to black and white image
BitmapProcessor.BlackAndWhite(ref image);

// Split it into 10
List<Bitmap> split = BitmapProcessor.Split(image, 10);

// Dispose the image
image.Dispose();
```
