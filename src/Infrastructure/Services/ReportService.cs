using Application.Interfaces.Services;
using Application.UseCases.Reports.DTOs;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services;

/// <summary>
/// Servicio para generación de reportes en formatos PDF y Excel.
/// </summary>
public class ReportService : IReportService
{
    private readonly IConfiguration _configuration;

    public ReportService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Configurar licencia de QuestPDF (Community license)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Sales Report

    public async Task<byte[]> GenerateSalesReportPdfAsync(
        SalesReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        var businessName = _configuration["BusinessInfo:Name"] ?? "Super POS";

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });

            void ComposeHeader(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().AlignCenter().Text(businessName)
                        .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().Text(reportData.ReportTitle)
                        .FontSize(14).SemiBold();

                    column.Item().AlignCenter().Text($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });
            }

            void ComposeContent(IContainer container)
            {
                container.Column(column =>
                {
                    // RESUMEN GENERAL
                    column.Item().PaddingTop(15).Text("RESUMEN GENERAL")
                        .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        AddSummaryRow(table, "Total de Ventas:", reportData.Summary.TotalSales.ToString());
                        AddSummaryRow(table, "Ingresos Totales:", $"${reportData.Summary.TotalRevenue:N2}");
                        AddSummaryRow(table, "Ticket Promedio:", $"${reportData.Summary.AverageTicketSize:N2}");
                        AddSummaryRow(table, "Artículos Vendidos:", reportData.Summary.TotalItemsSold.ToString());
                        AddSummaryRow(table, "Clientes Atendidos:", reportData.Summary.TotalCustomers.ToString());
                        AddSummaryRow(table, "Venta Más Alta:", $"${reportData.Summary.HighestSale:N2}");
                        AddSummaryRow(table, "Venta Más Baja:", $"${reportData.Summary.LowestSale:N2}");
                    });

                    // COMPARACIÓN CON PERÍODO ANTERIOR
                    if (reportData.Comparison != null)
                    {
                        column.Item().PaddingTop(15).Text("COMPARACIÓN CON PERÍODO ANTERIOR")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            var revenueChange = reportData.Comparison.RevenueChangePercent;
                            var salesChange = reportData.Comparison.SalesCountChangePercent;
                            var ticketChange = reportData.Comparison.AvgTicketChangePercent;

                            AddComparisonRow(table, "Cambio en Ingresos:", revenueChange);
                            AddComparisonRow(table, "Cambio en Ventas:", salesChange);
                            AddComparisonRow(table, "Cambio en Ticket Promedio:", ticketChange);
                        });
                    }

                    // TOP PRODUCTOS
                    if (reportData.TopProducts.Any())
                    {
                        column.Item().PaddingTop(15).Text($"TOP {reportData.TopProducts.Count} PRODUCTOS MÁS VENDIDOS")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Cantidad").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Ingresos").SemiBold();
                            });

                            int rank = 1;
                            foreach (var product in reportData.TopProducts)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(rank++.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(product.ProductName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(product.QuantitySold.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${product.TotalRevenue:N2}");
                            }
                        });
                    }

                    // TOP CLIENTES
                    if (reportData.TopCustomers.Any())
                    {
                        column.Item().PaddingTop(15).Text($"TOP {reportData.TopCustomers.Count} CLIENTES MÁS FRECUENTES")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cliente").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Visitas").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total Gastado").SemiBold();
                            });

                            int rank = 1;
                            foreach (var customer in reportData.TopCustomers)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(rank++.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(customer.CustomerName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(customer.PurchaseCount.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${customer.TotalSpent:N2}");
                            }
                        });
                    }

                    // TENDENCIAS HORARIAS
                    if (reportData.HourlyTrends.Any())
                    {
                        column.Item().PageBreak();
                        column.Item().PaddingTop(15).Text("TENDENCIAS DE VENTAS POR HORA")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Hora").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Ventas").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Ingresos").SemiBold();
                            });

                            foreach (var hourly in reportData.HourlyTrends.OrderBy(h => h.Hour))
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text($"{hourly.Hour:00}:00 - {hourly.Hour:00}:59");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(hourly.SalesCount.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${hourly.TotalRevenue:N2}");
                            }
                        });
                    }
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Página ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.Span(" de ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
            }

            void AddSummaryRow(TableDescriptor table, string label, string value)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(label).SemiBold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text(value);
            }

            void AddComparisonRow(TableDescriptor table, string label, decimal percentage)
            {
                var color = percentage >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                var symbol = percentage >= 0 ? "↑" : "↓";

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(label).SemiBold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{symbol} {Math.Abs(percentage):N2}%")
                    .FontColor(color).SemiBold();
            }

        }).GeneratePdf();

        return await Task.FromResult(pdfBytes);
    }

    public async Task<byte[]> GenerateSalesReportExcelAsync(
        SalesReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        // Hoja 1: Resumen
        var summarySheet = workbook.Worksheets.Add("Resumen");
        summarySheet.Cell("A1").Value = reportData.ReportTitle;
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A1").Style.Font.FontSize = 14;
        summarySheet.Cell("A2").Value = $"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}";

        summarySheet.Cell("A4").Value = "Métrica";
        summarySheet.Cell("B4").Value = "Valor";
        summarySheet.Range("A4:B4").Style.Font.Bold = true;
        summarySheet.Range("A4:B4").Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        summarySheet.Cell(row, 1).Value = "Total de Ventas";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalSales;

        summarySheet.Cell(row, 1).Value = "Ingresos Totales";
        summarySheet.Cell(row, 2).Value = reportData.Summary.TotalRevenue;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Ticket Promedio";
        summarySheet.Cell(row, 2).Value = reportData.Summary.AverageTicketSize;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Artículos Vendidos";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalItemsSold;

        summarySheet.Cell(row, 1).Value = "Clientes Atendidos";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalCustomers;

        summarySheet.Cell(row, 1).Value = "Venta Más Alta";
        summarySheet.Cell(row, 2).Value = reportData.Summary.HighestSale;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Venta Más Baja";
        summarySheet.Cell(row, 2).Value = reportData.Summary.LowestSale;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        // Comparación
        if (reportData.Comparison != null)
        {
            row += 2;
            summarySheet.Cell(row++, 1).Value = "COMPARACIÓN CON PERÍODO ANTERIOR";
            summarySheet.Cell(row - 1, 1).Style.Font.Bold = true;

            summarySheet.Cell(row, 1).Value = "Cambio en Ingresos";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.RevenueChangePercent:N2}%";

            summarySheet.Cell(row, 1).Value = "Cambio en Ventas";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.SalesCountChangePercent:N2}%";

            summarySheet.Cell(row, 1).Value = "Cambio en Ticket Promedio";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.AvgTicketChangePercent:N2}%";
        }

        summarySheet.Columns().AdjustToContents();

        // Hoja 2: Top Productos
        var productsSheet = workbook.Worksheets.Add("Top Productos");
        productsSheet.Cell("A1").Value = "Ranking";
        productsSheet.Cell("B1").Value = "Producto";
        productsSheet.Cell("C1").Value = "Cantidad Vendida";
        productsSheet.Cell("D1").Value = "Ingresos";
        productsSheet.Range("A1:D1").Style.Font.Bold = true;
        productsSheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        int rank = 1;
        foreach (var product in reportData.TopProducts)
        {
            productsSheet.Cell(row, 1).Value = rank++;
            productsSheet.Cell(row, 2).Value = product.ProductName;
            productsSheet.Cell(row, 3).Value = product.QuantitySold;
            productsSheet.Cell(row, 4).Value = product.TotalRevenue;
            productsSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        productsSheet.Columns().AdjustToContents();

        // Hoja 3: Top Clientes
        var customersSheet = workbook.Worksheets.Add("Top Clientes");
        customersSheet.Cell("A1").Value = "Ranking";
        customersSheet.Cell("B1").Value = "Cliente";
        customersSheet.Cell("C1").Value = "Visitas";
        customersSheet.Cell("D1").Value = "Total Gastado";
        customersSheet.Range("A1:D1").Style.Font.Bold = true;
        customersSheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        rank = 1;
        foreach (var customer in reportData.TopCustomers)
        {
            customersSheet.Cell(row, 1).Value = rank++;
            customersSheet.Cell(row, 2).Value = customer.CustomerName;
            customersSheet.Cell(row, 3).Value = customer.PurchaseCount;
            customersSheet.Cell(row, 4).Value = customer.TotalSpent;
            customersSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        customersSheet.Columns().AdjustToContents();

        // Hoja 4: Tendencias Horarias
        var hourlySheet = workbook.Worksheets.Add("Tendencias Horarias");
        hourlySheet.Cell("A1").Value = "Hora";
        hourlySheet.Cell("B1").Value = "Ventas";
        hourlySheet.Cell("C1").Value = "Ingresos";
        hourlySheet.Range("A1:C1").Style.Font.Bold = true;
        hourlySheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        foreach (var hourly in reportData.HourlyTrends.OrderBy(h => h.Hour))
        {
            hourlySheet.Cell(row, 1).Value = $"{hourly.Hour:00}:00 - {hourly.Hour:00}:59";
            hourlySheet.Cell(row, 2).Value = hourly.SalesCount;
            hourlySheet.Cell(row, 3).Value = hourly.TotalRevenue;
            hourlySheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        hourlySheet.Columns().AdjustToContents();

        // Hoja 5: Ventas Detalladas (si se incluyen)
        if (reportData.DetailedSales != null && reportData.DetailedSales.Any())
        {
            var detailSheet = workbook.Worksheets.Add("Ventas Detalladas");
            detailSheet.Cell("A1").Value = "Folio";
            detailSheet.Cell("B1").Value = "Fecha";
            detailSheet.Cell("C1").Value = "Cliente";
            detailSheet.Cell("D1").Value = "Vendedor";
            detailSheet.Cell("E1").Value = "Productos";
            detailSheet.Cell("F1").Value = "Artículos";
            detailSheet.Cell("G1").Value = "Subtotal";
            detailSheet.Cell("H1").Value = "IVA";
            detailSheet.Cell("I1").Value = "Total";
            detailSheet.Range("A1:I1").Style.Font.Bold = true;
            detailSheet.Range("A1:I1").Style.Fill.BackgroundColor = XLColor.LightGray;

            row = 2;
            foreach (var sale in reportData.DetailedSales)
            {
                detailSheet.Cell(row, 1).Value = sale.SaleId.ToString().Substring(0, 8).ToUpper();
                detailSheet.Cell(row, 2).Value = sale.Date;
                detailSheet.Cell(row, 2).Style.NumberFormat.Format = "dd/MM/yyyy HH:mm";
                detailSheet.Cell(row, 3).Value = sale.CustomerName;
                detailSheet.Cell(row, 4).Value = sale.UserName;
                detailSheet.Cell(row, 5).Value = sale.ProductsCount;
                detailSheet.Cell(row, 6).Value = sale.TotalItems;
                detailSheet.Cell(row, 7).Value = sale.Subtotal;
                detailSheet.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                detailSheet.Cell(row, 8).Value = sale.Tax;
                detailSheet.Cell(row, 8).Style.NumberFormat.Format = "$#,##0.00";
                detailSheet.Cell(row, 9).Value = sale.Total;
                detailSheet.Cell(row, 9).Style.NumberFormat.Format = "$#,##0.00";
                row++;
            }

            detailSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    #endregion

    #region Inventory Report

    public async Task<byte[]> GenerateInventoryReportPdfAsync(
        InventoryReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        var businessName = _configuration["BusinessInfo:Name"] ?? "Super POS";

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });

            void ComposeHeader(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().AlignCenter().Text(businessName)
                        .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().Text(reportData.ReportTitle)
                        .FontSize(14).SemiBold();

                    column.Item().AlignCenter().Text($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });
            }

            void ComposeContent(IContainer container)
            {
                container.Column(column =>
                {
                    // RESUMEN
                    column.Item().PaddingTop(15).Text("RESUMEN DE INVENTARIO")
                        .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        AddSummaryRow(table, "Total de Productos:", reportData.TotalProducts.ToString());
                        AddSummaryRow(table, "Total en Stock:", reportData.TotalStock.ToString());
                        AddSummaryRow(table, "Valor Total:", $"${reportData.TotalValue:N2}");
                        AddSummaryRow(table, "Productos con Stock Bajo:", reportData.LowStockCount.ToString());
                        AddSummaryRow(table, "Productos Agotados:", reportData.OutOfStockCount.ToString());
                    });

                    // PRODUCTOS AGOTADOS
                    if (reportData.OutOfStockItems.Any())
                    {
                        column.Item().PaddingTop(15).Text("PRODUCTOS AGOTADOS")
                            .FontSize(12).Bold().FontColor(Colors.Red.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Red.Lighten3).Padding(5).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Red.Lighten3).Padding(5).Text("Código de Barras").SemiBold();
                                header.Cell().Background(Colors.Red.Lighten3).Padding(5).AlignRight().Text("Precio").SemiBold();
                            });

                            foreach (var item in reportData.OutOfStockItems)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.ProductName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.Barcode);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${item.Price:N2}");
                            }
                        });
                    }

                    // PRODUCTOS CON STOCK BAJO
                    if (reportData.LowStockItems.Any())
                    {
                        column.Item().PaddingTop(15).Text("PRODUCTOS CON STOCK BAJO")
                            .FontSize(12).Bold().FontColor(Colors.Orange.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Orange.Lighten3).Padding(5).Text("Código de Barras").SemiBold();
                                header.Cell().Background(Colors.Orange.Lighten3).Padding(5).AlignRight().Text("Stock").SemiBold();
                                header.Cell().Background(Colors.Orange.Lighten3).Padding(5).AlignRight().Text("Precio").SemiBold();
                            });

                            foreach (var item in reportData.LowStockItems)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.ProductName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(item.Barcode);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(item.Stock.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${item.Price:N2}");
                            }
                        });
                    }
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Página ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.Span(" de ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
            }

            void AddSummaryRow(TableDescriptor table, string label, string value)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(label).SemiBold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text(value);
            }

        }).GeneratePdf();

        return await Task.FromResult(pdfBytes);
    }

    public async Task<byte[]> GenerateInventoryReportExcelAsync(
        InventoryReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        // Hoja 1: Resumen
        var summarySheet = workbook.Worksheets.Add("Resumen");
        summarySheet.Cell("A1").Value = reportData.ReportTitle;
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A1").Style.Font.FontSize = 14;
        summarySheet.Cell("A2").Value = $"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}";

        summarySheet.Cell("A4").Value = "Métrica";
        summarySheet.Cell("B4").Value = "Valor";
        summarySheet.Range("A4:B4").Style.Font.Bold = true;
        summarySheet.Range("A4:B4").Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        summarySheet.Cell(row, 1).Value = "Total de Productos";
        summarySheet.Cell(row++, 2).Value = reportData.TotalProducts;

        summarySheet.Cell(row, 1).Value = "Total en Stock";
        summarySheet.Cell(row++, 2).Value = reportData.TotalStock;

        summarySheet.Cell(row, 1).Value = "Valor Total";
        summarySheet.Cell(row, 2).Value = reportData.TotalValue;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Productos con Stock Bajo";
        summarySheet.Cell(row++, 2).Value = reportData.LowStockCount;

        summarySheet.Cell(row, 1).Value = "Productos Agotados";
        summarySheet.Cell(row++, 2).Value = reportData.OutOfStockCount;

        summarySheet.Columns().AdjustToContents();

        // Hoja 2: Stock Bajo
        var lowStockSheet = workbook.Worksheets.Add("Stock Bajo");
        lowStockSheet.Cell("A1").Value = "Producto";
        lowStockSheet.Cell("B1").Value = "Código de Barras";
        lowStockSheet.Cell("C1").Value = "Stock";
        lowStockSheet.Cell("D1").Value = "Precio";
        lowStockSheet.Cell("E1").Value = "Valor Total";
        lowStockSheet.Range("A1:E1").Style.Font.Bold = true;
        lowStockSheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.Orange;

        row = 2;
        foreach (var item in reportData.LowStockItems)
        {
            lowStockSheet.Cell(row, 1).Value = item.ProductName;
            lowStockSheet.Cell(row, 2).Value = item.Barcode;
            lowStockSheet.Cell(row, 3).Value = item.Stock;
            lowStockSheet.Cell(row, 4).Value = item.Price;
            lowStockSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            lowStockSheet.Cell(row, 5).Value = item.TotalValue;
            lowStockSheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        lowStockSheet.Columns().AdjustToContents();

        // Hoja 3: Agotados
        var outOfStockSheet = workbook.Worksheets.Add("Agotados");
        outOfStockSheet.Cell("A1").Value = "Producto";
        outOfStockSheet.Cell("B1").Value = "Código de Barras";
        outOfStockSheet.Cell("C1").Value = "Precio";
        outOfStockSheet.Range("A1:C1").Style.Font.Bold = true;
        outOfStockSheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.Red;

        row = 2;
        foreach (var item in reportData.OutOfStockItems)
        {
            outOfStockSheet.Cell(row, 1).Value = item.ProductName;
            outOfStockSheet.Cell(row, 2).Value = item.Barcode;
            outOfStockSheet.Cell(row, 3).Value = item.Price;
            outOfStockSheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        outOfStockSheet.Columns().AdjustToContents();

        // Hoja 4: Todos los Productos (si se incluyen)
        if (reportData.AllItems != null && reportData.AllItems.Any())
        {
            var allItemsSheet = workbook.Worksheets.Add("Todos los Productos");
            allItemsSheet.Cell("A1").Value = "Producto";
            allItemsSheet.Cell("B1").Value = "Código de Barras";
            allItemsSheet.Cell("C1").Value = "Stock";
            allItemsSheet.Cell("D1").Value = "Precio";
            allItemsSheet.Cell("E1").Value = "Valor Total";
            allItemsSheet.Cell("F1").Value = "Estado";
            allItemsSheet.Range("A1:F1").Style.Font.Bold = true;
            allItemsSheet.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.LightGray;

            row = 2;
            foreach (var item in reportData.AllItems)
            {
                allItemsSheet.Cell(row, 1).Value = item.ProductName;
                allItemsSheet.Cell(row, 2).Value = item.Barcode;
                allItemsSheet.Cell(row, 3).Value = item.Stock;
                allItemsSheet.Cell(row, 4).Value = item.Price;
                allItemsSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                allItemsSheet.Cell(row, 5).Value = item.TotalValue;
                allItemsSheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";

                string status = item.IsOutOfStock ? "Agotado" : item.IsLowStock ? "Stock Bajo" : "Normal";
                allItemsSheet.Cell(row, 6).Value = status;

                if (item.IsOutOfStock)
                    allItemsSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
                else if (item.IsLowStock)
                    allItemsSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightYellow;

                row++;
            }

            allItemsSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    #endregion

    #region Performance Report

    public async Task<byte[]> GeneratePerformanceReportPdfAsync(
        PerformanceReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        var businessName = _configuration["BusinessInfo:Name"] ?? "Super POS";

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });

            void ComposeHeader(IContainer container)
            {
                container.Column(column =>
                {
                    column.Item().AlignCenter().Text(businessName)
                        .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().Text(reportData.ReportTitle)
                        .FontSize(14).SemiBold();

                    column.Item().AlignCenter().Text($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });
            }

            void ComposeContent(IContainer container)
            {
                container.Column(column =>
                {
                    // RESUMEN DE RENDIMIENTO
                    column.Item().PaddingTop(15).Text("RESUMEN DE RENDIMIENTO")
                        .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        AddSummaryRow(table, "Total de Ventas:", reportData.Summary.TotalSales.ToString());
                        AddSummaryRow(table, "Ingresos Totales:", $"${reportData.Summary.TotalRevenue:N2}");
                        AddSummaryRow(table, "Ticket Promedio:", $"${reportData.Summary.AverageTicketSize:N2}");
                        AddSummaryRow(table, "Artículos Vendidos:", reportData.Summary.TotalItemsSold.ToString());
                        AddSummaryRow(table, "Clientes Atendidos:", reportData.Summary.TotalCustomers.ToString());
                    });

                    // COMPARACIÓN
                    if (reportData.Comparison != null)
                    {
                        column.Item().PaddingTop(15).Text("COMPARACIÓN CON PERÍODO ANTERIOR")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            AddComparisonRow(table, "Cambio en Ingresos:", reportData.Comparison.RevenueChangePercent);
                            AddComparisonRow(table, "Cambio en Ventas:", reportData.Comparison.SalesCountChangePercent);
                            AddComparisonRow(table, "Cambio en Ticket Promedio:", reportData.Comparison.AvgTicketChangePercent);
                        });
                    }

                    // TOP PRODUCTOS
                    if (reportData.TopProducts.Any())
                    {
                        column.Item().PaddingTop(15).Text($"TOP {reportData.TopProducts.Count} PRODUCTOS")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Producto").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Cantidad").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Ingresos").SemiBold();
                            });

                            int rank = 1;
                            foreach (var product in reportData.TopProducts)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(rank++.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(product.ProductName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(product.QuantitySold.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${product.TotalRevenue:N2}");
                            }
                        });
                    }

                    // TOP CLIENTES
                    if (reportData.TopCustomers.Any())
                    {
                        column.Item().PaddingTop(15).Text($"TOP {reportData.TopCustomers.Count} CLIENTES")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Cliente").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Visitas").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total Gastado").SemiBold();
                            });

                            int rank = 1;
                            foreach (var customer in reportData.TopCustomers)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(rank++.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(customer.CustomerName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text(customer.PurchaseCount.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .AlignRight().Text($"${customer.TotalSpent:N2}");
                            }
                        });
                    }
                });
            }

            void ComposeFooter(IContainer container)
            {
                container.AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Página ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.Span(" de ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
            }

            void AddSummaryRow(TableDescriptor table, string label, string value)
            {
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(label).SemiBold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text(value);
            }

            void AddComparisonRow(TableDescriptor table, string label, decimal percentage)
            {
                var color = percentage >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                var symbol = percentage >= 0 ? "↑" : "↓";

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Text(label).SemiBold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{symbol} {Math.Abs(percentage):N2}%")
                    .FontColor(color).SemiBold();
            }

        }).GeneratePdf();

        return await Task.FromResult(pdfBytes);
    }

    public async Task<byte[]> GeneratePerformanceReportExcelAsync(
        PerformanceReportDTO reportData,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        // Hoja 1: Resumen
        var summarySheet = workbook.Worksheets.Add("Resumen");
        summarySheet.Cell("A1").Value = reportData.ReportTitle;
        summarySheet.Cell("A1").Style.Font.Bold = true;
        summarySheet.Cell("A1").Style.Font.FontSize = 14;
        summarySheet.Cell("A2").Value = $"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}";

        summarySheet.Cell("A4").Value = "Métrica";
        summarySheet.Cell("B4").Value = "Valor";
        summarySheet.Range("A4:B4").Style.Font.Bold = true;
        summarySheet.Range("A4:B4").Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        summarySheet.Cell(row, 1).Value = "Total de Ventas";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalSales;

        summarySheet.Cell(row, 1).Value = "Ingresos Totales";
        summarySheet.Cell(row, 2).Value = reportData.Summary.TotalRevenue;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Ticket Promedio";
        summarySheet.Cell(row, 2).Value = reportData.Summary.AverageTicketSize;
        summarySheet.Cell(row++, 2).Style.NumberFormat.Format = "$#,##0.00";

        summarySheet.Cell(row, 1).Value = "Artículos Vendidos";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalItemsSold;

        summarySheet.Cell(row, 1).Value = "Clientes Atendidos";
        summarySheet.Cell(row++, 2).Value = reportData.Summary.TotalCustomers;

        // Comparación
        if (reportData.Comparison != null)
        {
            row += 2;
            summarySheet.Cell(row++, 1).Value = "COMPARACIÓN CON PERÍODO ANTERIOR";
            summarySheet.Cell(row - 1, 1).Style.Font.Bold = true;

            summarySheet.Cell(row, 1).Value = "Cambio en Ingresos";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.RevenueChangePercent:N2}%";

            summarySheet.Cell(row, 1).Value = "Cambio en Ventas";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.SalesCountChangePercent:N2}%";

            summarySheet.Cell(row, 1).Value = "Cambio en Ticket Promedio";
            summarySheet.Cell(row++, 2).Value = $"{reportData.Comparison.AvgTicketChangePercent:N2}%";
        }

        summarySheet.Columns().AdjustToContents();

        // Hoja 2: Top Productos
        var productsSheet = workbook.Worksheets.Add("Top Productos");
        productsSheet.Cell("A1").Value = "Ranking";
        productsSheet.Cell("B1").Value = "Producto";
        productsSheet.Cell("C1").Value = "Cantidad Vendida";
        productsSheet.Cell("D1").Value = "Ingresos";
        productsSheet.Range("A1:D1").Style.Font.Bold = true;
        productsSheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        int rank = 1;
        foreach (var product in reportData.TopProducts)
        {
            productsSheet.Cell(row, 1).Value = rank++;
            productsSheet.Cell(row, 2).Value = product.ProductName;
            productsSheet.Cell(row, 3).Value = product.QuantitySold;
            productsSheet.Cell(row, 4).Value = product.TotalRevenue;
            productsSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        productsSheet.Columns().AdjustToContents();

        // Hoja 3: Top Clientes
        var customersSheet = workbook.Worksheets.Add("Top Clientes");
        customersSheet.Cell("A1").Value = "Ranking";
        customersSheet.Cell("B1").Value = "Cliente";
        customersSheet.Cell("C1").Value = "Visitas";
        customersSheet.Cell("D1").Value = "Total Gastado";
        customersSheet.Range("A1:D1").Style.Font.Bold = true;
        customersSheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;

        row = 2;
        rank = 1;
        foreach (var customer in reportData.TopCustomers)
        {
            customersSheet.Cell(row, 1).Value = rank++;
            customersSheet.Cell(row, 2).Value = customer.CustomerName;
            customersSheet.Cell(row, 3).Value = customer.PurchaseCount;
            customersSheet.Cell(row, 4).Value = customer.TotalSpent;
            customersSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }

        customersSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    #endregion
}
