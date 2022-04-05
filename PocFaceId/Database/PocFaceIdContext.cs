using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PocFaceId.Model;

namespace PocFaceId.Data
{
    public class PocFaceIdContext : DbContext
    {
        public PocFaceIdContext (DbContextOptions<PocFaceIdContext> options)
            : base(options)
        {
        }

        public DbSet<Pessoa> Pessoa { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
    }
}
