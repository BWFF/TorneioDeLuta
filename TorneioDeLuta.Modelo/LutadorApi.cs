using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneioDeLuta.Modelo
{
    public class LutadorApi
    {
        public int id { get; set; }
        public string nome { get; set; }
        public int idade { get; set; }
        public List<string> artesMarciais { get; set; }
        public int lutas { get; set; }
        public int derrotas { get; set; }
        public int vitorias { get; set; }
        public bool selecionado { get; set; }
    }
}
