using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("metodo_pago")]
[Index("NombreMetodoPago", Name = "UQ__metodo_p__436F61F2512CA069", IsUnique = true)]
public partial class MetodoPago
{
    [Key]
    [Column("id_metodo_pago")]
    public int IdMetodoPago { get; set; }

    [Column("nombre_metodo_pago")]
    [StringLength(150)]
    public string NombreMetodoPago { get; set; } = null!;

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string? Tipo { get; set; }

    [Column("esta_activo")]
    public bool EstaActivo { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("MetodoReembolso")]
    public virtual ICollection<OrdenDevolucion> OrdenDevolucions { get; set; } = new List<OrdenDevolucion>();

    [InverseProperty("MetodoPago")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();

    [InverseProperty("MetodoPago")]
    public virtual ICollection<PagoTransaccion> PagoTransaccions { get; set; } = new List<PagoTransaccion>();
}
