﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Entities;
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int MarcaId { get; set; }
    public Marca Marca { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; }
    public DateTime FechaCreacion { get; set; }

}
