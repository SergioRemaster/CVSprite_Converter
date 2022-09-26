# CVSprite_Converter
Tool to convert sprites extracted from Dengeki Bunko Crossing Void using proper colors from palettes.

Requirements
---------
<ul>
<li>.NET Core Runtime 3.1 (for source code only AFAIK. VS should ask you to install it when you open the solution.)</li>
<li>Files decompiled using AssetStudio (Sprites and Palette <b>MUST BE</b> extracted in BMP!!): https://github.com/Perfare/AssetStudio</li>
</ul>

Installation
---------
<p>For binaries (compiled version), go to release page, and unzip compiled project: https://github.com/SergioRemaster/CVSprite_Converter/releases</p>
<p>For source code, just download or clone it, and open the VS solution file.</p>

Usage
---------
cvsprite_converter "images_folder_path" "palette_number" [-a] [-m]
<ul>
<li>Write your character folder path that contains the BMP images and palette. Be sure both are on the same folder.</li>
<li>This only support unindexed BMP images. Use AssetStudio tool to get sprites.</li>
<li>On palette number, write the number of palette to use for conversion. The first index is 1.</li>
<li>Use <b>-a</b> to generate a palette folder in output folder, which contains an image for each palette the character has.</li>
<li>Use <b>-m</b> to read and extract all sprites from all folders inside the input folder.</li>
</ul>
<p>I included bat files with examples of use on the released version.</p>
