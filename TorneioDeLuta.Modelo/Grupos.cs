using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneioDeLuta.Modelo
{
    public class Grupos: Lutador
    {
        public string Chave { get; set; }
        public int PercVitoria { get; set; }
        public int ordemClassificao { get; set; }
        public int classificacaoFinal { get; set; }
        public int disputa { get; set; }
        public int qtdArtesMarciais { get; set; }
    }
}
