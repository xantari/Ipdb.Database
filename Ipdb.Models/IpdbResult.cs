using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ipdb.Models
{
    public class IpdbResult
    {
        public IpdbResult()
        {
            //Don't new these up, otherwise blank arrays will get created if you re-save the file
            //RuleSheetUrls = new List<IpdbUrl>();
            //ROMs = new List<IpdbUrl>();
            //Documentation = new List<IpdbUrl>();
            //ServiceBulletins = new List<IpdbUrl>();
            //Files = new List<IpdbUrl>();
            //MultimediaFiles = new List<IpdbUrl>();
            //ImageFiles = new List<IpdbUrl>();
        }

        public int IpdbId { get; set; }
        public string Title { get; set; }
        public int? Players { get; set; }
        public string AdditionalDetails { get; set; }
        public decimal? AverageFunRating { get; set; }
        public string Manufacturer { get; set; }
        /// <summary>
        /// Manufacturer short trade name
        /// </summary>
        public string ManufacturerShortName { get; set; }
        public int ManufacturerId { get; set; }
        public string CommonAbbreviations { get; set; }
        public string Type { get; set; }
        [JsonIgnore]
        public IpdbSystemType SystemType
        {
            get { return IpdbSystemTypeInfo.GetSystemTypeFromString(Type); }
        }

        public string TypeShortName
        {
            get { return IpdbSystemTypeInfo.GetTypeShortName(Type); }
        }
        public string MPU { get; set; }
        public DateTime? DateOfManufacture { get; set; }
        public string ModelNumber { get; set; }
        public int? ProductionNumber { get; set; }
        public string Theme { get; set; }
        public string NotableFeatures { get; set; }
        public string Toys { get; set; }
        public string DesignBy { get; set; }
        public string ArtBy { get; set; }
        public string DotsAnimationBy { get; set; }
        public string MechanicsBy { get; set; }
        public string MusicBy { get; set; }
        public string SoundBy { get; set; }
        public string SoftwareBy { get; set; }
        public string Notes { get; set; }
        public string MarketingSlogans { get; set; }
        public string PhotosIn { get; set; }
        public string Source { get; set; }
        public List<IpdbUrl> RuleSheetUrls { get; set; }
        public List<IpdbUrl> ROMs { get; set; }
        public List<IpdbUrl> Documentation { get; set; }
        public List<IpdbUrl> ServiceBulletins { get; set; }
        public List<IpdbUrl> Files { get; set; }
        public List<IpdbUrl> MultimediaFiles { get; set; }
        public List<IpdbUrl> ImageFiles { get; set; }
    }
}
