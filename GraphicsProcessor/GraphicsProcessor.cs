using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace GraphicsProcessor
{
    /// <summary>
    /// Клас для обробки зображень
    /// </summary>
    public static class BitmapProcessor
    {
        #region Допоміжні функції

        /// <summary>Показує, чи однакові два кольори</summary>
        /// <param name="c1">Колір #1</param>
        /// <param name="c2">Колір #2</param>
        public static bool ColorsEqual(Color c1, Color c2)
        {
            return (c1.R == c2.R && c1.G == c2.G && c1.B == c2.B);
        }

        public static int MaxColorDifference(Color c1, Color c2)
        {
            int result = Math.Abs(c1.R - c2.R);

            int diff = Math.Abs(c1.G - c2.G);
            if (result < diff) result = diff;

            diff = Math.Abs(c1.B - c2.B);
            if (result < diff) result = diff;

            return result;
        }

        public static int ColorDifference(Color c1, Color c2)
        {
            int result = Math.Abs(c1.R - c2.R);
            result += Math.Abs(c1.G - c2.G);
            result += Math.Abs(c1.B - c2.B);

            return result;
        }

        public static Size GetSize(string path)
        {
            Bitmap bmp = new Bitmap(path);

            Size size = bmp.Size;

            bmp.Dispose();

            return size;
        }

        /// <summary>
        /// Визначає відношення кількості пікселів основних кольорів (0 - red, 1 - green, 2 - blue) до загальної кількості пікселів у зображенні
        /// </summary>
        /// <param name="bmp">Зображення</param>
        public static double[] GetRGBColorCoefs(Bitmap bmp)
        {
            int red = 0;
            int green = 0;
            int blue = 0;

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);

                    if (c.R > c.G + c.B) red++;
                    else if (c.G > c.R + c.B) green++;
                    else if (c.B > c.R + c.G) blue++;
                }


            double square = (double)(bmp.Width * bmp.Height);

            double[] result = new double[] { red, green, blue };

            return (from r in result select r / square).ToArray();
        }

        /// <summary>
        /// Визначає відношення кількості пікселів даного кольору до загальної кількості пікселів у зображенні
        /// </summary>
        /// <param name="bmp">Зображення</param>
        /// <param name="color">0 - red, 1 - green, 2 - blue</param>
        public static double GetRGBColorCoef(Bitmap bmp, int color)
        {
            if (color < 0 || color > 2) return 0.0;

            return GetRGBColorCoefs(bmp)[color];
        }

        /// <summary>
        /// Визначає відношення кількості пікселів даного кольору до загальної кількості пікселів у зображенні
        /// </summary>
        /// <param name="bmp">Зображення</param>
        /// <param name="color">Колір</param>
        public static double GetColorCoef(Bitmap bmp, Color color)
        {
            int count = 0;

            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);

                    if (ColorsEqual(c, color)) count++;
                }

            return (double)count / (double)(bmp.Width * bmp.Height);
        }

        #endregion

        #region Обрізання зображення

        /// <summary>Вирізає частину зображення</summary>
        /// <param name="source">Зображення для обробки</param>
        /// <param name="part">Прямокутник, який потрібно вирізати</param>
        public static void Copy(ref Bitmap source, Rectangle part)
        {
            // Створюємо нове зображення з потрібними розмірами
            Bitmap bmp = new Bitmap(part.Width, part.Height);

            // Ініціалізуємо графіку для цього зображення
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            // Малюємо частину зображення в цю графіку
            g.DrawImage(source, new Rectangle(Point.Empty, bmp.Size), part.X, part.Y, part.Width, part.Height, GraphicsUnit.Pixel);
            // Знищуємо її 
            g.Dispose();

            // Знищуємо початкове зображення
            source.Dispose();
            source = null;

            // Зберігаємо нове зображення в початкове
            source = bmp;
        }

        /// <summary>Вирізає частину зображення</summary>
        /// <param name="source">Зображення для обробки</param>
        /// <param name="part">Прямокутник, який потрібно вирізати</param>
        public static Bitmap Copy(Bitmap source, Rectangle part)
        {
            // Створюємо нове зображення з потрібними розмірами
            Bitmap bmp = new Bitmap(part.Width, part.Height);

            // Ініціалізуємо графіку для цього зображення
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            // Малюємо частину зображення в цю графіку
            g.DrawImage(source, new Rectangle(Point.Empty, bmp.Size), part.X, part.Y, part.Width, part.Height, GraphicsUnit.Pixel);
            // Знищуємо її 
            g.Dispose();

            // Зберігаємо нове зображення в початкове
            return bmp;
        }

        /// <summary>Видаляє білі вертикалі на початку зображення</summary>
        /// <param name="bmp">Зображення для обробки</param>
        public static void TrimStart(ref Bitmap bmp, int leave)
        {
            bool ok = true;

            int count = 0;

            while (count < bmp.Width - 1)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(count, y);
                    if (!ColorsEqual(c, Color.White))
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok) break;
                else count++;
            }

            BitmapProcessor.Copy(ref bmp, new Rectangle(Math.Max(0, count - leave), 0, bmp.Width - count, bmp.Height));
        }

        /// <summary>Видаляє білі вертикалі в кінці зображення</summary>
        /// <param name="bmp">Зображення для обробки</param>
        public static void TrimEnd(ref Bitmap bmp, int leave)
        {
            bool ok = true;

            int count = 0;

            while (count < bmp.Width - 1)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(bmp.Width - count - 1, y);
                    if (!ColorsEqual(c, Color.White))
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok) break;
                else count++;
            }

            BitmapProcessor.Copy(ref bmp, new Rectangle(0, 0, Math.Min(bmp.Width - count + leave, bmp.Width), bmp.Height));
        }

        /// <summary>Видаляє білі вертикалі на початку та в кінці зображення</summary>
        /// <param name="bmp">Зображення для обробки</param>
        public static void Trim(ref Bitmap bmp, int leave)
        {
            TrimStart(ref bmp, leave);
            TrimEnd(ref bmp, leave);
        }

        /// <summary>Видаляє білі вертикалі на початку та в кінці зображення</summary>
        /// <param name="bmp">Зображення для обробки</param>
        public static void Trim(ref Bitmap bmp)
        {
            TrimStart(ref bmp, 0);
            TrimEnd(ref bmp, 0);
        }

        /// <summary>
        /// Розбиває зображення на рівні частини
        /// </summary>
        /// <param name="bmp">Зображення для розбиття</param>
        /// <param name="count">Кількіть частин</param>
        public static List<Bitmap> Split(Bitmap bmp, int count)
        {
            List<Bitmap> result = new List<Bitmap>();

            int width = bmp.Width / count;

            for (int i = 0; i < count; i++)
            {
                Bitmap part = Copy(bmp, new Rectangle(i * width, 0, width, bmp.Height));
                Trim(ref part);

                result.Add(part);
            }

            return result;
        }

        public static Bitmap Scale(Bitmap bmp, double widthCoef, double heightCoef)
        {
            double w = (double)bmp.Width * widthCoef;
            double h = (double)bmp.Height * heightCoef;

            return Resize(bmp, (int)w, (int)h);
        }

        public static Bitmap Expand(Bitmap bmp, Size expand)
        {
            if (expand.Width == 0 && expand.Height == 0) return new Bitmap(bmp);

            Bitmap result = new Bitmap(bmp.Width + expand.Width, bmp.Height + expand.Height);

            Graphics g = Graphics.FromImage(result);
            g.Clear(Color.White);
            g.DrawImage(bmp, new Point(expand.Width / 2, expand.Height / 2));
            g.Save();

            return result;
        }

        public static Bitmap Resize(Bitmap bmp, int width, int height)
        {
            return Resize(bmp, width, height, InterpolationMode.High, SmoothingMode.None);
        }

        public static Bitmap Resize(Bitmap bmp, int width, int height, InterpolationMode interpolation, SmoothingMode smoothing)
        {
            Bitmap result = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(result);
            g.InterpolationMode = interpolation;
            g.SmoothingMode = smoothing;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.DrawImage(bmp, new Rectangle(0, 0, width, height));

            g.Save();

            return result;
        }

        #endregion

        #region Зміна кольору

        public static void BlackAndWhite(ref Bitmap sourceBitmap, int limit)
        {
            // Створюємо нове зображення того ж розміру
            Bitmap targetBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            for (int x = 0; x < sourceBitmap.Width; x++)
                for (int y = 0; y < sourceBitmap.Height; y++)
                {
                    Color c = sourceBitmap.GetPixel(x, y);

                    if (c.R > limit && c.G > limit && c.B > limit) targetBitmap.SetPixel(x, y, Color.White);
                    else targetBitmap.SetPixel(x, y, Color.Black);
                }

            sourceBitmap.Dispose();
            sourceBitmap = null;

            sourceBitmap = targetBitmap;
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        /// <param name="invert">Чи потрібно інвертувати колір</param>
        public static void BlackAndWhite(ref Bitmap sourceBitmap, bool invert)
        {
            byte mainColor = 255;
            byte backColor = 0;

            if (invert)
            {
                mainColor = 0;
                backColor = 255;
            }

            // Створюємо нове зображення того ж розміру
            Bitmap targetBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            // Проходимо по усіх пікселях початкового зображення
            for (int y = 0; y < sourceBitmap.Height; ++y)
                for (int x = 0; x < sourceBitmap.Width; ++x)
                {
                    // Беремо колір поточного пікселя
                    Color c = sourceBitmap.GetPixel(x, y);
                    // Вираховуємо середній колір
                    byte rgb = (byte)(0.3 * c.R + 0.59 * c.G + 0.11 * c.B);
                    // Якщо колір ближчий до білого
                    if (rgb > 256 / 2) rgb = mainColor;
                    // Якщо ж він ближчий до чорного - навпаки
                    else rgb = backColor;
                    // Присвоюємо вибраний колір відповідному пікселю в новому зображенні
                    targetBitmap.SetPixel(x, y, Color.FromArgb(c.A, rgb, rgb, rgb));
                }

            // Знищуємо початкове зображення
            sourceBitmap.Dispose();
            sourceBitmap = null;

            // Зберігаємо нове зображення в початкове
            sourceBitmap = targetBitmap;
        }

        public static byte GetIntesity(Color c, Color mainColor)
        {
            return (byte)(ColorDifference(c, mainColor) / 3);
        }

        public static byte[,] GetIntensityTable(Bitmap bmp, Color mainColor)
        {
            byte[,] result = new byte[bmp.Width, bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    result[x, y] = GetIntesity(c, mainColor);
                }
            }

            return result;
        }

        public static int[,] GetIntegralIntensityTable(byte[,] intensityTable)
        {
            int width = intensityTable.GetLength(0);
            int height = intensityTable.GetLength(1);

            int[,] result = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sum = 0;

                    for (int a = 0; a <= x; a++)
                        for (int b = 0; b <= y; b++)
                            sum += intensityTable[a, b];

                    result[x, y] = sum;
                }
            }

            return result;
        }

        public static int[,] GetIntegralIntensityTable(Bitmap bmp, Color mainColor)
        {
            byte[,] intensityTable = GetIntensityTable(bmp, mainColor);
            return GetIntegralIntensityTable(intensityTable);
        }

        public static byte GetIntensityThreshold(int[,] iiTable, int x, int y, short region)
        {
            int width = iiTable.GetLength(0);
            int height = iiTable.GetLength(1);

            int rad = region / 2;

            int minX = Math.Max(0, x - rad);
            int maxX = Math.Min(width - 1, x + rad);
            int minY = Math.Max(0, y - rad);
            int maxY = Math.Min(height - 1, y + rad);

            int a = iiTable[minX, minY];
            int b = iiTable[maxX, minY];
            int c = iiTable[minX, maxY];
            int d = iiTable[maxX, maxY];

            return (byte)((d - c - b + a) / (maxX - minX + 1) / (maxY - minY + 1));
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        /// <param name="invert">Чи потрібно інвертувати колір</param>
        public static void AdaptiveBlackAndWhite(ref Bitmap sourceBitmap, bool invert, short region, Color color)
        {
            byte mainColor = 255;
            byte backColor = 0;

            if (invert)
            {
                mainColor = 0;
                backColor = 255;
            }

            byte[,] iTable = GetIntensityTable(sourceBitmap, color);
            int[,] iiTable = GetIntegralIntensityTable(iTable);

            // Створюємо нове зображення того ж розміру
            Bitmap targetBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            // Проходимо по усіх пікселях початкового зображення
            for (int y = 0; y < sourceBitmap.Height; ++y)
                for (int x = 0; x < sourceBitmap.Width; ++x)
                {
                    byte intensity = iTable[x, y];
                    byte threshold = GetIntensityThreshold(iiTable, x, y, region);

                    // Якщо колір ближчий до білого
                    if (intensity > threshold) intensity = mainColor;
                    // Якщо ж він ближчий до чорного - навпаки
                    else intensity = backColor;
                    // Присвоюємо вибраний колір відповідному пікселю в новому зображенні
                    targetBitmap.SetPixel(x, y, Color.FromArgb(0, intensity, intensity, intensity));
                }

            // Знищуємо початкове зображення
            sourceBitmap.Dispose();
            sourceBitmap = null;

            // Зберігаємо нове зображення в початкове
            sourceBitmap = targetBitmap;
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        public static void BlackAndWhite(ref Bitmap sourceBitmap)
        {
            BlackAndWhite(ref sourceBitmap, false);
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        /// <param name="color">Колір, який буде замінено на чорний (решта - на білий)</param>
        /// <param name="invert">Чи потрібно інвертувати колір</param>
        public static void BlackAndWhite(ref Bitmap sourceBitmap, Color color, bool invert)
        {
            sourceBitmap = BlackAndWhite(sourceBitmap, color, invert);
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        /// <param name="color">Колір, який буде замінено на чорний (решта - на білий)</param>
        public static void BlackAndWhite(ref Bitmap sourceBitmap, Color color)
        {
            BlackAndWhite(ref sourceBitmap, color, false);
        }

        public static void BlackAndWhite(ref Bitmap sourceBitmap, Color color, int maxDiff)
        {
            for (int y = 0; y < sourceBitmap.Height; ++y)
                for (int x = 0; x < sourceBitmap.Width; ++x)
                {
                    Color c = sourceBitmap.GetPixel(x, y);

                    if (MaxColorDifference(color, c) <= maxDiff) sourceBitmap.SetPixel(x, y, Color.Black);
                    else sourceBitmap.SetPixel(x, y, Color.White);
                }
        }

        /// <summary>Конвертує зображення в монохромне</summary>
        /// <param name="sourceBitmap">Зображення для обробки</param>
        /// <param name="color">Колір, який буде замінено на чорний (решта - на білий)</param>
        /// <param name="invert">Чи потрібно інвертувати колір</param>
        public static Bitmap BlackAndWhite(Bitmap sourceBitmap, Color color, bool invert)
        {
            Bitmap result = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            Color mainColor = Color.Black;
            Color backColor = Color.White;

            if (invert)
            {
                mainColor = Color.White;
                backColor = Color.Black;
            }

            for (int y = 0; y < sourceBitmap.Height; ++y)
                for (int x = 0; x < sourceBitmap.Width; ++x)
                {
                    Color c = sourceBitmap.GetPixel(x, y);

                    if (ColorsEqual(c, color)) result.SetPixel(x, y, mainColor);
                    else result.SetPixel(x, y, backColor);
                }

            return result;
        }

        public static void Grayscale(ref Bitmap sourceBitmap)
        {
            Grayscale(ref sourceBitmap, false);
        }

        public static void Grayscale(ref Bitmap sourceBitmap, bool invert)
        {
            for (int i = 0; i < sourceBitmap.Width; i++)
            {
                for (int j = 0; j < sourceBitmap.Height; j++)
                {
                    //get the pixel from the original image
                    Color originalColor = sourceBitmap.GetPixel(i, j);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    if (invert) grayScale = 255 - grayScale;

                    //create the color object
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    //set the new image's pixel to the grayscale version
                    sourceBitmap.SetPixel(i, j, newColor);
                }
            }
        }

        public static void MaxGrayscale(ref Bitmap sourceBitmap)
        {
            for (int i = 0; i < sourceBitmap.Width; i++)
            {
                for (int j = 0; j < sourceBitmap.Height; j++)
                {
                    //get the pixel from the original image
                    Color originalColor = sourceBitmap.GetPixel(i, j);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .33) + (originalColor.G * .34)
                        + (originalColor.B * .33));

                    //create the color object
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    //set the new image's pixel to the grayscale version
                    sourceBitmap.SetPixel(i, j, newColor);
                }
            }
        }

        #endregion

        #region Пошук підзображень

        /// <summary>Знаходить усі позиції subBmp в bmp</summary>
        /// <param name="bmp">Саме зображення</param>
        /// <param name="subBmp">Підзображення (тієї ж висоти, що і саме зображення)</param>
        /// <param name="maxDiff">Кількість точок, достатня для того, щоб вважати зображення різними</param>
        /// <param name="enoughSim">Мінімально достатня кількість точок для того, щоб вважати зображення схожими</param>
        /// <returns>Список позицій входжень subBmp в bmp</returns>
        /// <remarks>** НЕ ТЕСТОВАНО **</remarks>
        public static List<int> FindSubimage(Bitmap bmp, Bitmap subBmp, int maxDiff, int enoughSim, bool gray)
        {
            return FindSubimage(bmp, subBmp, maxDiff, enoughSim, gray, 0);
        }

        /// <summary>Знаходить усі позиції subBmp в bmp</summary>
        /// <param name="bmp">Саме зображення</param>
        /// <param name="subBmp">Підзображення (тієї ж висоти, що і саме зображення)</param>
        /// <param name="maxDiff">Кількість точок, достатня для того, щоб вважати зображення різними</param>
        /// <param name="enoughSim">Мінімально достатня кількість точок для того, щоб вважати зображення схожими</param>
        /// <param name="maxColorDiff">Максимальна дозволена різниця в кольорі, щоб вважати кольори однаковими</param>
        /// <returns>Список позицій входжень subBmp в bmp</returns>
        /// <remarks>** НЕ ТЕСТОВАНО **</remarks>
        public static List<int> FindSubimage(Bitmap bmp, Bitmap subBmp, int maxDiff, int enoughSim, bool gray, int maxColorDiff)
        {
            // Ініціалізуємо список позицій
            List<int> result = new List<int>();

            // Кількість схожих пікселів
            int simCount = 0;
            // Кількість різних пікселів
            int diffCount = 0;
            // Поточний стовбець subBmp
            int col = 0;

            // Проходимо по усіх стовбцях 
            for (int x = 0; x < bmp.Width; x++)
            {
                // Проходимо по усіх рядках
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color bmpColor = bmp.GetPixel(x, y);
                    Color subColor = subBmp.GetPixel(col, y);

                    // Якщо відповідні пікселі двох зображень мають однаковий колір, збільшуємо simCount
                    if (MaxColorDifference(bmpColor, subColor) <= maxColorDiff) simCount++;
                    // якщо ні - збільшуємо diffCount
                    else
                    {
                        if (gray)
                        {
                            if ((bmpColor.R != 0 && bmpColor.R != 255) || (subColor.R != 0 && subColor.R != 255)) simCount++;
                            else diffCount++;
                        }
                        else diffCount++;
                    }

                    // Якщо різних пікселів стало забагато
                    if (diffCount > maxDiff)
                    {
                        // Повертаємось на початковий стовбець
                        x = x - col;

                        // Якщо ми зайшли за кінець зображення
                        if (x >= bmp.Width)
                        {
                            // виходимо
                            return result;
                        }

                        // Обнуляємо усі змінні
                        simCount = 0;
                        diffCount = 0;
                        col = 0;

                        // Переходимо на наступний стовбчик
                        break;
                    }

                    // Якщо однакових пікселів уже достатньо
                    if (simCount >= enoughSim)
                    {
                        // Додаємо початкову позицію до результату
                        result.Add(x - col);

                        // Переходимо на стовбчик, що йде після subBmp
                        x = x - col + subBmp.Width - 1;

                        // Обнуляємо усі змінні
                        simCount = 0;
                        diffCount = 0;
                        col = 0;

                        // Якщо ми зайшли за кінець зображення
                        if (x >= bmp.Width)
                        {
                            // виходимо
                            return result;
                        }

                        // Переходимо на наступний стовбчик
                        break;
                    }

                    // Якщо ми уже повністю пройшли цей стовбчик, збільшуємо col
                    if (y == bmp.Height - 1) col++;
                }

                // Якщо ми повністю пройшли усе subBmp
                if (col == subBmp.Width)
                {
                    // Додаємо поточну позицію до результату
                    result.Add(x - col + 1);

                    // Переходимо на стовбчик, що йде після subBmp
                    x = x - col + subBmp.Width;

                    // Обнуляємо змінні
                    simCount = 0;
                    diffCount = 0;
                    col = 0;

                    // Якщо ми зайшли за кінець зображення
                    if (x >= bmp.Width)
                    {
                        // виходимо
                        return result;
                    }
                }
            }

            return result;
        }

        public static List<int> FindSmallerSubimage(Bitmap bmp, Bitmap subBmp, int maxDiff, int enoughSim, bool gray, int maxColorDiff)
        {
            List<int> result = new List<int>();

            int count = bmp.Height - subBmp.Height;

            for (int i = 0; i <= count; i++)
            {
                Rectangle rect = new Rectangle(0, i, bmp.Width, subBmp.Height);

                Bitmap newBmp = Copy(bmp, rect);

                List<int> positions = FindSubimage(newBmp, subBmp, maxDiff, enoughSim, gray, maxColorDiff);

                result.AddRange(positions);

                newBmp.Dispose();
            }

            return result;
        }

        public static List<int> FindSmallerSubimage(Bitmap bmp, Bitmap subBmp, int maxDiff, int enoughSim, bool gray)
        {
            return FindSmallerSubimage(bmp, subBmp, maxDiff, enoughSim, gray, 0);
        }

        /// <summary>Повертає істину, якщо зображення схожі</summary>
        /// <param name="b1">Зображення</param>
        /// <param name="b2">Зображення</param>
        /// <param name="colorDiff">Максимальна різниця кольорів</param>
        public static bool AreEqual(Bitmap b1, Bitmap b2, int colorDiff)
        {
            return AreEqual(b1, b2, colorDiff, 0);
        }

        public static bool AreEqual(Bitmap b1, Bitmap b2, int colorDiff, int diff)
        {
            if (b1.Size != b2.Size) return false;

            int count = 0;

            for (int x = 0; x < b1.Width; x++)
                for (int y = 0; y < b1.Height; y++)
                {
                    Color c1 = b1.GetPixel(x, y);
                    Color c2 = b2.GetPixel(x, y);

                    if (Math.Abs(c1.R - c2.R) > colorDiff || Math.Abs(c1.G - c2.G) > colorDiff || Math.Abs(c1.B - c2.B) > colorDiff) if (++count > diff) return false;
                }

            return true;
        }

        public static bool Contains(Bitmap bmp, Bitmap subBmp, int colorDiff)
        {
            return Contains(bmp, subBmp, colorDiff, 0);
        }

        public static bool Contains(Bitmap bmp, Bitmap subBmp, int colorDiff, int diff)
        {
            for (int x = 0; x <= bmp.Width - subBmp.Width; x++)
                for (int y = 0; y <= bmp.Height - subBmp.Height; y++)
                {
                    Bitmap temp = BitmapProcessor.Copy(bmp, new Rectangle(new Point(x, y), subBmp.Size));

                    if (AreEqual(temp, subBmp, colorDiff, diff))
                    {
                        temp.Dispose();
                        return true;
                    }

                    temp.Dispose();
                }

            return false;
        }

        public static int Difference(Bitmap bmp1, Bitmap bmp2, int allowedColorDiff)
        {
            if (bmp1.Size != bmp2.Size) return int.MaxValue;

            int result = 0;

            for (int x = 0; x < bmp1.Width; x++)
                for (int y = 0; y < bmp1.Height; y++)
                {
                    Color c1 = bmp1.GetPixel(x, y);
                    Color c2 = bmp2.GetPixel(x, y);

                    int diff = MaxColorDifference(c1, c2);

                    if (diff > allowedColorDiff) result++;
                }

            return result;
        }

        public static int MaxColorDifference(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size) return int.MaxValue;

            int result = 0;

            for (int x = 0; x < bmp1.Width; x++)
                for (int y = 0; y < bmp1.Height; y++)
                {
                    Color c1 = bmp1.GetPixel(x, y);
                    Color c2 = bmp2.GetPixel(x, y);

                    result += MaxColorDifference(c1, c2);
                }

            return result;
        }

        public static int ColorDifference(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size) return int.MaxValue;

            int result = 0;

            for (int x = 0; x < bmp1.Width; x++)
                for (int y = 0; y < bmp1.Height; y++)
                {
                    Color c1 = bmp1.GetPixel(x, y);
                    Color c2 = bmp2.GetPixel(x, y);

                    result += ColorDifference(c1, c2);
                }

            return result;
        }

        public static bool IsEmpty(Bitmap bmp)
        {
            return IsEmpty(bmp, Color.White);
        }

        public static bool IsEmpty(Bitmap bmp, Color emptyColor)
        {
            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (!ColorsEqual(bmp.GetPixel(x, y), emptyColor)) return false;
                }

            return true;
        }

        #endregion

        #region Конвертація

        /// <summary>Конвертує зображення в неіндексоване</summary>
        /// <param name="img">Зображення для обробки</param>
        public static Bitmap ToNonIndexed(Bitmap img)
        {
            Bitmap image = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = image.Palette;
            for (int i = 0; i <= 255; i++)
            {
                // create greyscale color table
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }

            img.Palette = pal;

            Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppRgb);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
            gr.DrawImage(img, 0, 0);

            gr.Dispose();
            img.Dispose();
            image.Dispose();

            return bmp;
        }

        #endregion

        #region Накладання

        public static void Overlay(ref Bitmap baseImage, Bitmap topImage, Point location)
        {
            if (baseImage == null || topImage == null) return;

            var result = new Bitmap(baseImage.Width, baseImage.Height);

            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(baseImage, new Point(0, 0));
                g.DrawImage(topImage, location);
                g.Save();

                baseImage.Dispose();
                baseImage = result;
            }
        }

        public static void DrawText(ref Bitmap baseImage, string text, Font font, Brush brush, Rectangle rect)
        {
            if (baseImage == null || string.IsNullOrEmpty(text)) return;

            var result = new Bitmap(baseImage.Width, baseImage.Height);

            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(baseImage, new Point());

                StringFormat stringFormat = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                g.DrawString(text, font, brush, rect, stringFormat);

                g.Save();

                baseImage.Dispose();
                baseImage = result;
            }
        }

        #endregion
    }
}
