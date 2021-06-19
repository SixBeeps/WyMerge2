<<<<<<< HEAD
﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Linq;

namespace WyMerge2 {
    class Program {

        public enum InterpolationTypes {
            Hue,
            Brightness,
            Random
        }

        public static Random rng = new Random();

        static void PrintDocs() {
            Console.WriteLine(@"~~~ WyMerge Image Animation, Version 2 ~~~

WYMERGE2 [-o path] [-i path] [-f frames] [-d delay] [-l reps] [-c frames] [-r WxH] [-B or -R] img1 img2 [img3...]

Options:
-o : Output file location (default out.gif)
-i : Split animation into frames, put into specified folder
-f : Number of interpolation frames in animation (default 25)
-d : Delay between frames in ms (default 200)
-l : Number of times to loop the animation (default -1)
-c : Number of capstone frames (default 1)
-r : Forces the resolution of the resulting animation to be W by H
-B : Compare by brightness
-R : Compare by nothing (random)
-? : Shows this help listing
-7 : Activates blue mode

By default, WyMerges compare by hue");
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                PrintDocs();
                return;
            }

            string outputLocation = "out.gif";
            string frameFolder = "";
            InterpolationTypes interpolationMode = InterpolationTypes.Hue;
            int interpolationFrames = 25;
            int frameDelay = 200;
            int repeats = -1;
            Size? resolution = null;
            int capstoneFrames = 1;
            List<string> imgPaths = new List<string>();

            for (int argIdx = 0; argIdx < args.Length; argIdx++) {
                switch(args[argIdx]) {
                    case "/o":
                    case "-o":
                        outputLocation = args[argIdx + 1];
                        argIdx++;
                        break;
                    case "/i":
                    case "-i":
                        frameFolder = args[argIdx + 1];
                        argIdx++;
                        break;
                    case "/f":
                    case "-f":
                        interpolationFrames = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/d":
                    case "-d":
                        frameDelay = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/l":
                    case "-l":
                        repeats = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/c":
                    case "-c":
                        capstoneFrames = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/B":
                    case "-B":
                        interpolationMode = InterpolationTypes.Brightness;
                        break;
                    case "/R":
                    case "-R":
                        interpolationMode = InterpolationTypes.Random;
                        break;
                    case "/r":
                    case "-r":
                        string res = args[argIdx + 1];
                        argIdx++;
                        int width = int.Parse(res.Substring(0, res.IndexOf("x")));
                        int height = int.Parse(res.Substring(res.IndexOf("x") + 1));
                        resolution = new Size(width, height);
                        break;
                    case "/?":
                    case "-?":
                        PrintDocs();
                        break;
                    case "/7":
                    case "-7":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    default:
                        if (File.Exists(args[argIdx])) {
                            imgPaths.Add(args[argIdx]);
                        } else {
                            Console.WriteLine($"File {args[argIdx]} doesn\'t exist, ignoring");
                        }
                        break;
                }
            }

            if (imgPaths.Count < 2) {
                Console.WriteLine("Not enough input images to make a WyMerge from");
                return;
            }

            List<Bitmap> frames = new List<Bitmap>(imgPaths.Count * interpolationFrames);
            if (!resolution.HasValue) {
                Bitmap firstImage = (Bitmap)Bitmap.FromFile(imgPaths.First());
                Size res = firstImage.Size;
                resolution = new Size(res.Width, res.Height);
            }
            for (int imgIdx = 0; imgIdx < imgPaths.Count - 1; imgIdx++) {
                Bitmap from = (Bitmap)Bitmap.FromFile(imgPaths[imgIdx]);
                Bitmap to = (Bitmap)Bitmap.FromFile(imgPaths[imgIdx + 1]);

                from = new Bitmap(from, resolution.Value);
                to = new Bitmap(to, resolution.Value);


                List<Bitmap> sectFrames = WyMerge(from, to, interpolationFrames, interpolationMode);
                foreach(Bitmap frame in sectFrames) {
                    frames.Add(frame);
                }
            }

            GifWriter gif = new GifWriter(outputLocation, frameDelay, repeats);
            for (int c = 0; c < capstoneFrames; c++) {
                gif.WriteFrame(frames.First());
            }
            foreach(Bitmap frame in frames) {
                gif.WriteFrame(frame);
                if (frameFolder != "") frame.Save($"{frameFolder}/frame_{frame.GetHashCode()}");
            }
            for (int c = 0; c < capstoneFrames - 1; c++) {
                gif.WriteFrame(frames.Last());
            }
            gif.Dispose();
            Console.ResetColor();
        }

        /// <summary>
        /// Serializes an image into a 1D array
        /// </summary>
        /// <param name="image">The image to serialize</param>
        /// <returns>Array of <c>Pixel</c>s representing the image in reading order</returns>
        static Pixel[] SerializeImageToArray(Bitmap image) {
            List<Pixel> serialized = new List<Pixel>(image.Width * image.Height);

            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    serialized.Add(new Pixel(image.GetPixel(x, y), x, y));
                }
            }

            return serialized.ToArray();
        }

        static List<Bitmap> WyMerge(Bitmap fromImg, Bitmap toImg, int interpFrames, InterpolationTypes interpType) {
            List<Bitmap> frames = new List<Bitmap>(interpFrames + 1);

            var fromSerialized = (from p in SerializeImageToArray(fromImg)
                                 orderby p.Composite(interpType)
                                 select p).ToList();

            var toSerialized = (from p in SerializeImageToArray(toImg)
                               orderby p.Composite(interpType)
                               select p).ToList();

            for (int f = 0; f < interpFrames; f++) {
                Bitmap currentFrame = new Bitmap(fromImg.Width, fromImg.Height);
                for (int currentPixel = 0; currentPixel < fromSerialized.Count(); currentPixel++) {
                    Pixel newPixel = Pixel.Interpolate(
                        fromSerialized[currentPixel],
                        toSerialized[currentPixel],
                        f / (float)interpFrames
                    );
                    currentFrame.SetPixel((int)newPixel.position.X, (int)newPixel.position.Y, newPixel.color);
                }
                frames.Add(currentFrame);
            }

            frames.Add(toImg);
            return frames;
        }
    }
}
=======
﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Linq;

namespace WyMerge2 {
    class Program {

        public enum InterpolationTypes {
            Hue,
            Brightness,
            Random
        }

        public static Random rng = new Random();

        static void PrintDocs() {
            Console.WriteLine(@"~~~ WyMerge Image Animation, Version 2 ~~~

WYMERGE2 [-o path] [-i path] [-f frames] [-d delay] [-l reps] [-c frames] [-r WxH] [-B or -R] img1 img2 [img3...]

Options:
-o : Output file location (default out.gif)
-i : Split animation into frames, put into specified folder
-f : Number of interpolation frames in animation (default 25)
-d : Delay between frames in ms (default 200)
-l : Number of times to loop the animation (default -1)
-c : Number of capstone frames (default 1)
-r : Forces the resolution of the resulting animation to be W by H
-B : Compare by brightness
-R : Compare by nothing (random)
-? : Shows this help listing
-7 : Activates blue mode

By default, WyMerges compare by hue");
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                PrintDocs();
                return;
            }

            string outputLocation = "out.gif";
            string frameFolder = "";
            InterpolationTypes interpolationMode = InterpolationTypes.Hue;
            int interpolationFrames = 25;
            int frameDelay = 200;
            int repeats = -1;
            Size? resolution = null;
            int capstoneFrames = 1;
            List<string> imgPaths = new List<string>();

            // Process all command-line args
            for (int argIdx = 0; argIdx < args.Length; argIdx++) {
                switch(args[argIdx]) {
                    case "/o":
                    case "-o":
                        outputLocation = args[argIdx + 1];
                        argIdx++;
                        break;
                    case "/i":
                    case "-i":
                        frameFolder = args[argIdx + 1];
                        if (!Directory.Exists(frameFolder)) Directory.CreateDirectory(frameFolder);
                        argIdx++;
                        break;
                    case "/f":
                    case "-f":
                        interpolationFrames = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/d":
                    case "-d":
                        frameDelay = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/l":
                    case "-l":
                        repeats = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/c":
                    case "-c":
                        capstoneFrames = int.Parse(args[argIdx + 1]);
                        argIdx++;
                        break;
                    case "/B":
                    case "-B":
                        interpolationMode = InterpolationTypes.Brightness;
                        break;
                    case "/R":
                    case "-R":
                        interpolationMode = InterpolationTypes.Random;
                        break;
                    case "/r":
                    case "-r":
                        string res = args[argIdx + 1];
                        argIdx++;
                        int width = int.Parse(res.Substring(0, res.IndexOf("x")));
                        int height = int.Parse(res.Substring(res.IndexOf("x") + 1));
                        resolution = new Size(width, height);
                        break;
                    case "/?":
                    case "-?":
                        PrintDocs();
                        break;
                    case "/7":
                    case "-7":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    default:
                        if (File.Exists(args[argIdx])) {
                            imgPaths.Add(args[argIdx]);
                        } else {
                            Console.WriteLine($"File {args[argIdx]} doesn\'t exist, ignoring");
                        }
                        break;
                }
            }

            if (imgPaths.Count < 2) {
                Console.WriteLine("Not enough input images to make a WyMerge from");
                return;
            }

            List<Bitmap> frames = new List<Bitmap>(imgPaths.Count * interpolationFrames);
            if (!resolution.HasValue) {
                Bitmap firstImage = (Bitmap)Bitmap.FromFile(imgPaths.First());
                Size res = firstImage.Size;
                resolution = new Size(res.Width, res.Height);
            }
            for (int imgIdx = 0; imgIdx < imgPaths.Count - 1; imgIdx++) {
                Bitmap from = (Bitmap)Bitmap.FromFile(imgPaths[imgIdx]);
                Bitmap to = (Bitmap)Bitmap.FromFile(imgPaths[imgIdx + 1]);

                from = new Bitmap(from, resolution.Value);
                to = new Bitmap(to, resolution.Value);


                List<Bitmap> sectFrames = WyMerge(from, to, interpolationFrames, interpolationMode);
                foreach(Bitmap frame in sectFrames) {
                    frames.Add(frame);
                }
            }

            GifWriter gif = new GifWriter(outputLocation, frameDelay, repeats);
            for (int c = 0; c < capstoneFrames; c++) {
                gif.WriteFrame(frames.First());
            }
            foreach(Bitmap frame in frames) {
                gif.WriteFrame(frame);
                if (frameFolder != "") frame.Save($"{frameFolder}/frame_{frame.GetHashCode()}");
            }
            for (int c = 0; c < capstoneFrames - 1; c++) {
                gif.WriteFrame(frames.Last());
            }
            gif.Dispose();
            Console.ResetColor();
        }

        /// <summary>
        /// Serializes an image into a 1D array
        /// </summary>
        /// <param name="image">The image to serialize</param>
        /// <returns>Array of <c>Pixel</c>s representing the image in reading order</returns>
        static Pixel[] SerializeImageToArray(Bitmap image) {
            List<Pixel> serialized = new List<Pixel>(image.Width * image.Height);

            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    serialized.Add(new Pixel(image.GetPixel(x, y), x, y));
                }
            }

            return serialized.ToArray();
        }

        static List<Bitmap> WyMerge(Bitmap fromImg, Bitmap toImg, int interpFrames, InterpolationTypes interpType) {
            List<Bitmap> frames = new List<Bitmap>(interpFrames + 1);

            var fromSerialized = (from p in SerializeImageToArray(fromImg)
                                 orderby p.Composite(interpType)
                                 select p).ToList();

            var toSerialized = (from p in SerializeImageToArray(toImg)
                               orderby p.Composite(interpType)
                               select p).ToList();

            for (int f = 0; f < interpFrames; f++) {
                Bitmap currentFrame = new Bitmap(fromImg.Width, fromImg.Height);
                for (int currentPixel = 0; currentPixel < fromSerialized.Count(); currentPixel++) {
                    Pixel newPixel = Pixel.Interpolate(
                        fromSerialized[currentPixel],
                        toSerialized[currentPixel],
                        f / (float)interpFrames
                    );
                    currentFrame.SetPixel((int)newPixel.position.X, (int)newPixel.position.Y, newPixel.color);
                }
                frames.Add(currentFrame);
            }

            frames.Add(toImg);
            return frames;
        }
    }
}
>>>>>>> 59b946e... Fix minor bug in frame export
