﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace WikiDataAnalysis
{
    class BEMSmodel
    {
        public enum BEMS { B,E,M,S};
        const string dicUrl = "https://raw.githubusercontent.com/fxsjy/jieba/master/jieba/finalseg/prob_emit.py";
        Dictionary<char, Dictionary<char, double>> dic;
        public async Task DownloadDictionaryAsync()
        {
            HttpClient client = new HttpClient();
            var data = await client.GetStringAsync(dicUrl);
            data = data.Remove(data.LastIndexOf('}') + 1).Substring(data.IndexOf('{'));
            //Console.WriteLine(data.Remove(1000) + "\r\n===============\r\n" + data.Substring(data.Length - 1000));
            dic = JsonConvert.DeserializeObject<Dictionary<char, Dictionary<char, double>>>(data);
        }
        public double Query(BEMS b,char c)
        {
            return dic[b == BEMS.B ? 'B' : b == BEMS.E ? 'E' : b == BEMS.M ? 'M' : 'S'][c];
        }
    }
}
