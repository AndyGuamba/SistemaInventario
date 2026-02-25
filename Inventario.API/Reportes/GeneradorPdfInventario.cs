using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Inventario.Modelos.Entidades;

namespace Inventario.API.Reportes
{
    public static class GeneradorPdfInventario
    {
        public static byte[] Generar(IEnumerable<Parabrisa> listaParabrisas)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, listaParabrisas));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Reporte de Inventario").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                    column.Item().Text("Sistema de Gestión de Parabrisas").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, IEnumerable<Parabrisa> listaParabrisas)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(5);

                column.Item().Table(table =>
                {
                    // Definimos 6 columnas para que encajen con tu modelo
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // ID
                        columns.RelativeColumn(3);   // Modelo
                        columns.RelativeColumn(2);   // Marca
                        columns.RelativeColumn(2);   // Tipo
                        columns.RelativeColumn(1);   // Stock
                        columns.RelativeColumn(2);   // Precio
                    });

                    // Cabeceras de la tabla
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("ID").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Modelo").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Marca").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Tipo").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Stock").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Precio").SemiBold().FontColor(Colors.White);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black).Background(Colors.Blue.Darken2).AlignCenter();
                        }
                    });

                    // Llenado con los datos EXACTOS de tu clase Parabrisa
                    foreach (var item in listaParabrisas)
                    {
                        table.Cell().Element(BlockStyle).Text(item.Id.ToString());
                        table.Cell().Element(BlockStyle).Text(item.Modelo ?? "N/A");
                        // OJO: Asumo que en tu clase Marca tienes una propiedad MarcaVehiculo (por el código anterior que me pasaste)
                        table.Cell().Element(BlockStyle).Text(item.Marca?.MarcaVehiculo ?? "N/A");
                        table.Cell().Element(BlockStyle).Text(item.Tipo ?? "N/A");
                        table.Cell().Element(BlockStyle).Text(item.Stock.ToString());
                        table.Cell().Element(BlockStyle).Text($"${item.Precio:F2}");

                        static IContainer BlockStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter();
                        }
                    }
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }
    }
}