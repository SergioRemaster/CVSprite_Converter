using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CVSprite_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Process argument parameters
            //Show help
            if (args.Length == 0 || args[0] == "-h")
                ShowHelp();

            //Load input folder
            string[] folders = { args[0] };
            
            //-m option to load subdirectories
            if (args.Length >= 4 && args[3] == "-m")
                folders = Directory.GetDirectories(args[0]);
            
            //Do conversion for each folder
            foreach (string folder in folders)
            {
                //Determine main palette and set palette extract mode with -a
                int pal_num = Int32.Parse(args[1]) - 1;
                bool all_pal = false;

                if (args.Length >= 3)
                    all_pal = args[2] == "-a";

                if (!Directory.Exists(args[0]))
                {
                    Console.WriteLine("Folder doesn't exist!");
                    ShowHelp();
                }

                //List all BMPs of current folder
                string pal_file = "";
                List<byte[]> palette = new List<byte[]>();
                List<string> files = Directory.GetFiles(folder, "*.bmp").ToList();


                //Exclude palette.bmp and repeated images from AssetStudio
                for (int x = 0; x < files.Count; ++x)
                {
                    if (files[x].Contains("_palette.bmp") || files[x].Contains(" #"))
                    {
                        if (files[x].Contains("_palette.bmp"))
                            pal_file = files[x];
                        files.RemoveAt(x);
                        --x;
                    }
                }

                //Start conversion of files
                Console.WriteLine("Start reading from " + folder +
                    "\nNumber of files: " + files.Count);

                //Create indexed palette reading bytes of image
                using (BinaryReader reader = new BinaryReader(File.Open(pal_file, FileMode.Open)))
                {
                    //Get image width. Is the number of palettes
                    reader.BaseStream.Seek(22, SeekOrigin.Begin);
                    int number = reader.ReadInt32();

                    //Get palettes reading bytes
                    reader.BaseStream.Seek(122, SeekOrigin.Begin);
                    for (int x = 0; x < number; ++x)
                        //4 (n of channels per color) * 256 (n of colors)
                        palette.Add(reader.ReadBytes(4 * 256));
                }

                //Check palette is out of bounds
                if (pal_num > palette.Count - 1 || pal_num < 0)
                {
                    Console.WriteLine("Palette color out of bounds!");
                    ShowHelp();
                }

                //Create image
                foreach (string file in files)
                {
                    //Get file name and create output folder
                    Console.WriteLine("Reading file " + Path.GetFileName(file));
                    Directory.CreateDirectory(folder + "/output/");

                    //Save conversion as png inside output folder
                    string pngname = Path.GetFileNameWithoutExtension(file) + ".png";
                    File.WriteAllBytes(folder + "/output/" + pngname,
                        CVImageHandler.CreatePNGImage(palette[pal_num], file));

                    //Save all palettes using first image as base if mode was enabled
                    if (all_pal)
                    {
                        Directory.CreateDirectory(folder + "/output/Palettes/");
                        for (int x = 0; x < palette.Count; ++x)
                        {
                            //Skip main palette for extract since it's already used
                            if (x == pal_num)
                                continue;
                            File.WriteAllBytes(folder + "/output/Palettes/" + (x + 1) + ".png",
                                CVImageHandler.CreatePNGImage(palette[x], file));
                        }

                        //Disable mode since it was already extracted for this folder
                        all_pal = false;
                    }
                }
                Console.WriteLine("All done. Result saved at " + folder + "\\output\\");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("cvsprite_converter \"images_folder_path\" \"palette_number\" [-a] [-m]");
            Console.WriteLine("\tWrite your character folder path that contains the BMP images and palette. Be sure both are on the same folder.");
            Console.WriteLine("\tThis only support unindexed BMP images. Use AssetStudio tool to get sprites.");
            Console.WriteLine("\tOn palette number, write the number of palette to use for conversion. The first index is 1.");
            Console.WriteLine("\tUse -a to generate a palette folder in output folder, which contains an image for each palette the character has.");
            Console.WriteLine("\tUse -m to read and extract all sprites from all folders inside the input folder.");
            System.Environment.Exit(1);
        }
    }
}
