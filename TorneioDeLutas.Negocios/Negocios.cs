using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TorneioDeLuta.DTO;
using TorneioDeLuta.Modelo;

namespace TorneioDeLuta.Negocios
{
    public class Inteligencia
    {
        private Contexto db = new Contexto();

        HttpClient client = new HttpClient();
        public IEnumerable<LutadorApi> lutadores;

        public Inteligencia()
        {
            client.BaseAddress = new Uri("http://177.36.237.87/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public List<Grupos> IniciaTorneio(List<Lutador> lutadores)
        {
            List<Lutador> listaLutadores = new List<Lutador>();
            listaLutadores.AddRange(GetLutadoresSelecionados());
            List<Grupos> campeonato = new List<Grupos>();

            if (listaLutadores.Count() == 20)
            {
                campeonato = DivisaoDeGrupos(listaLutadores.ToList().Where(l => l.selecionado));
                campeonato = FaseDeGrupos(campeonato);
                campeonato = RetornaClassificado("", campeonato, "Q");
                campeonato = RetornaClassificado("", campeonato, "S");
                campeonato = RetornaClassificado("", campeonato, "F");

                LimpaRegistros();
            }

            return campeonato;
        }

        private void CarregaLutadores()
        {
            if (db.lutadores.Where(l => l.selecionado).Count() == 0)
            {
                LimpaRegistros();

                HttpResponseMessage response = client.GetAsync("/lutadores/api/competidores").Result;

                if (response.IsSuccessStatusCode)
                {
                    lutadores = response.Content.ReadAsAsync<IEnumerable<LutadorApi>>().Result;
                }

                foreach (var item in lutadores)
                {
                    Lutador competidor = new Lutador()
                    {
                        id = item.id,
                        nome = item.nome,
                        idade = item.idade,
                        artesMarciais = item.artesMarciais.Count(),
                        lutas = item.lutas,
                        derrotas = item.derrotas,
                        vitorias = item.vitorias,
                        selecionado = item.selecionado
                    };
                    db.lutadores.Add(competidor);
                }

                db.SaveChanges();
            }
        }

        public List<Lutador> GetLutadores()
        {
            CarregaLutadores();
            return db.lutadores.ToList();
        }

        public List<Lutador> GetLutadoresSelecionados()
        {
            List<Lutador> selecionados = new List<Lutador>();
            selecionados.AddRange(db.lutadores.ToList().Where(l => l.selecionado));

            return selecionados;
        }

        public void SetSelecionaLutador(int id, bool check)
        {
            Lutador lutador = db.lutadores.Find(id);
            lutador.selecionado = check;
            db.SaveChanges();            
        }

        public List<Grupos> DivisaoDeGrupos(IEnumerable<Lutador> Participantes)
        {
            List<Grupos> listaGrupos = new List<Grupos>();

            foreach (var participante in Participantes.OrderBy(p => p.idade))
            {
                string sChave = string.Empty;

                if (listaGrupos.Count() < 5)
                {
                    sChave = "A";
                }
                else if (listaGrupos.Count() >= 5 && listaGrupos.Count() < 10)
                {
                    sChave = "B";
                }
                else if (listaGrupos.Count() >= 10 && listaGrupos.Count() < 15)
                {
                    sChave = "C";
                }
                else if (listaGrupos.Count() >= 15 && listaGrupos.Count() <= 20)
                {
                    sChave = "D";
                }

                Grupos grupo = new Grupos()
                {
                    Chave = sChave,
                    id = participante.id,
                    nome = participante.nome,
                    idade = participante.idade,
                    qtdArtesMarciais = participante.artesMarciais,
                    lutas = participante.lutas,
                    derrotas = participante.derrotas,
                    vitorias = participante.vitorias,
                    selecionado = participante.selecionado
                };

                listaGrupos.Add(grupo);
            }

            return listaGrupos;
        }

        public List<Grupos> FaseDeGrupos(List<Grupos> grupos)
        {
            List<Grupos> listaGrupoRes = new List<Grupos>();

            listaGrupoRes.AddRange(RetornaClassificado("A", grupos, "G"));
            listaGrupoRes.AddRange(RetornaClassificado("B", grupos, "G"));
            listaGrupoRes.AddRange(RetornaClassificado("C", grupos, "G"));
            listaGrupoRes.AddRange(RetornaClassificado("D", grupos, "G"));

            return listaGrupoRes;
        }

        public List<Grupos> RetornaClassificado(string chave, List<Grupos> grupo, string fase)
        {
            List<Grupos> classificados = new List<Grupos>();

            if (fase == "G")
            {
                List<Grupos> grupoLutando = new List<Grupos>();
                grupoLutando.AddRange(grupo.ToList().Where(g => g.Chave == chave));

                foreach (var luta in grupoLutando)
                {
                    luta.PercVitoria = (int)Math.Floor(((float)(luta.vitorias / (float)luta.lutas)) * 100);
                }

                classificados.AddRange(grupoLutando.OrderByDescending(c => c.PercVitoria).Take(2));
            }
            else if (fase == "Q")
            {
                List<Grupos> lutaA = new List<Grupos>();

                lutaA.AddRange(grupo.Where(g => g.Chave == "A").OrderByDescending(g => g.PercVitoria).Take(1));
                lutaA.AddRange(grupo.Where(g => g.Chave == "B").OrderBy(g => g.PercVitoria).Take(1));
                classificados.AddRange(RetornaSemiFinalistas(lutaA, 1).OrderByDescending(r => r.PercVitoria).Take(1));

                lutaA.Clear();
                lutaA.AddRange(grupo.Where(g => g.Chave == "A").OrderBy(g => g.PercVitoria).Take(1));
                lutaA.AddRange(grupo.Where(g => g.Chave == "B").OrderByDescending(g => g.PercVitoria).Take(1));
                classificados.AddRange(RetornaSemiFinalistas(lutaA, 2).OrderByDescending(r => r.PercVitoria).Take(1));

                lutaA.Clear();
                lutaA.AddRange(grupo.Where(g => g.Chave == "C").OrderByDescending(g => g.PercVitoria).Take(1));
                lutaA.AddRange(grupo.Where(g => g.Chave == "D").OrderBy(g => g.PercVitoria).Take(1));
                classificados.AddRange(RetornaSemiFinalistas(lutaA, 3).OrderByDescending(r => r.PercVitoria).Take(1));

                lutaA.Clear();
                lutaA.AddRange(grupo.Where(g => g.Chave == "C").OrderBy(g => g.PercVitoria).Take(1));
                lutaA.AddRange(grupo.Where(g => g.Chave == "D").OrderByDescending(g => g.PercVitoria).Take(1));
                classificados.AddRange(RetornaSemiFinalistas(lutaA, 4).OrderByDescending(r => r.PercVitoria).Take(1));
            }
            else if (fase == "S")
            {
                List<Grupos> lutas = new List<Grupos>();
                List<Grupos> terceiroQuarto = new List<Grupos>();
                List<Grupos> finalista = new List<Grupos>();

                lutas.AddRange(grupo.Where(g => g.ordemClassificao == 1 || g.ordemClassificao == 2));
                finalista.AddRange(lutas.OrderByDescending(g => g.PercVitoria).Take(1));

                lutas.Clear();
                lutas.AddRange(grupo.Where(g => g.ordemClassificao == 3 || g.ordemClassificao == 4));
                finalista.AddRange(lutas.OrderByDescending(g => g.PercVitoria).Take(1));
                foreach (var final in finalista)
                {
                    final.disputa = 1;
                }
                classificados.AddRange(finalista);

                lutas.Clear();
                lutas.AddRange(grupo.Where(g => g.ordemClassificao == 1 || g.ordemClassificao == 2));
                terceiroQuarto.AddRange(lutas.OrderBy(g => g.PercVitoria).Take(1));

                lutas.Clear();
                lutas.AddRange(grupo.Where(g => g.ordemClassificao == 3 || g.ordemClassificao == 4));
                terceiroQuarto.AddRange(lutas.OrderBy(g => g.PercVitoria).Take(1));
                foreach (var terceiro in terceiroQuarto)
                {
                    terceiro.disputa = 3;
                }

                classificados.AddRange(terceiroQuarto);

            }
            else if (fase == "F")
            {
                ResultadoFinal(grupo, 1);
                ResultadoFinal(grupo, 3);

                classificados.AddRange(grupo);
            }

            return classificados;
        }

        private List<Grupos> RetornaSemiFinalistas(List<Grupos> grupos, int rodada)
        {
            List<Grupos> listaClassificado = new List<Grupos>();

            grupos.OrderByDescending(l => l.PercVitoria);
            foreach (var lutador in grupos)
            {
                lutador.ordemClassificao = rodada;

                listaClassificado.Add(lutador);
            }

            return listaClassificado;
        }

        private List<Grupos> ResultadoFinal(List<Grupos> grupos, int disputa)
        {
            List<Grupos> listaFinal = new List<Grupos>();

            foreach (var item in grupos.Where(g => g.disputa == disputa).OrderByDescending(g => g.PercVitoria))
            {
                item.classificacaoFinal = disputa;
                listaFinal.Add(item);
                disputa++;
            }

            return listaFinal;
        }

        public void LimpaRegistros()
        {
            db.Database.Delete();
        }
    }    
}
