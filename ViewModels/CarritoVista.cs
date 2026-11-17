namespace MultiTenantCSharpVargas.ViewModels;

public class CarritoVista
{
    public List<CarritoItemVista> Items { get; set; } = new();
    public decimal Total => Items.Sum(item => item.Subtotal);
    public int CantidadTotal => Items.Sum(item => item.Cantidad);

    // El carrito admite un solo comercio, así que hay un único botón de WhatsApp.
    // Si quedó un carrito viejo con varios comercios, no se muestra el botón.
    public CarritoEmpresaVista? Empresa
    {
        get
        {
            var grupos = Items
                .GroupBy(item => new
                {
                    item.EmpresaId,
                    item.EmpresaNombre,
                    item.EmpresaWhatsApp,
                    item.EmpresaTelefono
                })
                .Select(grupo => new CarritoEmpresaVista
                {
                    EmpresaId = grupo.Key.EmpresaId,
                    EmpresaNombre = grupo.Key.EmpresaNombre,
                    WhatsApp = grupo.Key.EmpresaWhatsApp,
                    Telefono = grupo.Key.EmpresaTelefono,
                    Items = grupo.ToList()
                })
                .ToList();

            return grupos.Count == 1 ? grupos[0] : null;
        }
    }
}

public class CarritoItemVista
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaWhatsApp { get; set; }
    public string? EmpresaTelefono { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public int Stock { get; set; }
    public string? RutaImagen { get; set; }

    public string Inicial => string.IsNullOrWhiteSpace(Nombre) ? "?" : Nombre.Trim()[0].ToString().ToUpper();
}

public class CarritoEmpresaVista
{
    public int EmpresaId { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Telefono { get; set; }
    public List<CarritoItemVista> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(item => item.Subtotal);
    public int CantidadTotal => Items.Sum(item => item.Cantidad);

    // Si el comerciante no cargó WhatsApp se intenta con el teléfono.
    public string? NumeroWhatsApp => Normalizar(WhatsApp) ?? Normalizar(Telefono);
    public bool TieneWhatsApp => NumeroWhatsApp is not null;

    public string EnlaceWhatsApp =>
        $"https://wa.me/{NumeroWhatsApp}?text={Uri.EscapeDataString(MensajePedido)}";

    private string MensajePedido
    {
        get
        {
            var lineas = Items.Select(item =>
                $"- {item.Cantidad} x {item.Nombre} — $ {item.Subtotal:N2}");

            return $"Hola {EmpresaNombre}! Quiero hacer este pedido desde Tu Tienda Cerca:{Environment.NewLine}" +
                   $"{string.Join(Environment.NewLine, lineas)}{Environment.NewLine}{Environment.NewLine}" +
                   $"Total: $ {Subtotal:N2}";
        }
    }

    private static string? Normalizar(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return null;
        }

        var digitos = new string(numero.Where(char.IsDigit).ToArray());
        return digitos.Length >= 8 ? digitos : null;
    }
}
