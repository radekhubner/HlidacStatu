﻿    using System;

namespace HlidacStatu.Lib.Data.External.DataSets
{
    
    public partial class Registration
    {
        public string id { get { return datasetId; } }
        public string name { get; set; }
        public string datasetId { get; set; }
        public string origUrl { get; set; }
        public string sourcecodeUrl { get; set; }
        public string description { get; set; }
        public string jsonSchema { get; set; }

        Newtonsoft.Json.Schema.JSchema _schema = null;
        public Newtonsoft.Json.Schema.JSchema GetSchema()
        {
            if (_schema == null)
            {
                _schema = Newtonsoft.Json.Schema.JSchema.Parse(jsonSchema);
            }

            return _schema;
        }


        //public string registration_elasticmapping { get; set; } = null;
        public string createdBy { get; set; }
        public System.DateTime created { get; set; } = DateTime.Now;

        public bool betaversion { get; set; } = false;
        public bool allowWriteAccess { get; set; } = false;

        public bool hidden { get; set; } = false;

        public Template searchResultTemplate { get; set; }
        public Template detailTemplate { get; set; }

        public const string DbCreatedLabel = "Datumu importu do db";
        public string[,] orderList { get; set; } = new string[,] { { DbCreatedLabel , "DbCreated" } };

        public void NormalizeShortName()
        {
            char[] invalidchars = @" #,\/*?""<>|_\+.[]".ToCharArray();
            string invalidregex = @"[" + String.Join("", invalidchars) + "]*";

            string shortName = this.datasetId;
            if (string.IsNullOrEmpty(shortName))
                shortName = this.name;
            if (string.IsNullOrEmpty(shortName))
                return;

            shortName = Devmasters.Core.TextUtil.RemoveDiacritics(shortName).Trim();

            shortName = System.Text.RegularExpressions.Regex.Replace(shortName, invalidregex, "-").ToLower();
            shortName = Devmasters.Core.TextUtil.ReplaceDuplicates(shortName, "-");
            shortName = System.Text.RegularExpressions.Regex.Replace(shortName, "^(-)*", "");
            shortName = System.Text.RegularExpressions.Regex.Replace(shortName, "(-)*$", "");


            if (shortName.Length > 120)
                shortName = shortName.Substring(0, 120);
            if (shortName.Length < 5)
                shortName = shortName + "-" + Devmasters.Core.TextUtil.GenRandomString(10);
            this.datasetId = shortName;
        }


        public DataSet GetDataset()
        {
            if (string.IsNullOrEmpty(this.datasetId))
                return null;
            else
                return DataSet.CachedDatasets.Get(this.datasetId);
        }

    }

}
