using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using TorneioDeLuta.Modelo;
using TorneioDeLuta.DTO;
using TorneioDeLuta.Negocios;

namespace TorneioDeLuta.Controllers
{
    public class TorneioController : Controller
    {
        private Inteligencia torneio = new Inteligencia(); 

        [HttpGet]
        public ActionResult Index()
        {            
            return View(torneio.GetLutadores());
        }

        public ActionResult Selecionar(int id, bool check)
        {
            torneio.SetSelecionaLutador(id, check);
            return RedirectToAction("Index");
        }

        public ActionResult Resultado(List<Lutador> lutadores)
        {
            if (torneio.GetLutadoresSelecionados().Count() == 20)
            {
                return View(
                        torneio.IniciaTorneio(lutadores).Where(c => c.classificacaoFinal == 1 || c.classificacaoFinal == 2 || c.classificacaoFinal == 3).OrderBy(c => c.classificacaoFinal)
                );                
            }
            else
            {
                return RedirectToAction("Index");
            }            
        }

        public List<Grupos> DivisaoDeGrupos(List<Lutador> Participantes)
        {           
            return torneio.DivisaoDeGrupos(Participantes);
        }        
    }
}
