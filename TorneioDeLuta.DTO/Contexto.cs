using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneioDeLuta.Modelo;

namespace TorneioDeLuta.DTO
{
    public class Contexto: DbContext
    {
        public DbSet<Lutador> lutadores { get; set; }
    }
}
