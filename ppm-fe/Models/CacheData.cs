using ppm_fe.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ppm_fe.Models
{
    public class CacheData
    {
        public List<Work> works { get; set; }

        public List<Work> usersWorks { get; set; }

        public List<Billing> billings { get; set; }

        public Dictionary<string, List<Billing>> billingsPerMonth { get; set; } = new Dictionary<string, List<Billing>>();


    }
}