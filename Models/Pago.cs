public class Pago
{
    public int IdPago { get; set; }
    
    public int MembresiaId { get; set; }
    public Membresia Membresia { get; set; } = null!;
    
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Importe { get; set; }
    public MedioPago MedioPago { get; set; }
    public bool Estado { get; set; } 

    // Auditoría
    public int RegistradoPor { get; set; } // IdUsuario
    public int? AnuladoPor { get; set; }   // IdUsuario (opcional si se anula)
}
    public enum MedioPago
{
    Efectivo=1,
    Debito=2,
    Credito=3,
    Transferencia=4

}