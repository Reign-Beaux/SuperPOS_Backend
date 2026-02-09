using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services;

/// <summary>
/// Service for generating PDF tickets using QuestPDF.
/// </summary>
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public TicketService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;

        // Configure QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateSaleTicketAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        // Get sale with complete details (customer, user, sale details, products)
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(saleId, cancellationToken);

        if (sale == null)
            throw new InvalidOperationException($"Sale with ID {saleId} not found");

        // Get business information from configuration
        var businessName = _configuration["BusinessInfo:Name"] ?? "Super POS";
        var businessAddress = _configuration["BusinessInfo:Address"] ?? "Dirección no configurada";
        var businessPhone = _configuration["BusinessInfo:Phone"] ?? "Teléfono no configurado";
        var businessEmail = _configuration["BusinessInfo:Email"] ?? "Email no configurado";

        // Generate PDF using QuestPDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });

            // Local functions for composition
            void ComposeHeader(IContainer container)
            {
                container.Column(column =>
                {
                    // Business name
                    column.Item().AlignCenter().Text(businessName)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().Text("Ticket de Venta")
                        .FontSize(14)
                        .SemiBold();

                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    // Sale information
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Folio: #{sale.Id.ToString().Substring(0, 8).ToUpper()}").SemiBold();
                            col.Item().Text($"Fecha: {sale.CreatedAt:dd/MM/yyyy HH:mm:ss}");
                            col.Item().Text($"Cajero: {sale.User?.Name ?? "N/A"}");
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text($"Cliente: {sale.Customer?.Name ?? "Público General"}");
                            col.Item().AlignRight().Text(businessAddress).FontSize(8);
                            col.Item().AlignRight().Text(businessPhone).FontSize(8);
                        });
                    });

                    column.Item().PaddingVertical(10).LineHorizontal(1);
                });
            }

            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(10).Column(column =>
                {
                    // Table header
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Product
                            columns.RelativeColumn(1); // Quantity
                            columns.RelativeColumn(1); // Unit Price
                            columns.RelativeColumn(1); // Total
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                .Text("PRODUCTO").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                .AlignCenter().Text("CANT").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                .AlignRight().Text("P.UNIT").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                .AlignRight().Text("TOTAL").SemiBold();
                        });

                        // Items
                        foreach (var detail in sale.SaleDetails)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                .Text(detail.Product?.Name ?? "Producto");

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                .AlignCenter().Text(detail.Quantity.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                .AlignRight().Text($"${detail.UnitPrice:N2}");

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                .AlignRight().Text($"${detail.Total:N2}");
                        }
                    });

                    // Totals section
                    column.Item().PaddingTop(20).AlignRight().Column(totalsColumn =>
                    {
                        // Calculate subtotal and tax from total (assuming total includes 16% IVA)
                        var subtotal = sale.TotalAmount / 1.16m;
                        var tax = sale.TotalAmount - subtotal;

                        totalsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal:");
                            row.ConstantItem(100).AlignRight().Text($"${subtotal:N2}");
                        });

                        totalsColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA (16%):");
                            row.ConstantItem(100).AlignRight().Text($"${tax:N2}");
                        });

                        totalsColumn.Item().PaddingTop(5).LineHorizontal(1);

                        totalsColumn.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL:").FontSize(12).Bold();
                            row.ConstantItem(100).AlignRight().Text($"${sale.TotalAmount:N2}")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                        });
                    });
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().PaddingTop(20).LineHorizontal(1);

                    column.Item().PaddingTop(10).AlignCenter()
                        .Text("¡Gracias por su compra!")
                        .FontSize(12)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken1);

                    column.Item().AlignCenter()
                        .Text($"Vuelva pronto a {businessName}")
                        .FontSize(10);

                    column.Item().PaddingTop(10).AlignCenter()
                        .Text(businessEmail)
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().AlignCenter()
                        .Text("Este documento es un comprobante de venta simplificado")
                        .FontSize(7)
                        .Italic()
                        .FontColor(Colors.Grey.Medium);
                });
            }
        }).GeneratePdf();

        return pdfBytes;
    }

    public async Task<string> GenerateAndSaveTicketAsync(Guid saleId, string outputPath, CancellationToken cancellationToken = default)
    {
        var pdfBytes = await GenerateSaleTicketAsync(saleId, cancellationToken);
        await File.WriteAllBytesAsync(outputPath, pdfBytes, cancellationToken);
        return outputPath;
    }

    public async Task<byte[]> GenerateCashRegisterReportAsync(Guid cashRegisterId, CancellationToken cancellationToken = default)
    {
        // Get cash register with user details
        var cashRegister = await _unitOfWork.CashRegisters.GetByIdWithDetailsAsync(cashRegisterId, cancellationToken);

        if (cashRegister == null)
            throw new InvalidOperationException($"Cash register with ID {cashRegisterId} not found");

        // Get all sales for the period
        var sales = await _unitOfWork.Sales.GetByDateRangeAsync(
            cashRegister.OpeningDate,
            cashRegister.ClosingDate,
            cancellationToken);

        // Get business information from configuration
        var businessName = _configuration["BusinessInfo:Name"] ?? "Super POS";
        var businessAddress = _configuration["BusinessInfo:Address"] ?? "Dirección no configurada";
        var businessPhone = _configuration["BusinessInfo:Phone"] ?? "Teléfono no configurado";

        // Calculate average ticket
        var averageTicket = cashRegister.TotalTransactions > 0
            ? cashRegister.TotalSales / cashRegister.TotalTransactions
            : 0;

        // Calculate expected amount
        var expectedAmount = cashRegister.InitialCash + cashRegister.TotalSales;

        // Generate PDF using QuestPDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });

            // Local functions for composition
            void ComposeHeader(IContainer container)
            {
                container.Column(column =>
                {
                    // Business name
                    column.Item().AlignCenter().Text(businessName)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().Text("Corte de Caja Diario")
                        .FontSize(14)
                        .SemiBold();

                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    // Cash register information
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Fecha: {cashRegister.OpeningDate:dd/MM/yyyy}").SemiBold();
                            col.Item().Text($"Cajero: {cashRegister.User?.Name ?? "N/A"}");
                            col.Item().Text($"Apertura: {cashRegister.OpeningDate:HH:mm:ss}");
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text(businessAddress).FontSize(8);
                            col.Item().AlignRight().Text(businessPhone).FontSize(8);
                            col.Item().AlignRight().Text($"Cierre: {cashRegister.ClosingDate:HH:mm:ss}");
                        });
                    });

                    column.Item().PaddingVertical(10).LineHorizontal(1);
                });
            }

            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(10).Column(column =>
                {
                    // Financial Summary Section
                    column.Item().Text("RESUMEN FINANCIERO")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(120);
                        });

                        // Rows
                        AddSummaryRow(table, "Fondo Inicial:", cashRegister.InitialCash);
                        AddSummaryRow(table, "Total Ventas:", cashRegister.TotalSales);
                        AddSummaryRow(table, "Fondo Esperado:", expectedAmount);
                        AddSummaryRow(table, "Fondo Real:", cashRegister.FinalCash);

                        // Difference row with color
                        table.Cell().Padding(5).Text("Diferencia:").Bold();
                        table.Cell().Padding(5).AlignRight().Text($"${cashRegister.Difference:N2}")
                            .Bold()
                            .FontColor(cashRegister.Difference == 0 ? Colors.Green.Darken2 :
                                      cashRegister.Difference > 0 ? Colors.Orange.Darken2 :
                                      Colors.Red.Darken2);
                    });

                    column.Item().PaddingTop(20).Text("ESTADÍSTICAS")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(120);
                        });

                        AddStatRow(table, "Total de Ventas:", cashRegister.TotalTransactions.ToString());
                        AddStatRow(table, "Items Vendidos:", cashRegister.TotalItemsSold.ToString());
                        AddSummaryRow(table, "Ticket Promedio:", averageTicket);
                    });

                    // Sales Detail Section
                    if (sales.Any())
                    {
                        column.Item().PaddingTop(20).Text("DETALLE DE VENTAS")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);  // Time
                                columns.RelativeColumn(2);    // Customer
                                columns.ConstantColumn(80);   // Total
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("Hora").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("Cliente").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text("Total").SemiBold();
                            });

                            // Sales rows
                            foreach (var sale in sales.OrderBy(s => s.CreatedAt))
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                    .Text(sale.CreatedAt.ToString("HH:mm"));

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                    .Text(sale.Customer?.Name ?? "N/A");

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5)
                                    .AlignRight().Text($"${sale.TotalAmount:N2}");
                            }
                        });
                    }

                    // Notes section
                    if (!string.IsNullOrWhiteSpace(cashRegister.Notes))
                    {
                        column.Item().PaddingTop(20).Text("NOTAS")
                            .FontSize(12)
                            .Bold();

                        column.Item().PaddingTop(5).Text(cashRegister.Notes)
                            .FontSize(9);
                    }
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().PaddingTop(30).LineHorizontal(1);

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().AlignCenter().Text("Firma del Cajero")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(50); // Spacing

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().AlignCenter().Text("Firma del Supervisor")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    column.Item().PaddingTop(20).AlignCenter()
                        .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .FontSize(7)
                        .Italic()
                        .FontColor(Colors.Grey.Medium);
                });
            }

            void AddSummaryRow(TableDescriptor table, string label, decimal value)
            {
                table.Cell().Padding(5).Text(label);
                table.Cell().Padding(5).AlignRight().Text($"${value:N2}");
            }

            void AddStatRow(TableDescriptor table, string label, string value)
            {
                table.Cell().Padding(5).Text(label);
                table.Cell().Padding(5).AlignRight().Text(value);
            }
        }).GeneratePdf();

        return pdfBytes;
    }
}
