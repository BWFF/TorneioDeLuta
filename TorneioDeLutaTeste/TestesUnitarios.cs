using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TorneioDeLuta.Negocios;

namespace TorneioDeLutaTeste
{
    [TestClass]
    public class TestesUnitarios
    {
        Inteligencia torneio = new Inteligencia();

        [TestMethod]
        public void Validacao_Metodos()
        {
            var lutadores = torneio.GetLutadores();
            Assert.IsTrue( lutadores.Count > 0);

            foreach (var item in lutadores.Take(20))
            {
                torneio.SetSelecionaLutador(item.id, true);
            }

            var divisaoGrupo = torneio.DivisaoDeGrupos(torneio.GetLutadoresSelecionados());            

            Assert.IsTrue(divisaoGrupo.Where(g => g.Chave == "A").Count() == 5);
            Assert.IsTrue(divisaoGrupo.Where(g => g.Chave == "B").Count() == 5);
            Assert.IsTrue(divisaoGrupo.Where(g => g.Chave == "C").Count() == 5);
            Assert.IsTrue(divisaoGrupo.Where(g => g.Chave == "D").Count() == 5);

            var faseGrupo = torneio.FaseDeGrupos(divisaoGrupo);

            Assert.IsTrue(faseGrupo.Where(f => f.Chave == "A").Count() == 2);
            Assert.IsTrue(faseGrupo.Where(f => f.Chave == "B").Count() == 2);
            Assert.IsTrue(faseGrupo.Where(f => f.Chave == "C").Count() == 2);
            Assert.IsTrue(faseGrupo.Where(f => f.Chave == "D").Count() == 2);

            var quartaFinais = torneio.RetornaClassificado("", faseGrupo, "Q");
            Assert.IsTrue(quartaFinais.Count() == 4);

            var semiFinal = torneio.RetornaClassificado("", quartaFinais, "S");
            Assert.IsTrue(semiFinal.Where(s => s.disputa == 1).Count() == 2);
            Assert.IsTrue(semiFinal.Where(s => s.disputa == 3).Count() == 2);

            var final = torneio.RetornaClassificado("", semiFinal, "F");
            Assert.IsTrue(final.Where(f => f.classificacaoFinal == 1).Count() == 1);
            Assert.IsTrue(final.Where(f => f.classificacaoFinal == 2).Count() == 1);
            Assert.IsTrue(final.Where(f => f.classificacaoFinal == 3).Count() == 1);
            Assert.IsTrue(final.Where(f => f.classificacaoFinal == 4).Count() == 1);
        }

    }
}
