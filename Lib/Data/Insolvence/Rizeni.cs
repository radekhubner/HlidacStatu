﻿using System;
using System.Collections.Generic;

namespace HlidacStatu.Lib.Data.Insolvence
{
    public class Rizeni
        : Bookmark.IBookmarkable
    {
        public Rizeni()
        {
            Dokumenty = new List<Dokument>();
            Dluznici = new List<Osoba>();
            Veritele = new List<Osoba>();
            Spravci = new List<Osoba>();
        }

        [Nest.Keyword]
        public string SpisovaZnacka { get; set; }
        [Nest.Keyword]
        public string Stav { get; set; }
        [Nest.Date]
        public DateTime? Vyskrtnuto { get; set; }
        [Nest.Keyword]
        public string Url { get; set; }
        [Nest.Date]
        public DateTime? DatumZalozeni { get; set; }
        [Nest.Date]
        public DateTime PosledniZmena { get; set; }
        [Nest.Keyword]
        public string Soud { get; set; }
        [Nest.Object]
        public List<Dokument> Dokumenty { get; set; }
        [Nest.Object]
        public List<Osoba> Dluznici { get; set; }
        [Nest.Object]
        public List<Osoba> Veritele { get; set; }
        [Nest.Object]
        public List<Osoba> Spravci { get; set; }

        public string UrlId() => SpisovaZnacka.Replace(" ", "_").Replace("/", "-");




        public HlidacStatu.Lib.OCR.Api.CallbackData CallbackDataForOCRReq(int prilohaindex)
        {
            var url = Devmasters.Core.Util.Config.GetConfigValue("ESConnection");

            url = url + $"/{Lib.ES.Manager.defaultIndexName_Insolvence}/rizeni/{System.Net.WebUtility.UrlEncode(this.SpisovaZnacka)}/_update";

            string callback = HlidacStatu.Lib.OCR.Api.CallbackData.PrepareElasticCallbackDataForOCRReq($"dokumenty[{prilohaindex}].plainText", true);
            callback = callback.Replace("#ADDMORE#", $"ctx._source.dokumenty[{prilohaindex}].lastUpdate = '#NOW#';"
                + $"ctx._source.dokumenty[{prilohaindex}].lenght = #LENGTH#;"
                + $"ctx._source.dokumenty[{prilohaindex}].wordCount=#WORDCOUNT#;");

            return new HlidacStatu.Lib.OCR.Api.CallbackData(new Uri(url), callback, HlidacStatu.Lib.OCR.Api.CallbackData.CallbackType.LocalElastic);
        }

        public string GetUrl(bool local = true)
        {
            return GetUrl(local, string.Empty);
        }

        public string GetUrl(bool local, string foundWithQuery)
        {
            string url = $"/Insolvence/Rizeni/{this.UrlId()}";
            if (!string.IsNullOrEmpty(foundWithQuery))
                url = url + "?qs=" + System.Net.WebUtility.UrlEncode(foundWithQuery);
            if (local == false)
                url = "https://www.hlidacstatu.cz" + url;

            return url;
        }

        public string SoudFullName()
        {
            //zdroj dat: https://ispis.cz/soudy

            switch (this.Soud)
            {
                case "NS":
                    return "Nejvyšší soud";

                case "US":
                    return "Ústavní soud";

                case "NSS":
                    return "Nej. Správní soud";

                case "KSJIMBM":
                    return "Krajský soud v Brně";

                case "KSJICCB":
                    return "Krajský soud v Českých Budějovicích";

                case "KSVYCHK":
                case "KSVYCHKP1":
                    return "Krajský soud v Hradci Králové";

                case "KSVYCPA":
                    return "Krajský soud v Hradci Králové pobočka Pardubice";

                case "KSSEMOS":
                case "KSSEMOSP1":
                    return "Krajský soud v Ostravě";

                case "KSSEMOC":
                    return "Krajský soud v Ostravě pobočka Olomouc";

                case "KSZPCPM":
                    return "Krajský soud v Plzni";

                case "KSSTCAB":
                    return "Krajský soud v Praze";

                case "KSSCEUL":
                    return "Krajský soud v Ústí nad Labem";

                case "KSSCELB":
                case "KSSECULP1":
                    return "Krajský soud v Ústí nad Labem pobočka Liberec";

                case "MSPHAAB":
                    return "Městský soud v Praze";

                case "VSSTCAB":
                    return "Vrchní soud v Praze";

                case "VSSEMOC":
                    return "Vrchní soud v Olomouci";

                case "OSJIMBM":
                    return "Městský soud v Brně";

                case "OSPHA01":
                    return "Obvodní soud pro Prahu 1";

                case "OSPHA02":
                    return "Obvodní soud pro Prahu 2";

                case "OSPHA03":
                    return "Obvodní soud pro Prahu 3";

                case "OSPHA04":
                    return "Obvodní soud pro Prahu 4";

                case "OSPHA05":
                    return "Obvodní soud pro Prahu 5";

                case "OSPHA06":
                    return "Obvodní soud pro Prahu 6";

                case "OSPHA07":
                    return "Obvodní soud pro Prahu 7";

                case "OSPHA08":
                    return "Obvodní soud pro Prahu 8";

                case "OSPHA09":
                    return "Obvodní soud pro Prahu 9";

                case "OSPHA10":
                    return "Obvodní soud pro Prahu 10";

                case "OSSTCBN":
                    return "Okresní soud v Benešově";

                case "OSSTCBE":
                    return "Okresní soud v Berouně";

                case "OSJIMBK":
                    return "Okresní soud v Blansku";

                case "OSJIMBO":
                    return "Okresní soud Brno-Venkov";

                case "OSSEMBR":
                    return "Okresní soud v Bruntále";

                case "OSSEMKR":
                    return "Okresní soud v Bruntále - poboèka Krnov";

                case "OSJIMBV":
                    return "Okresní soud v Břeclavi";

                case "OSSCECL":
                    return "Okresní soud v České Lípě";

                case "OSJICCK":
                    return "Okresní soud v Českém Krumlově";

                case "OSJICCB":
                    return "Okresní soud v Českých Budějovicích";

                case "OSZPCCH":
                    return "Okresní soud v Chebu";

                case "OSSCECV":
                    return "Okresní soud v Chomutově";

                case "OSVYCCR":
                    return "Okresní soud v Chrudimi";

                case "OSSCEDC":
                    return "Okresní soud v Děčíně";

                case "OSZPCDO":
                    return "Okresní soud v Domažlicích";

                case "OSSEMFM":
                    return "Okresní soud ve Frýdku-Místku";

                case "OSVYCHB":
                    return "Okresní soud v Havlíčkovì Brodě";

                case "OSJIMHO":
                    return "Okresní soud v Hodoníně";

                case "OSVYCHK":
                    return "Okresní soud v Hradci Králové";

                case "OSSCEJN":
                    return "Okresní soud v Jablonci nad Nisou";

                case "OSSEMJE":
                    return "Okresní soud v Jeseníku";

                case "OSJIMJI":
                    return "Okresní soud Jihlava";

                case "OSVYCJC":
                    return "Okresní soud v Jičíně";

                case "OSJICJH":
                    return "Okresní soud v Jindřichově Hradci";

                case "OSZPCKV":
                    return "Okresní soud v Karlových Varech";

                case "OSSEMKA":
                    return "Okresní soud v Karviné";

                case "OSSEMHA":
                    return "Okresní soud v Karviné pobočka Havířov";

                case "OSSTCKL":
                    return "Okresní soud v Kladně";

                case "OSZPCKT":
                    return "Okresní soud v Klatovech";

                case "OSSTCKO":
                    return "Okresní soud v Kolíně";

                case "OSJIMKM":
                    return "Okresní soud v Kroměříži";

                case "OSSTCKH":
                    return "Okresní soud v Kutné Hoře";

                case "OSSCELB":
                    return "Okresní soud v Liberci";

                case "OSSCELT":
                    return "Okresní soud v Litoměřicích";

                case "OSSCELN":
                    return "Okresní soud v Lounech";

                case "OSSTCME":
                    return "Okresní soud v Mělníku";

                case "OSSTCMB":
                    return "Okresní soud v Mladé Boleslavi";

                case "OSSCEMO":
                    return "Okresní soud v Mostě";

                case "OSVYCNA":
                    return "Okresní soud v Náchodě";

                case "OSSEMNJ":
                    return "Okresní soud v Novém Jičíně";

                case "OSSTCNB":
                    return "Okresní soud v Nymburce";

                case "OSSEMOC":
                    return "Okresní soud v Olomouci";

                case "OSSEMOP":
                    return "Okresní soud v Opavě";

                case "OSSEMOS":
                    return "Okresní soud v Ostravě";

                case "OSVYCPA":
                    return "Okresní soud v Pardubicích";

                case "OSJICPE":
                    return "Okresní soud v Pelhřimově";

                case "OSJICPI":
                    return "Okresní soud v Písku";

                case "OSSEMPR":
                    return "Okresní soud v Přerově";

                case "OSSTCPB":
                    return "Okresní soud v Příbrami";

                case "OSZPCPJ":
                    return "Okresní soud Plzeň-jih";

                case "OSZPCPM":
                    return "Okresní soud Plzeň-Město";

                case "OSZPCPS":
                    return "Okresní soud Plzeň-sever";

                case "OSJICPT":
                    return "Okresní soud Prachatice";

                case "OSSTCPY":
                    return "Okresní soud Praha-východ";

                case "OSSTCPZ":
                    return "Okresní soud Praha-západ";

                case "OSJIMPV":
                    return "Okresní soud v Prostějově";

                case "OSSTCRA":
                    return "Okresní soud v Rakovníku";

                case "OSZPCRO":
                    return "Okresní soud v Rokycanech";

                case "OSVYCRK":
                    return "Okresní soud v Rychnově nad Kněžnou";

                case "OSVYCSM":
                    return "Okresní soud v Semilech";

                case "OSZPCSO":
                    return "Okresní soud v Sokolově";

                case "OSJICST":
                    return "Okresní soud ve Strakonicích";

                case "OSSEMSU":
                    return "Okresní soud v Šumperku";

                case "OSJICTA":
                    return "Okresní soud v Táboře";

                case "OSZPCTC":
                    return "Okresní soud v Tachově";

                case "OSSCETP":
                    return "Okresní soud v Teplicích";

                case "OSJIMTR":
                    return "Okresní soud v Třebíči";

                case "OSVYCTU":
                    return "Okresní soud v Trutnově";

                case "OSJIMUH":
                    return "Okresní soud v Uherském Hradišti";

                case "OSSCEUL":
                    return "Okresní soud v Ústí nad Labem";

                case "OSVYCUO":
                    return "Okresní soud v Ústí nad Orlicí";

                case "OSVYCSY":
                    return "Okresní soud ve Svitavách";

                case "OSSEMVS":
                    return "Okresní soud ve Vsetíně";

                case "OSSEMVM":
                    return "Okresní soud ve Vsetíně pobočka Valašské Meziřičí";

                case "OSJIMVY":
                    return "Okresní soud ve Vyškově";

                case "OSJIMZR":
                    return "Okresní soud ve Žďáru nad Sázavou";

                case "OSJIMZL":
                    return "Okresní soud ve Zlíně";

                case "OSJIMZN":
                    return "Okresní soud ve Znojmě";

                default:

                    return "@Model.Soud";
            }

        }

        public string StavRizeni()
        {

            switch (this.Stav)
            {
                case Insolvence.StavRizeni.Nevyrizena:
                    return "Nevyřízená";
                    
                case Insolvence.StavRizeni.Moratorium:
                    return "Moratorium";
                    
                case Insolvence.StavRizeni.Upadek:
                    return "Úpadek";
                    
                case Insolvence.StavRizeni.Konkurs:
                    return "Konkurs";
                    
                case Insolvence.StavRizeni.Oddluzeni:
                    return "Oddlužení";
                    
                case Insolvence.StavRizeni.Reorganizace:
                    return "Reorganizace";
                    
                case Insolvence.StavRizeni.Vyrizena:
                    return "Vyřízená";
                    
                case Insolvence.StavRizeni.Pravomocna:
                    return "Pravomocná";
                    
                case Insolvence.StavRizeni.Odskrtnuta:
                    return "Odškrtnutá";
                    
                case Insolvence.StavRizeni.Zruseno:
                    return "Zrušeno vrchním soudem";
                    
                case Insolvence.StavRizeni.KonkursPoZruseni:
                    return "Konkurs po zrušení";
                    
                case Insolvence.StavRizeni.Obzivla:
                    return "Obživlá";
                    
                case Insolvence.StavRizeni.MylnyZapis:
                    return "Mylný zápis";
                    
                case Insolvence.StavRizeni.PostoupenaVec:
                    return "Postoupená věc";
                    
                default:
                    return this.Stav;
                    
            }

        }
        public string StavRizeniDetail()
        {
            switch (this.Stav)
            {
                case Insolvence.StavRizeni.Nevyrizena:
                    return "Před rozhodnutím o úpadku";
                    
                case Insolvence.StavRizeni.Moratorium:
                    return "Povoleno moratorium";
                    
                case Insolvence.StavRizeni.Upadek:
                    return "V úpadku";
                    
                case Insolvence.StavRizeni.Konkurs:
                    return "Prohlášený konkurs";
                    
                case Insolvence.StavRizeni.Oddluzeni:
                    return "Povoleno oddlužení";
                    
                case Insolvence.StavRizeni.Reorganizace:
                    return "Povolena reorganizace";
                    
                case Insolvence.StavRizeni.Vyrizena:
                    return "Vyřízená věc";
                    
                case Insolvence.StavRizeni.Pravomocna:
                    return "Pravomocně skončená věc";
                    
                case Insolvence.StavRizeni.Odskrtnuta:
                    return "Odškrtnutá - skončená věc";
                    
                case Insolvence.StavRizeni.Zruseno:
                    return "Zrušeno vrchním soudem";
                    
                case Insolvence.StavRizeni.KonkursPoZruseni:
                    return "Prohlášený konkurs po zrušení VS";
                    
                case Insolvence.StavRizeni.Obzivla:
                    return "Obživlá věc";
                    
                case Insolvence.StavRizeni.MylnyZapis:
                    return "Mylný zápis do rejstříku";
                    
                case Insolvence.StavRizeni.PostoupenaVec:
                    return "Postoupená věc";
                    
                default:
                    return this.Stav;

        
            }
        }


            public string BookmarkName()
        {
            return "Insolvence " + this.SpisovaZnacka;
        }

        public string ToAuditJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public string ToAuditObjectTypeName()
        {
            return "Insolvence";
        }

        public string ToAuditObjectId()
        {
            return this.SpisovaZnacka;
        }
    }
}
