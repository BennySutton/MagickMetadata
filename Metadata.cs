using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// A wrapper class for ImageMagick that gets the important stuff from an image file's metadata so that you can do neat sh*t with it!
/// </summary>
/// <remarks> free to use under creative commons licence https://www.bennysutton.com/ </remarks>
namespace Ben.Classes
{
    public class MagickMetadata : IDisposable
    {
        private MagickImage _image;
        private string _keywords;

        public MagickMetadata(string rootpath)
        {
            if (!rootpath.IsImage()) return;
            _image = new MagickImage(rootpath.PhysicalPathFromRootPath());
            Init();
        }

        public MagickMetadata(MagickImage image)
        {
            _image = image;
            Init();
        }

        /// <summary>
        /// persist existing metadata values (if any) to property values
        /// </summary>
        private void Init()
        {
            IExifProfile exifprofile = _image.GetExifProfile();
            if (!exifprofile.IsNull())
            {
                if (exifprofile.GetValue(ExifTag.Copyright) != null) { Copyright = exifprofile.GetValue(ExifTag.Copyright).ToString(); }
                if (exifprofile.GetValue(ExifTag.Artist) != null) { Creator = exifprofile.GetValue(ExifTag.Artist).ToString(); }
                if (exifprofile.GetValue(ExifTag.ImageDescription) != null) { Subject = exifprofile.GetValue(ExifTag.ImageDescription).ToString(); }
                if (exifprofile.GetValue(ExifTag.Software) != null) { Software = exifprofile.GetValue(ExifTag.Software).ToString(); }
            }
            IIptcProfile iptcprofile = _image.GetIptcProfile();
            if (!iptcprofile.IsNull())
            {
                if (iptcprofile.GetValue(IptcTag.Country) != null) { Country = iptcprofile.GetValue(IptcTag.Country).ToString(); }
                if (iptcprofile.GetValue(IptcTag.Headline) != null) { Headline = iptcprofile.GetValue(IptcTag.Headline).ToString(); }
                if (iptcprofile.GetValue(IptcTag.Keyword) != null) { Keywords = iptcprofile.GetValue(IptcTag.Keyword).ToString(); }
                if (iptcprofile.GetValue(IptcTag.Source) != null) { Source = iptcprofile.GetValue(IptcTag.Source).ToString(); }
                if (iptcprofile.GetValue(IptcTag.Caption) != null) { Subject = iptcprofile.GetValue(IptcTag.Caption).ToString(); }
                if (iptcprofile.GetValue(IptcTag.Title) != null) { Title = iptcprofile.GetValue(IptcTag.Title).ToString(); }
            }
        }
        public bool Save(string rootpath)
        {
            bool success = false;

            ImageMagick.ExifProfile exifprofile = new ImageMagick.ExifProfile();
            exifprofile.SetValue(ExifTag.Copyright, " ©" + Copyright);
            exifprofile.SetValue(ExifTag.Artist, Creator);
            exifprofile.SetValue(ExifTag.ImageDescription, Subject);
            exifprofile.SetValue(ExifTag.Software, Software);
            _image.AddProfile(exifprofile);

            ImageMagick.IptcProfile iptcprofile = new ImageMagick.IptcProfile();
            iptcprofile.SetValue(IptcTag.CopyrightNotice, "No Unauthorized reproduction ©" + Copyright);
            iptcprofile.SetValue(IptcTag.Byline, Creator);
            iptcprofile.SetValue(IptcTag.Country, Country);
            iptcprofile.SetValue(IptcTag.Headline, Headline);
            iptcprofile.SetValue(IptcTag.Keyword, Keywords);
            iptcprofile.SetValue(IptcTag.Source, Source);
            iptcprofile.SetValue(IptcTag.Caption, Subject);
            iptcprofile.SetValue(IptcTag.Title, Title);
            _image.AddProfile(iptcprofile);

            _image.Write(rootpath);
            success = true;

            return success;
        }

        /// <summary>
        /// resets Keywords - when you add keywords with multiple calls to Keyword they are preserved so this is the only way to reset them - don't forget to call Save() afterwards
        /// </summary>
        public void ClearKeywords()
        {
            _keywords = "";
        }

        /// <summary>
        /// Allows you to loop through all the fields e.g. on a razor page
        /// </summary>
        /// <example>Ben.Classes.MagickMetadata magickMetadata = new Ben.Classes.MagickMetadata(filename);
        /// Dictionary<string, object> myDictionary = magickMetadata.ClassPropertiesToDictionary();</example>
        /// <returns>a dictionary of field names and values</returns>
        public Dictionary<string, object> ClassPropertiesToDictionary()
        {
            return this.GetType()
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
             .ToDictionary(prop => prop.Name, prop => prop.GetValue(this, null));
        }

        #region AutoImplementedProperties 

        /// <summary>
        /// used to show copyright notice NOTE the copyright symbol © gets automatically prepended
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// use to show the name of the person who took the photo and/or user who uploaded 
        /// </summary>
        public string Creator { get; set; }

        public string Country { get; set; }

        public string Headline { get; set; }

        /// <summary>
        /// Keywords NOTE if you call multiple times values will be appended not overwritten - call KeywordsClear() to reset 
        /// </summary>
        public string Keywords
        {
            get
            {
                return _keywords;
            }
            set
            {
                if (String.IsNullOrEmpty(_keywords))
                {
                    _keywords = value;
                }
                else
                {
                    _keywords = _keywords + " " + value;
                }
            }
        }

        public string Software { get; set; }

        public string Source { get; set; }

        public string Subject { get; set; }

        public string Title { get; set; }

        #endregion


        // DISPOSE
        public void Dispose()
        {
            _image.Dispose();
        }
    }
}