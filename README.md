![out](https://user-images.githubusercontent.com/19637928/122663895-ba2ab900-d16b-11eb-89ec-5ee0cc63da45.gif)

# WyMerge2
A better, more efficient implementation of the WyMerge algorithm.

## What is WyMerge?
WyMerge is an algorithm for transitioning between images. It works by pairing two similar pixels between images and linearly interpolating between the two, effectively re-assembling the first image into the second.

## What's new in this implementation of WyMerge?
The old WyMerge was *super* slow. A 64x64 WyMerge would take up to a few seconds to generate, and you really couldn't do anything above 100x100. That was because the program generated "pixel relations" iteratively for every pixel, which took up tons of memory and processing time. In theory, this would have been O((x\*y)²).  In WyMerge2, instead of generating pixel relations, both images are sorted and the same indices are used. Due to the efficiency of sorting algorithms being far better than O(n²), WyMerge can run much smoother and generate much larger animations than it used to.

## Anything else?
I originally wrote WyMerge in C# using the .NET Framework. This ultimately meant that unless I recompiled with something like Mono, cross-platform was generally impossible. However, for WyMerge2, I have decided to write the thing using .NET Core over .NET Framework, which *does* advocate for cross-platform capability.

Aside from those major changes, there are also some other useful features and command-line args that you can use to make your life simpler. Non-BMP support and explicit sizing are just some of those features.

# General Tips
   - This is a command-line tool.
   - Provide a negative number to the loops parameter to have it not loop whatsoever, or 0 to have it loop indefinitely.
   - To have it ping-pong, provide the images again in reverse order.
   - Images can be in BMP, GIF, EXIF, JPG, PNG and TIFF formats.
