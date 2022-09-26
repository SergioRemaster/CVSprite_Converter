using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;

namespace CVSprite_Converter
{
    public static class CVImageHandler
    {
        /// <summary>
        /// Generate indexed BMP from given image data.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="img">Image byte data.</param>
        /// <param name="palette">Image palette. Always 256 colors.</param>
        /// <returns>Byte array of generated indexed BMP.</returns>
        public static byte[] WriteIndexedBMP(int width, int height, byte[] img, byte[] palette)
        {
            List<byte> file = new List<byte>();

            //Write BMP header
            //bmp identifier
            file.AddRange(Encoding.ASCII.GetBytes("\x42\x04d"));
            //Image size + Palette size
            file.AddRange(BitConverter.GetBytes(img.Length + 1078));
            //Bytes reserved
            file.AddRange(BitConverter.GetBytes(0));
            //Palette size
            file.AddRange(BitConverter.GetBytes(1078));
            //Always 0x40
            file.AddRange(BitConverter.GetBytes(40)); 
            //Image width and height
            file.AddRange(BitConverter.GetBytes(width));
            file.AddRange(BitConverter.GetBytes(height));
            //bits per pixel and pixel related info
            file.AddRange(BitConverter.GetBytes((short)1));
            file.AddRange(BitConverter.GetBytes((short)8)); 
            file.AddRange(BitConverter.GetBytes(0));
            //Image data size
            file.AddRange(BitConverter.GetBytes(img.Length));
            //Reserved
            file.AddRange(BitConverter.GetBytes(0));
            file.AddRange(BitConverter.GetBytes(0));
            file.AddRange(BitConverter.GetBytes(0));
            file.AddRange(BitConverter.GetBytes(0));

            //Write BMP data
            //Write palette bytes
            file.AddRange(palette);
            //Write image data bytes
            file.AddRange(img);

            return file.ToArray();
        }

        /// <summary>
        /// Generate PNG from an unindexed BMP.
        /// </summary>
        /// <param name="palette">Palette to use on this image.</param>
        /// <param name="inputFile">Unindexed BMP file to read data from.</param>
        /// <returns>Byte array of converted PNG data.</returns>
        public static byte[] CreatePNGImage(byte[] palette, string inputFile)
        {
            //Start creating indexed BMP
            byte[] indexed;
            using (BinaryReader reader = new BinaryReader(File.Open(inputFile, FileMode.Open)))
            {
                //Get BMP size, width, height, and image data
                reader.BaseStream.Seek(2, SeekOrigin.Begin);
                int size = reader.ReadInt32() - 54;
                reader.BaseStream.Seek(18, SeekOrigin.Begin);
                int width = reader.ReadInt32();
                reader.BaseStream.Seek(22, SeekOrigin.Begin);
                int height = reader.ReadInt32();

                //Calculate padding of current image
                int padding = width % 4;

                //Get image data
                List<byte> data = new List<byte>();
                reader.BaseStream.Seek(122, SeekOrigin.Begin);
                int count = 0;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    //skip unused channels
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    //read color byte
                    byte index = reader.ReadByte();
                    //skip another unused channel
                    reader.BaseStream.Seek(1, SeekOrigin.Current);
                    //Add color and add +1 to count for padding calculation
                    data.Add(index);
                    count++;
                    if (count == width && padding != 0)
                    {
                        for (int y = 0; y < 4 - padding; ++y) data.Add(0);
                        count = 0;
                    }
                }
                //Add two 0 bytes to end file
                data.Add(0);
                data.Add(0);

                //Generate indexed BMP from current one
                indexed = WriteIndexedBMP(width, height, data.ToArray(), palette);
            }

            //Create PNG
            List<byte> newpng;
            using (Bitmap indexedBmp = new Bitmap(new MemoryStream(indexed)))
            {
                using (MemoryStream png = new MemoryStream())
                {
                    indexedBmp.Save(png, ImageFormat.Png);
                    //test: indexedBmp.Save("test.bmp");
                    newpng = png.ToArray().ToList();
                }
            }

            //Insert tRNS parameter to create transparency index at 0 on palette table
            //WARNING: Use different index value will require different CRC value
            byte[] alpha = { 0, 0, 0, 1, 116, 82, 78, 83, 0, 64, 230, 216, 102 };
            newpng.InsertRange(842, alpha);

            return newpng.ToArray();
        }
    }
}
