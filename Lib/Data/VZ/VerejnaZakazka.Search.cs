﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using HlidacStatu.Util;
using HlidacStatu.Util.Cache;
using HtmlAgilityPack;
using Nest;

namespace HlidacStatu.Lib.Data.VZ
{
    public partial class VerejnaZakazka
    {
        //https://hooks.slack.com/services/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
        public static class Searching
        {
            static string regex = "[^/]*\r\n/(?<regex>[^/]*)/\r\n[^/]*\r\n";
            static System.Text.RegularExpressions.RegexOptions options = ((System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace | System.Text.RegularExpressions.RegexOptions.Multiline)
                        | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            static System.Text.RegularExpressions.Regex regFindRegex = new System.Text.RegularExpressions.Regex(regex, options);
            static RegexOptions regexQueryOption = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline;



            [Devmasters.Core.ShowNiceDisplayName]
            [Devmasters.Core.Sortable(Devmasters.Core.SortableAttribute.SortAlgorithm.BySortValueAndThenAlphabetically)]
            public enum CPVSkupiny
            {
                [Devmasters.Core.NiceDisplayName("IT, HW, SW"), Devmasters.Core.SortValue(10)]
                IT = 1,
                [Devmasters.Core.NiceDisplayName("Stavebnictví"), Devmasters.Core.SortValue(10)]
                Stav = 2,
                [Devmasters.Core.NiceDisplayName("Doprava"), Devmasters.Core.SortValue(10)]
                Doprava = 3,
                [Devmasters.Core.NiceDisplayName("Strojírenské produkty"), Devmasters.Core.SortValue(10)]
                Stroje = 4,
                [Devmasters.Core.NiceDisplayName("Telekomunikace"), Devmasters.Core.SortValue(10)]
                Telco = 5,
                [Devmasters.Core.NiceDisplayName("Zdravotnictví, medicína"), Devmasters.Core.SortValue(10)]
                Zdrav = 6,
                [Devmasters.Core.NiceDisplayName("Potraviny"), Devmasters.Core.SortValue(10)]
                Jidlo = 7,
                [Devmasters.Core.NiceDisplayName("Bezpečnost, vojsko, policie"), Devmasters.Core.SortValue(10)]
                Bezpecnost = 8,
                [Devmasters.Core.NiceDisplayName("Přírodní zdroje"), Devmasters.Core.SortValue(10)]
                PrirodniZdroj = 9,
                [Devmasters.Core.NiceDisplayName("Energetika"), Devmasters.Core.SortValue(10)]
                Energie = 10,
                [Devmasters.Core.NiceDisplayName("Zemědělství a lesnictví"), Devmasters.Core.SortValue(10)]
                Agro = 11,
                [Devmasters.Core.NiceDisplayName("Kancelářské služby a materiál"), Devmasters.Core.SortValue(10)]
                Kancelar = 12,
                [Devmasters.Core.NiceDisplayName("Řemeslné služby a výrobky"), Devmasters.Core.SortValue(10)]
                Remeslo = 13,
                [Devmasters.Core.NiceDisplayName("Zdravotní, sociální a vzdělávací služby"), Devmasters.Core.SortValue(10)]
                Social = 14,
                [Devmasters.Core.NiceDisplayName("Finanční služby"), Devmasters.Core.SortValue(10)]
                Finance = 15,
                [Devmasters.Core.NiceDisplayName("Právnické služby"), Devmasters.Core.SortValue(10)]
                Legal = 16,
                [Devmasters.Core.NiceDisplayName("Technické služby	"), Devmasters.Core.SortValue(10)]
                TechSluzby = 17,
                [Devmasters.Core.NiceDisplayName("Výzkum"), Devmasters.Core.SortValue(10)]
                Vyzkum = 18,
                [Devmasters.Core.NiceDisplayName("Marketing & PR"), Devmasters.Core.SortValue(10)]
                Marketing = 20,
                [Devmasters.Core.NiceDisplayName("Ostatní"), Devmasters.Core.SortValue(99)]
                Jine = 19
            }

            //source 
            public static Dictionary<string, string> cpvSearchGroups = new Dictionary<string, string> {
                {"it", "302,72,64216,791211,48,50312,516,"},
                {"stav","44,45,71,75123,34946,351131,4331,433,436,507,51541,7011,79993,909112," }, //stavebnictvi
                {"doprava","34,60,63,09132,091342,0913423,09211,501,502,5114"}, //doprava
                {"stroje","16,31,38,42,43,505,515"}, //strojírenské produkty	
                {"telco","32,64,5033,513,"}, //telco
                {"zdrav","33,504,514,"}, //medicínské vybavení	
                {"jidlo","03,15,4111"}, //potraviny, 
                {"bezpecnost","35,506,5155,519,"}, //bezpecnost, vojsko, policie
                {"prirodnizdroj","14,24,41"},
                {"energie","09,65,3112,3113,3114,45251,71314,713231,"}, //energetika
                {"agro","16,77,5152"}, //zemedelstvi a lest
                {"kancelar","22,301,39,795,796,797,798,799,300,503"},//kancelářský materiál
                {"remeslo","18,19,374,375,378,373,3700"}, //oděvy, obuv a jiné vybavení
                {"social","80,85,92,98"}, //zdravotní, sociální a vzdělávací služby
                {"finance","66,792,794,"}, //financni služby
                {"legal","70,791,7524"}, //právní, poradenské a jiné komerční služby
                {"techsluzby","500,51,76,90"}, //technické služby
                {"vyzkum","73,79315,452146,45214,3897,3829,3012513"},
                {"marketing","7934,79341,793411,793412,793414,793415,79342,793421,793422,793423,7934231,79342311,7934232,79342321,794,79413,79416,794161"},
                {"jine","75,55,793,790,508"},

            };

            public static string[] CPVOblastToCPV(CPVSkupiny skupina)
            {
                var key = skupina.ToString().ToLower();
                if (cpvSearchGroups.ContainsKey(key))
                    return cpvSearchGroups[key].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                else
                    throw new ArgumentOutOfRangeException("CPVSkupinaToCPV failed for " + skupina);
            }

            public static string[] CPVOblastToCPV(string skupinaJmeno)
            {
                if (string.IsNullOrWhiteSpace(skupinaJmeno))
                    return null;
                if (Devmasters.Core.TextUtil.IsNumeric(skupinaJmeno))
                {
                    int iSkup;
                    if (int.TryParse(skupinaJmeno, out iSkup))
                    {
                        try
                        {
                            return CPVOblastToCPV((CPVSkupiny)iSkup);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
                var key = skupinaJmeno.ToLower();
                if (cpvSearchGroups.ContainsKey(key))
                    return cpvSearchGroups[key].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                else
                    return null;
            }

            public static string NormalizeOblastValue(string value)
            {
                if (String.IsNullOrWhiteSpace(value))
                    return string.Empty;
                if (Devmasters.Core.TextUtil.IsNumeric(value))
                {
                    int iSkup;
                    if (int.TryParse(value, out iSkup))
                    {
                        try
                        {
                            var skupina = (CPVSkupiny)iSkup;
                            return skupina.ToString();
                        }
                        catch (Exception)
                        {
                            return string.Empty;
                        }
                    }
                }
                return value;
            }

            public static QueryContainer GetRawQuery(string jsonQuery)
            {
                QueryContainer qc = null;
                if (string.IsNullOrEmpty(jsonQuery))
                    qc = new QueryContainerDescriptor<VerejnaZakazka>().MatchAll();
                else
                {
                    qc = new QueryContainerDescriptor<VerejnaZakazka>().Raw(jsonQuery);
                }

                return qc;

            }


            static string[] queryShorcuts = new string[] {
                "ico:",
                "icododavatel:",
                "icoprijmece:",
                "icozadavatel:",
                "icoplatce:",
                "jmenozadavatel:",
                "jmenododavatel:",
                "id:",
                "osobaid:",
                "cpv:",
                "form:",
                "zahajeny:",
                "popis:",
                "cena:",
                "zverejneno:",
                "podepsano:",
                "text:",
                "oblast:",
                "holding:"
            };
            static string[] queryOperators = new string[] {
            "AND","OR"
        };


            public static QueryContainer GetSimpleQuery(ES.VerejnaZakazkaSearchData searchdata)
            {

                var query = searchdata.Q?.Trim();
                //if (string.IsNullOrEmpty(query) || query == "*")
                //    return new QueryContainerDescriptor<VerejnaZakazka>().MatchAll();


                string regexPrefix = @"(^|\s|[(])";
                string regexTemplate = "{0}(?<q>(-|\\w)*)\\s*";
                //fix field prefixes
                //ds: -> 
                string[,] rules = new string[,] {
                    {@"osobaid:(?<q>((\w{1,} [-]{1} \w{1,})([-]{1} \d{1,3})?)) (\s|$){1,}","${ico}" },
                    {@"holding:(?<q>(\d{1,8})) (\s|$){1,}","${ico}" },
                    {"cpv:","${cpv}" },
                    {"oblast:","${oblast}" },
                    {"form:","${form}" },
                    {"zahajeny:1","stavVZ:<=100" },
                    {regexPrefix + "ico:","(zadavatel.iCO:${q} OR dodavatele.iCO:${q}) " },
                    {regexPrefix + "icododavatel:","dodavatele.iCO:" },
                    {regexPrefix + "icoprijemce:","dodavatele.iCO:" },
                    {regexPrefix + "icozadavatel:","zadavatel.iCO:" },
                    {regexPrefix + "icoplatce:","zadavatel.iCO:" },
                    {"jmenoprijemce:","dodavatele.jmeno:" },
                    {"jmenoplatce:","zadavatel.jmeno:" },
                    {regexPrefix + "id:","id:" },
                    {"popis:","(nazevZakazky:${q} OR popisZakazky:${q}) " },
                    {"cena:<=","(konecnaHodnotaBezDPH:<=${q} OR odhadovanaHodnotaBezDPH:<=${q}) " },
                    {"cena:>=","(konecnaHodnotaBezDPH:>=${q} OR odhadovanaHodnotaBezDPH:>=${q}) " },
                    {"cena:<","(konecnaHodnotaBezDPH:<${q} OR odhadovanaHodnotaBezDPH:<${q}) " },
                    {"cena:>","(konecnaHodnotaBezDPH:>${q} OR odhadovanaHodnotaBezDPH:>${q}) " },
                    {"cena:","(konecnaHodnotaBezDPH:${q} OR odhadovanaHodnotaBezDPH:${q}) " },
                    {"zverejneno:\\[","datumUverejneni:[" },
                    {"zverejneno:(?=[<>])","datumUverejneni:${q}" },
                    {"zverejneno:(?=\\d)","datumUverejneni:[${q} TO ${q}||+1d]" },
                    {"podepsano:\\[","datumUzavreniSmlouvy:[" },
                    {"podepsano:(?=[<>])","datumUzavreniSmlouvy:${q}" },
                    {"podepsano:(?=\\d)","datumUzavreniSmlouvy:[${q} TO ${q}||+1d]" },
                    {"text:","prilohy.plainTextContent:" },
                };


                string modifiedQ = query; // ES.SearchTools.FixInvalidQuery(query, queryShorcuts, queryOperators) ?? "";
                //check invalid query ( tag: missing value)


                if (searchdata.Zahajeny)
                    modifiedQ = Lib.ES.SearchTools.ModifyQuery(modifiedQ, "zahajeny:1");

                if (!string.IsNullOrWhiteSpace(searchdata.Oblast))
                {
                    var oblValue = NormalizeOblastValue(searchdata.Oblast);
                    if (!string.IsNullOrEmpty(oblValue))
                        modifiedQ = Lib.ES.SearchTools.ModifyQuery(modifiedQ, "oblast:" + oblValue);
                }


                for (int i = 0; i < rules.GetLength(0); i++)
                {
                    string lookFor = rules[i, 0];
                    string replaceWith = rules[i, 1];

                    MatchEvaluator evalMatch = (m) =>
                    {
                        var s = m.Value;
                        if (string.IsNullOrEmpty(s))
                            return string.Empty;
                        var newVal = replaceWith;
                        if (newVal.Contains("${q}"))
                            newVal = newVal.Replace("${q}", m.Groups["q"].Value);
                        if (s.StartsWith("("))
                            return " (" + newVal;
                        else
                            return " " + newVal;
                    };

                    //if (modifiedQ.ToLower().Contains(lookFor.ToLower()))
                    if (Regex.IsMatch(modifiedQ, lookFor, regexQueryOption))
                    {
                        if (replaceWith.Contains("${q}"))
                        {
                            modifiedQ = Regex.Replace(modifiedQ, string.Format(regexTemplate, lookFor), evalMatch, regexQueryOption);
                        } //do regex replace
                        //else if (replaceWith.Contains("${zahajeny}") && )
                        //{
                        //    modifiedQ = Regex.Replace(modifiedQ, lookFor, "stavVZ:<=100", regexQueryOption);
                        //} //do regex replace
                        else if (replaceWith.Contains("${oblast}"))
                        {
                            string cpv = "";
                            if (replaceWith.Contains("${oblast}"))
                            {
                                var oblastVal = ParseTools.GetRegexGroupValue(modifiedQ, @"oblast:(?<oblast>\w*)", "oblast");
                                var cpvs = CPVOblastToCPV(oblastVal);
                                if (cpvs != null)
                                {
                                    var q_cpv = "cPV:(" + cpvs.Select(s => s + "*").Aggregate((f, s) => f + " OR " + s) + ")";
                                    modifiedQ = Regex.Replace(modifiedQ, @"oblast:(?<oblast>\w*)", q_cpv, regexQueryOption);
                                }
                            }
                        }
                        else if (replaceWith.Contains("${cpv}"))
                        {
                            string cpv = "";
                            //Match m = Regex.Match(modifiedQ, lookFor, regexQueryOption);
                            //string cpv = "";
                            //if (m.Success)
                            //    cpv = m.Groups["q"].Value;
                            cpv = ParseTools.GetRegexGroupValue(modifiedQ, @"cpv:(?<q>(-|,|\d)*)\s*", "q");
                            lookFor = @"cpv:(?<q>(-|,|\d)*)\s*";
                            if (!string.IsNullOrEmpty(cpv))
                            {
                                string[] cpvs = cpv.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                                string q_cpv = "";
                                if (cpvs.Length > 0)
                                    q_cpv = "cPV:(" + cpvs.Select(s => s + "*").Aggregate((f, s) => f + " OR " + s) + ")";

                                modifiedQ = Regex.Replace(modifiedQ, lookFor, q_cpv, regexQueryOption);
                            }
                            else
                                modifiedQ = Regex.Replace(modifiedQ, lookFor, "", regexQueryOption);
                        }
                        else if (replaceWith.Contains("${form}"))
                        {
                            lookFor = @"form:(?<q>((F|CZ)\d{1,2}(,)?)*)\s*";
                            Match m = Regex.Match(modifiedQ, lookFor, regexQueryOption);
                            string form = "";
                            if (m.Success)
                                form = m.Groups["q"].Value;
                            if (!string.IsNullOrEmpty(form))
                            {
                                string[] forms = form.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                                string q_form = "";
                                if (forms.Length > 0)
                                    q_form = "formulare.druh:(" + forms.Select(s => s + "*").Aggregate((f, s) => f + " OR " + s) + ")";

                                modifiedQ = Regex.Replace(modifiedQ, lookFor, q_form, regexQueryOption);
                            }
                            else
                                modifiedQ = Regex.Replace(modifiedQ, lookFor, "", regexQueryOption);
                        }
                        else if (lookFor.Contains("holding:"))
                        {
                            //list of ICO connected to this person
                            Match m = Regex.Match(modifiedQ, lookFor, regexQueryOption);
                            string holdingIco = m.Groups["q"].Value;
                            HlidacStatu.Lib.Data.Relation.AktualnostType aktualnost = HlidacStatu.Lib.Data.Relation.AktualnostType.Nedavny;
                            Data.Firma f = Data.Firmy.Get(holdingIco);
                            if (f != null && f.Valid)
                            {
                                var icos = new string[] { f.ICO }
                                        .Union(f.AktualniVazby(aktualnost)
                                            .Select(s => s.To.Id)
                                            )
                                    .Distinct();
                                string icosQuery = "";
                                var icosPresLidi = f.AktualniVazby(aktualnost)
                                        .Where(o=>o.To.Type == Graph.Node.NodeType.Person)
                                        .Select(o => Data.Osoby.GetById.Get(Convert.ToInt32(o.To.Id)))
                                        .Where(o => o != null)
                                        .SelectMany(o => o.AktualniVazby(aktualnost))
                                        .Select(v => v.To.Id)
                                        .Distinct();
                                icos = icos.Union(icosPresLidi).Distinct();

                                var templ = "(ico:{0})";
                                if (icos != null && icos.Count() > 0)
                                {
                                    icosQuery = "(" + icos
                                        .Select(t => string.Format(templ, t))
                                        .Aggregate((fi, s) => fi + " OR " + s) + ")";
                                }
                                else
                                {
                                    icosQuery = "(ico:noOne)";
                                }
                                modifiedQ = Regex.Replace(modifiedQ, lookFor, icosQuery, regexQueryOption);

                            }
                        } //do regex replace
                        else if (replaceWith.Contains("${ico}"))
                        {
                            //list of ICO connected to this person
                            Match m = Regex.Match(modifiedQ, lookFor, regexQueryOption);
                            string nameId = m.Groups["q"].Value;
                            Data.Osoba p = Data.Osoby.GetByNameId.Get(nameId);
                            string icosQuery = "";
                            if (p != null)
                            {
                                var icos = p
                                            .AktualniVazby(Data.Relation.AktualnostType.Nedavny)
                                            .Where(w => !string.IsNullOrEmpty(w.To.Id))
                                            .Where(w => Analysis.ACore.GetBasicStatisticForICO(w.To.Id).Summary.Pocet > 0)
                                            .Select(w => w.To.Id)
                                            .Distinct().ToArray();
                                var templ = "(ico:{0})";
                                if (icos != null && icos.Length > 0)
                                {
                                    icosQuery = "(" + icos
                                        .Select(t => string.Format(templ, t))
                                        .Aggregate((f, s) => f + " OR " + s) + ")";
                                }
                                else
                                {
                                    icosQuery = "(ico:noOne)";
                                }
                                modifiedQ = Regex.Replace(modifiedQ, lookFor, icosQuery, regexQueryOption);
                            }
                            else
                                modifiedQ = Regex.Replace(modifiedQ, lookFor, "(ico:noOne)", regexQueryOption);
                        }
                        else
                        {
                            modifiedQ = Regex.Replace(modifiedQ, lookFor, evalMatch, regexQueryOption);

                        }
                    }
                }


                QueryContainer qc = null;
                if (modifiedQ == null)
                    qc = new QueryContainerDescriptor<VerejnaZakazka>().MatchNone();
                else if (string.IsNullOrEmpty(modifiedQ))
                    qc = new QueryContainerDescriptor<VerejnaZakazka>().MatchAll();
                else
                {
                    modifiedQ = modifiedQ.Replace(" | ", " OR ").Trim();
                    qc = new QueryContainerDescriptor<VerejnaZakazka>()
                        .QueryString(qs => qs
                            .Query(modifiedQ)
                            .DefaultOperator(Operator.And)
                        );
                }

                return qc;
            }


            public static ES.VerejnaZakazkaSearchData SimpleSearch(string query, string[] cpv,
                int page, int pageSize, int order, bool Zahajeny = false, bool withHighlighting = false)
            {
                return SimpleSearch(
                    new ES.VerejnaZakazkaSearchData()
                    {
                        Q = query,
                        OrigQuery = query,
                        CPV = cpv,
                        Page = page,
                        PageSize = pageSize,
                        Order = order.ToString(),
                        Zahajeny = Zahajeny
                    }, withHighlighting: withHighlighting
                    );
            }




            public static ES.VerejnaZakazkaSearchData SimpleSearch(
                ES.VerejnaZakazkaSearchData search,
                AggregationContainerDescriptor<VerejnaZakazka> anyAggregation = null,
                bool logError = true, bool fixQuery = true, ElasticClient client = null, bool withHighlighting = false)
            {

                if (client == null)
                    client = HlidacStatu.Lib.ES.Manager.GetESClient_VZ();

                string query = search.Q ?? "";

                int page = search.Page - 1;
                if (page < 0)
                    page = 0;



                AggregationContainerDescriptor<VerejnaZakazka> baseAggrDesc = null;
                baseAggrDesc = anyAggregation == null ?
                            null //new AggregationContainerDescriptor<VerejnaZakazka>().Sum("sumKc", m => m.Field(f => f.Castka))
                        : anyAggregation;

                Func<AggregationContainerDescriptor<VerejnaZakazka>, AggregationContainerDescriptor<VerejnaZakazka>> aggrFunc
                    = (aggr) => { return baseAggrDesc; };


                Devmasters.Core.StopWatchEx sw = new Devmasters.Core.StopWatchEx();
                sw.Start();

                if (fixQuery)
                {
                    search.OrigQuery = query;
                    query = ES.SearchTools.FixInvalidQuery(query, queryShorcuts, queryOperators);
                }
                if (logError && search.Q != search.OrigQuery)
                {
                    HlidacStatu.Util.Consts.Logger.Debug(new Devmasters.Core.Logging.LogMessage()
                        .SetMessage("Fixed query")
                        .SetCustomKeyValue("runningQuery", search.Q)
                        .SetCustomKeyValue("origQuery", search.OrigQuery)
                        );
                }


                search.Q = query;
                ISearchResponse<VerejnaZakazka> res = null;
                try
                {
                    res = client
                        .Search<VerejnaZakazka>(s => s
                            .Size(search.PageSize)
                            .Source(so=>so.Excludes(ex=>ex.Field("dokumenty.plainText")))
                            .From(page * search.PageSize)
                            .Query(q => GetSimpleQuery(search))
                            .Sort(ss => GetSort(Convert.ToInt32(search.Order)))
                            .Aggregations(aggrFunc)
                            .Highlight(h =>ES.SearchTools.GetHighlight<VerejnaZakazka>(withHighlighting))
                    );

                }
                catch (Exception e)
                {
                    if (res != null && res.ServerError != null)
                        Lib.ES.Manager.LogQueryError<VerejnaZakazka>(res, "Exception, Orig query:"
                            + search.OrigQuery + "   query:"
                            + search.Q
                            + "\n\n res:" + search.Result.ToString()
                            , ex: e);
                    else
                        HlidacStatu.Util.Consts.Logger.Error("", e);
                    throw;
                }
                sw.Stop();

                if (res.IsValid == false && logError)
                    Lib.ES.Manager.LogQueryError<VerejnaZakazka>(res, "Exception, Orig query:"
                        + search.OrigQuery + "   query:"
                        + search.Q
                        + "\n\n res:" + search.Result?.ToString()
                        );


                search.Total = res?.Total ?? 0;
                search.IsValid = res?.IsValid ?? false;
                search.ElasticResults = res;
                search.ElapsedTime = sw.Elapsed;
                return search;
            }


            public static Nest.ISearchResponse<VerejnaZakazka> RawSearch(string jsonQuery, int page, int pageSize, ES.VerejnaZakazkaSearchData.VZOrderResult order = ES.VerejnaZakazkaSearchData.VZOrderResult.Relevance,
                AggregationContainerDescriptor<VerejnaZakazka> anyAggregation = null
                )
            {
                return RawSearch(GetRawQuery(jsonQuery), page, pageSize, order, anyAggregation);
            }


            public static Nest.ISearchResponse<VerejnaZakazka> RawSearch(Nest.QueryContainer query, int page, int pageSize, ES.VerejnaZakazkaSearchData.VZOrderResult order = ES.VerejnaZakazkaSearchData.VZOrderResult.Relevance,
                AggregationContainerDescriptor<VerejnaZakazka> anyAggregation = null
                )
            {
                page = page - 1;
                if (page < 0)
                    page = 0;


                AggregationContainerDescriptor<VerejnaZakazka> baseAggrDesc = null;
                baseAggrDesc = anyAggregation == null ?
                            null //new AggregationContainerDescriptor<VerejnaZakazka>().Sum("sumKc", m => m.Field(f => f.Castka))
                        : anyAggregation;

                Func<AggregationContainerDescriptor<VerejnaZakazka>, AggregationContainerDescriptor<VerejnaZakazka>> aggrFunc
                    = (aggr) => { return baseAggrDesc; };



                var client = Lib.ES.Manager.GetESClient();
                Indices indexes = client.ConnectionSettings.DefaultIndex;


                var res = client
                        .Search<VerejnaZakazka>(s => s
                            .Index(indexes)
                            .Size(pageSize)
                            .From(page * pageSize)
                            .Query(q => query)
                            //.Source(m => m.Excludes(e => e.Field(o => o.Prilohy)))
                            .Sort(ss => GetSort((int)order))
                            .Aggregations(aggrFunc)

                        );
                if (res.IsValid == false)
                    Lib.ES.Manager.LogQueryError<VerejnaZakazka>(res);
                
                return res;

            }


            public static ES.VerejnaZakazkaSearchData _Search(
            ES.VerejnaZakazkaSearchData search,
            AggregationContainerDescriptor<VerejnaZakazka> anyAggregation, ElasticClient client)
            {
                if (string.IsNullOrEmpty(search.Q) && (search.CPV == null || search.CPV.Length == 0))
                    return null;

                if (client == null)
                    client = HlidacStatu.Lib.ES.Manager.GetESClient_VZ();

                AggregationContainerDescriptor<VerejnaZakazka> baseAggrDesc = null;
                baseAggrDesc = anyAggregation == null ?
                            null //new AggregationContainerDescriptor<VerejnaZakazka>().Sum("sumKc", m => m.Field(f => f.Castka))
                        : anyAggregation;

                Func<AggregationContainerDescriptor<VerejnaZakazka>, AggregationContainerDescriptor<VerejnaZakazka>> aggrFunc
                    = (aggr) => { return baseAggrDesc; };

                string queryString = search.Q;
                Nest.ISearchResponse<VerejnaZakazka> res = null;
                if (search.CPV != null && search.CPV.Length > 0)
                {
                    string cpvs = search.CPV.Select(c => c + "*").Aggregate((f, s) => f + " OR " + s);
                    if (!string.IsNullOrEmpty(queryString))
                        queryString = queryString + " AND (cPV:(" + cpvs + "))";
                    else
                        queryString = "cPV:(" + cpvs + ")";

                }

                int page = search.Page - 1;
                if (page < 0)
                    page = 0;
                res = client
                        .Search<HlidacStatu.Lib.Data.VZ.VerejnaZakazka>(a => a
                            .Size(search.PageSize)
                            .From(search.PageSize * page)
                            .Aggregations(aggrFunc)
                            .Query(qq => qq.QueryString(qs => qs
                                    .Query(queryString)
                                    .DefaultOperator(Nest.Operator.And)
                                    )
                                )
                            .Sort(ss => GetSort(Convert.ToInt32(search.Order)))
                            .Aggregations(aggrFunc)
                            );


                search.IsValid = res.IsValid;
                search.ElasticResults = res;
                search.Total = res.Total;


                return search;
            }

            public static SortDescriptor<Data.VZ.VerejnaZakazka> GetSort(int iorder)
            {
                ES.VerejnaZakazkaSearchData.VZOrderResult order = (ES.VerejnaZakazkaSearchData.VZOrderResult)iorder;

                SortDescriptor<Data.VZ.VerejnaZakazka> s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field("_score").Descending());
                switch (order)
                {
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.DateAddedDesc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.DatumUverejneni).Descending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.DateAddedAsc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.DatumUverejneni).Ascending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.DateSignedDesc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.DatumUzavreniSmlouvy).Descending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.DateSignedAsc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.DatumUzavreniSmlouvy).Ascending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.PriceAsc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.KonecnaHodnotaBezDPH).Ascending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.PriceDesc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(m => m.Field(f => f.KonecnaHodnotaBezDPH).Descending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.FastestForScroll:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field("_doc"));
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.CustomerAsc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field(ff => ff.Zadavatel.Jmeno).Ascending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.ContractorAsc:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field("dodavatele.jmeno").Ascending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.LastUpdate:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field("lastUpdated").Descending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.PosledniZmena:
                        s = new SortDescriptor<Data.VZ.VerejnaZakazka>().Field(f => f.Field("posledniZmena").Descending());
                        break;
                    case ES.VerejnaZakazkaSearchData.VZOrderResult.Relevance:
                    default:
                        break;
                }

                return s;

            }

            public static MemoryCacheManager<ES.VerejnaZakazkaSearchData, string>
                cachedSearches = new MemoryCacheManager<ES.VerejnaZakazkaSearchData, string>("VZsearch", cachedFuncSimpleSearch, TimeSpan.FromHours(24));

            public static ES.VerejnaZakazkaSearchData CachedSimpleSearch(TimeSpan expiration, ES.VerejnaZakazkaSearchData search,
                bool logError = true, bool fixQuery = true, ElasticClient client = null)
            {
                FullSearchQuery q = new FullSearchQuery()
                {
                    search = search,
                    logError = logError,
                    fixQuery = fixQuery,
                    client = client
                };
                return cachedSearches.Get(Newtonsoft.Json.JsonConvert.SerializeObject(q), expiration);
            }
            private static ES.VerejnaZakazkaSearchData cachedFuncSimpleSearch(string jsonFullSearchQuery)
            {
                var query = Newtonsoft.Json.JsonConvert.DeserializeObject<FullSearchQuery>(jsonFullSearchQuery);
                return SimpleSearch(query.search, query.anyAggregation, query.logError, query.fixQuery, query.client);
            }

            private class FullSearchQuery {
                public ES.VerejnaZakazkaSearchData search;
                public AggregationContainerDescriptor<VerejnaZakazka> anyAggregation = null;
                public bool logError = true;
                public bool fixQuery = true;
                public ElasticClient client = null;
            }
        }

    }
}
