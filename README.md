# MagickMetadata
Read and write image metadata (IPTC and Exif) using ImageMagick in ASP.NET

ImageMagick issues I have seen on stackoverflow.com and other places include wiping existing profiles and losing existing keywords (if you want to add some more keywords). So my class MagickMetadata solves both those problems.
MagickMetadata doesn't preserve all metadata fields, if you want more you can adapt it. All I will say as the author of photo contests and stock photo libraries is YAGNI (you ain't going to need it).
All the important metadata fields, the ones that show up in Windows Explorer when you right click properties, are there. If you use Adobe bridge to search a folder by keywords, that field is there.
