using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Models.dbModels;

[Table("direccion")]
[Index("CodigoPostal", Name = "IX_direccion_codigo_postal")]
public partial class Direccion
{
    [Key]
    [Column("id_direccion")]
    public int IdDireccion { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("pais_id")]
    public int? PaisId { get; set; }

    [Column("estado_id")]
    public int? EstadoId { get; set; }

    [Column("ciudad_id")]
    public int? CiudadId { get; set; }

    [Column("google_PlaceId")]
    [StringLength(255)]
    public string? GooglePlaceId { get; set; }

    [Column("pais_nombre")]
    [StringLength(100)]
    public string? PaisNombre { get; set; }

    [Column("ciudad_nombre")]
    [StringLength(100)]
    public string? CiudadNombre { get; set; }

    [Column("codigo_postal")]
    [StringLength(20)]
    public string? CodigoPostal { get; set; }

    [Column("colonia")]
    [StringLength(100)]
    public string? Colonia { get; set; }

    [Column("calle")]
    [StringLength(150)]
    public string? Calle { get; set; }

    [Column("numero_interior")]
    [StringLength(20)]
    public string? NumeroInterior { get; set; }

    [Column("numero_exterior")]
    [StringLength(20)]
    public string? NumeroExterior { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Telefono { get; set; }

    [Column("latitud", TypeName = "decimal(10, 8)")]
    public decimal? Latitud { get; set; }

    [Column("longitud", TypeName = "decimal(11, 8)")]
    public decimal? Longitud { get; set; }

    [Column("direccion_completa")]
    [StringLength(500)]
    public string? DireccionCompleta { get; set; }

    [Column("referencia")]
    [StringLength(255)]
    public string? Referencia { get; set; }

    [Column("notas")]
    [StringLength(500)]
    public string? Notas { get; set; }

    [Column("etiqueta")]
    [StringLength(100)]
    public string? Etiqueta { get; set; }

    [Column("es_default")]
    public bool EsDefault { get; set; }

    [Column("fecha_creacion")]
    [Precision(3)]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion", TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }

    [ForeignKey("CiudadId")]
    [InverseProperty("Direccions")]
    public virtual Ciudad? Ciudad { get; set; }

    [InverseProperty("Direccion")]
    public virtual ICollection<CotizacionEnvio> CotizacionEnvios { get; set; } = new List<CotizacionEnvio>();

    [ForeignKey("EstadoId")]
    [InverseProperty("Direccions")]
    public virtual Estado? Estado { get; set; }

    [InverseProperty("DireccionEnvio")]
    public virtual ICollection<Orden> Ordens { get; set; } = new List<Orden>();

    [ForeignKey("PaisId")]
    [InverseProperty("Direccions")]
    public virtual Pai? Pais { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Direccions")]
    public virtual Usuario Usuario { get; set; } = null!;
}
